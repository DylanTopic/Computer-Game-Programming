using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MiniGolf.Components;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf.Components
{
    public class PhysicsEngine : GameComponent
    {
        private BallComponent _ball;
        private InputComponent _input;
        private HUDComponent _hud;
        private SoundManager _sounds;

        public List<Rectangle> Walls = new List<Rectangle>();
        public List<Rectangle> Obstacles = new List<Rectangle>();
        public List<(Vector2 start, Vector2 end)> SlantedWalls = new List<(Vector2, Vector2)>();
        public List<(Rectangle region, Vector2 force)> Slopes = new List<(Rectangle, Vector2)>();

        public Vector2 HolePosition = new Vector2(220, 220);
        public float HoleRadius = 15f;
        public bool BallInHole = false;

        private double _bounceTimer = 0;
        private bool _collidedThisFrame = false;
        private const float Friction = 0.985f;
        private const float StopThreshold = 0.1f;
        private const float MaxSpeed = 25f; // cap to prevent insane velocity

        public PhysicsEngine(Game game) : base(game) { }

        public override void Initialize()
        {
            _ball = Game.Components.OfType<BallComponent>().FirstOrDefault();
            _input = Game.Components.OfType<InputComponent>().FirstOrDefault();
            _hud = Game.Components.OfType<HUDComponent>().FirstOrDefault();
            _sounds = Game.Components.OfType<SoundManager>().FirstOrDefault();

            // Outer walls
            Walls.Add(new Rectangle(100, 100, 400, 20));
            Walls.Add(new Rectangle(100, 400, 400, 20));
            Walls.Add(new Rectangle(100, 100, 20, 300));
            Walls.Add(new Rectangle(480, 100, 20, 300));

            // Inner walls
            Walls.Add(new Rectangle(100, 300, 250, 10));
            Walls.Add(new Rectangle(310, 180, 100, 10));
            Walls.Add(new Rectangle(350, 260, 10, 50));
            Walls.Add(new Rectangle(350, 250, 60, 10));
            Walls.Add(new Rectangle(410, 180, 10, 80));

            // Obstacle
            Obstacles.Add(new Rectangle(400, 280, 40, 40));

            // Slope
            Slopes.Add((new Rectangle(420, 220, 30, 60), new Vector2(0, 0.2f)));

            // Hole
            HolePosition = new Vector2(380, 220);

            // Slanted wall
            SlantedWalls.Add((new Vector2(420, 430), new Vector2(485, 320)));

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (_bounceTimer > 0)
                _bounceTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            // Reset check BEFORE the IsMoving early return so R always works
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                BallInHole = false;
                _ball.Position = new Vector2(160, 362);
                _ball.Velocity = Vector2.Zero;
                _ball.IsMoving = false;
                _hud.StrokeCount = 0;
                return;
            }

            if (_input.ShotFired && !_ball.IsMoving && !BallInHole)
            {
                float angle = _input.GetAngleRadians();
                float power = _input.GetPowerMagnitude();

                _ball.Velocity = new Vector2(
                    (float)System.Math.Cos(angle) * power,
                    (float)System.Math.Sin(angle) * power
                );

                // Cap speed on shot
                if (_ball.Velocity.Length() > MaxSpeed)
                    _ball.Velocity = Vector2.Normalize(_ball.Velocity) * MaxSpeed;

                _ball.IsMoving = true;
                _input.ShotFired = false;
                _hud.StrokeCount++;
            }

            if (!_ball.IsMoving) return;

            // Apply slope forces
            foreach (var (region, force) in Slopes)
            {
                if (region.Contains(_ball.Position))
                    _ball.Velocity += force;
            }

            // Apply friction
            _ball.Velocity *= Friction;

            // Substep movement — use _ball.Velocity / substeps directly each step
            // so recalculation after bounce doesn't compound speed
            int substeps = 5;

            for (int i = 0; i < substeps; i++)
            {
                _ball.Position += _ball.Velocity / substeps;

                _collidedThisFrame = false;

                foreach (var wall in Walls)
                    HandleCollision(wall);

                foreach (var obstacle in Obstacles)
                    HandleCollision(obstacle);

                foreach (var (start, end) in SlantedWalls)
                    HandleSlantedCollision(start, end);

                if (_collidedThisFrame && _bounceTimer <= 0)
                {
                    _sounds?.PlayBounce();
                    _bounceTimer = 0.05;
                }
            }

            // Hole detection
            float distToHole = Vector2.Distance(_ball.Position, HolePosition);
            if (distToHole < HoleRadius)
            {
                BallInHole = true;
                _ball.IsMoving = false;
                _ball.Velocity = Vector2.Zero;
                _ball.Position = HolePosition;
                return;
            }

            // Stop ball if nearly still
            if (_ball.Velocity.Length() < StopThreshold)
            {
                _ball.IsMoving = false;
                _ball.Velocity = Vector2.Zero;
            }
        }

        private void HandleCollision(Rectangle rect)
        {
            Rectangle ballRect = new Rectangle(
                (int)(_ball.Position.X - _ball.Radius),
                (int)(_ball.Position.Y - _ball.Radius),
                _ball.Radius * 2,
                _ball.Radius * 2
            );

            if (!ballRect.Intersects(rect)) return;

            _collidedThisFrame = true;

            float overlapLeft   = ballRect.Right - rect.Left;
            float overlapRight  = rect.Right - ballRect.Left;
            float overlapTop    = ballRect.Bottom - rect.Top;
            float overlapBottom = rect.Bottom - ballRect.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            if (minOverlapX < minOverlapY)
            {
                _ball.Velocity = new Vector2(-_ball.Velocity.X * 0.7f, _ball.Velocity.Y);
                _ball.Position += new Vector2(
                    overlapLeft < overlapRight ? -overlapLeft : overlapRight, 0);
            }
            else
            {
                _ball.Velocity = new Vector2(_ball.Velocity.X, -_ball.Velocity.Y * 0.7f);
                _ball.Position += new Vector2(0,
                    overlapTop < overlapBottom ? -overlapTop : overlapBottom);
            }
        }

        private void HandleSlantedCollision(Vector2 start, Vector2 end)
        {
            Vector2 ab = end - start;
            Vector2 ac = _ball.Position - start;
            float t = Vector2.Dot(ac, ab) / Vector2.Dot(ab, ab);
            t = System.Math.Clamp(t, 0f, 1f);
            Vector2 closest = start + t * ab;

            float dist = Vector2.Distance(_ball.Position, closest);
            if (dist < _ball.Radius)
            {
                Vector2 normal = Vector2.Normalize(_ball.Position - closest);
                _ball.Position = closest + normal * (_ball.Radius + 1);

                float dot = Vector2.Dot(_ball.Velocity, normal);
                _ball.Velocity -= 2 * dot * normal;
                _ball.Velocity *= 0.7f;

                _collidedThisFrame = true;
            }
        }
    }
}