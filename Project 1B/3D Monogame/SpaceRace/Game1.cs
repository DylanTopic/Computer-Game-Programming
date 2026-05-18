using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using SpaceRace.Gameplay;
using SpaceRace.Physics;
using SpaceRace.Rendering;


using NumericsVector3 = System.Numerics.Vector3;
using System;

namespace SpaceRace
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Simulation _simulation;
        private BufferPool _bufferPool;
        private ThreadDispatcher _threadDispatcher;

        private Camera _camera;
        private CubePrimitive _cubeMesh;
        private Ship _ship;

        private TorusPrimitive _ringMesh;

        private CourseManager _course;

        private Skybox _skybox;
        private SpriteFont _font;

        private ShipPrimitive _shipMesh;
        private SpaceRace.UI.Hud _hud;
        private KeyboardState _previousKb;
        private const float FixedDt = 1f / 60f;
        private float _physicsAccumulator;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Bepu setup
            _bufferPool = new BufferPool();
            int cores = System.Environment.ProcessorCount;
            int targetThreadCount = System.Math.Max(1, cores > 4 ? cores - 2 : cores - 1);
            _threadDispatcher = new ThreadDispatcher(targetThreadCount);

            _simulation = Simulation.Create(
                _bufferPool,
                new NarrowPhaseCallbacks { ContactSpringiness = new SpringSettings(30, 1) },
                new PoseIntegratorCallbacks(
                    gravity: new NumericsVector3(0, 0, 0),
                    linearDamping: 0.5f,   
                      angularDamping: 0.6f),
                new SolveDescription(velocityIterationCount: 8, substepCount: 1));

            // Camera
            _camera = new Camera
            {
                Position = new Vector3(0, 8, 25),
                Target = Vector3.Zero,
                AspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight,
            };

            // Ship 
            _ship = Ship.Create(_simulation, new NumericsVector3(0, 0, 0));
            _camera.SnapTo(_ship);

            var rings = new System.Collections.Generic.List<Ring>
            {
                new Ring(_simulation, new Vector3(   0,   0,  -40), Quaternion.Identity),
                new Ring(_simulation, new Vector3(  15,   5,  -90), Quaternion.CreateFromAxisAngle(Vector3.Up,        MathF.PI / 4f)),
                new Ring(_simulation, new Vector3(   0,  10, -150), Quaternion.CreateFromAxisAngle(Vector3.Right,    -MathF.PI / 8f)),
                new Ring(_simulation, new Vector3( -20,   0, -210), Quaternion.CreateFromAxisAngle(Vector3.Up,       -MathF.PI / 4f)),
                new Ring(_simulation, new Vector3( -10, -10, -270), Quaternion.CreateFromAxisAngle(Vector3.Right,     MathF.PI / 8f)),
                new Ring(_simulation, new Vector3(  10,   5, -340), Quaternion.CreateFromAxisAngle(Vector3.Forward,   MathF.PI / 4f)),
                new Ring(_simulation, new Vector3(   0,   0, -410), Quaternion.Identity),
            };

            _course = new CourseManager(rings);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _skybox = new Skybox(GraphicsDevice, scale: 2000f, textureSize: 1024);
            _cubeMesh = new CubePrimitive(GraphicsDevice, size: 2f, color: Color.OrangeRed);
            _shipMesh = new ShipPrimitive(GraphicsDevice);
            _ringMesh = new TorusPrimitive(GraphicsDevice, majorRadius: 10f, minorRadius: 0.6f);
            _font = Content.Load<SpriteFont>("Hud");
            _hud = new SpaceRace.UI.Hud(GraphicsDevice, _spriteBatch, _font);
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                kb.IsKeyDown(Keys.Escape))
                Exit();

            // Restart on R
            if (kb.IsKeyDown(Keys.R) && !_previousKb.IsKeyDown(Keys.R))
                ResetGame();

            float frameDt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _physicsAccumulator += frameDt;
            if (_physicsAccumulator > 0.25f) _physicsAccumulator = 0.25f;

            while (_physicsAccumulator >= FixedDt)
            {
                _ship.ApplyInput(kb, FixedDt);
                _simulation.Timestep(FixedDt, _threadDispatcher);
                _course.Update(_ship, FixedDt);
                _physicsAccumulator -= FixedDt;
            }

            // Camera smoothing 
            if (frameDt > 0f) _camera.UpdateChase(_ship, frameDt);

            _previousKb = kb;
            base.Update(gameTime);
        }
        private void ResetGame()
        {
            // Snap the ship back to the start with zero velocity
            var bodyRef = _simulation.Bodies[_ship.BodyHandle];
            bodyRef.Pose = new BepuPhysics.RigidPose(
                new NumericsVector3(0, 0, 0),
                System.Numerics.Quaternion.Identity);
            bodyRef.Velocity = default; // zero linear and angular
            bodyRef.Awake = true;

            _course.Reset();
            _camera.SnapTo(_ship);
            _physicsAccumulator = 0f;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _skybox.Draw(_camera.Position, _camera.View, _camera.Projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (var ring in _course.Rings)
                ring.Draw(_ringMesh, _camera.View, _camera.Projection, gameTime);

            _shipMesh.Draw(_ship.WorldMatrix, _camera.View, _camera.Projection);

            _hud.Draw(_course, _ship);

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _simulation.Dispose();
            _threadDispatcher.Dispose();
            _bufferPool.Clear();
            base.UnloadContent();
        }
    }
}