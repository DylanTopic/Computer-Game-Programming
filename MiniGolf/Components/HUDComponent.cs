using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace MiniGolf.Components
{
    public class HUDComponent : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private SpriteFont _font;

        private InputComponent _input;
        private BallComponent _ball;
        private PhysicsEngine _physics;

        public int StrokeCount = 0;

        public HUDComponent(Game game) : base(game) { }

        public override void Initialize()
        {
            _input = Game.Components.OfType<InputComponent>().FirstOrDefault();
            _ball = Game.Components.OfType<BallComponent>().FirstOrDefault();
            _physics = Game.Components.OfType<PhysicsEngine>().FirstOrDefault();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _font = Game.Content.Load<SpriteFont>("GameFont");
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            // Draw aim arrow only when ball is not moving
            if (!_ball.IsMoving && !_physics.BallInHole)
            {
                float angle = _input.GetAngleRadians();
                float length = 40f;
                Vector2 direction = new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle));
                Vector2 arrowEnd = _ball.Position + direction * length;
                DrawLine(_ball.Position, arrowEnd, Color.Yellow);
            }

            // Power bar background
            _spriteBatch.Draw(_pixel, new Rectangle(10, 10, 240, 20), Color.DarkGray);
            // Power bar fill
            int barWidth = _input.PowerLevel * 15;
            _spriteBatch.Draw(_pixel, new Rectangle(10, 10, barWidth, 20), Color.LimeGreen);

            // Power label
            _spriteBatch.DrawString(_font, $"Power: {_input.PowerLevel}/16",
                new Vector2(10, 35), Color.White);

            // Stroke counter
            _spriteBatch.DrawString(_font, $"Strokes: {StrokeCount}",
                new Vector2(10, 60), Color.White);

            // Game state message
            string message = "";
            if (_physics.BallInHole)
                message = $"Hole in {StrokeCount} strokes!";
            else if (_ball.IsMoving)
                message = "...";
            else
                message = "Aim with arrows, power with Up/Down, Space to shoot";

            _spriteBatch.DrawString(_font, message, new Vector2(10, 85), Color.Yellow);

            _spriteBatch.End();
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            float angle = (float)System.Math.Atan2(diff.Y, diff.X);

            _spriteBatch.Draw(_pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, 2),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}