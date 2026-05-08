using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace MiniGolf.Components
{
    public class InputComponent : GameComponent
    {
        public float Angle = 0f;
        public int PowerLevel = 8;
        public bool ShotFired = false;

        private KeyboardState _prevKeyboard;
        private BallComponent _ball;
        private Point _prevMousePos;

        private const int MaxPower = 16;
        private const float ArrowAngleStep = (float)(2 * Math.PI / 64);

        public InputComponent(Game game) : base(game) { }

        public override void Initialize()
        {
            _ball = Game.Components.OfType<BallComponent>().FirstOrDefault();
            _prevMousePos = Mouse.GetState().Position;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            bool mouseMoving = mouse.Position != _prevMousePos;

            if (mouseMoving && _ball != null)
            {
                // Aim with mouse when it's moving
                Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
                Vector2 direction = mousePos - _ball.Position;

                if (direction.Length() > 5f)
                    Angle = (float)Math.Atan2(direction.Y, direction.X);
            }
            else
            {
                // Aim with arrow keys when mouse is still
                if (WasPressed(kb, _prevKeyboard, Keys.Left))
                    Angle -= ArrowAngleStep;
                if (WasPressed(kb, _prevKeyboard, Keys.Right))
                    Angle += ArrowAngleStep;
            }

            // Adjust power with Up/Down
            if (WasPressed(kb, _prevKeyboard, Keys.Up) && PowerLevel < MaxPower)
                PowerLevel++;
            if (WasPressed(kb, _prevKeyboard, Keys.Down) && PowerLevel > 1)
                PowerLevel--;

            // Fire with Space
            if (WasPressed(kb, _prevKeyboard, Keys.Space))
                ShotFired = true;

            _prevMousePos = mouse.Position;
            _prevKeyboard = kb;
        }

        private bool WasPressed(KeyboardState current, KeyboardState previous, Keys key)
        {
            return current.IsKeyDown(key) && previous.IsKeyUp(key);
        }

        public float GetAngleRadians() => Angle;

        public float GetPowerMagnitude() => PowerLevel * 1.5f;
    }
}