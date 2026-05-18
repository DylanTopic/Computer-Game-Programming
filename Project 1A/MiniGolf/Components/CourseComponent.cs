using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf.Components
{
    public class CourseComponent : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        // Outer walls
        private Rectangle _wallTop = new Rectangle(100, 100, 400, 20);
        private Rectangle _wallBottom = new Rectangle(100, 400, 400, 20);
        private Rectangle _wallLeft = new Rectangle(100, 100, 20, 300);
        private Rectangle _wallRight = new Rectangle(480, 100, 20, 300);

        // Inner walls
        private Rectangle _wallMiddle1 = new Rectangle(100, 300, 250, 10);
        private Rectangle _wallMiddle2 = new Rectangle(310, 180, 100, 10);
        private Rectangle _wallMiddle3 = new Rectangle(350, 260, 10, 50);
        private Rectangle _wallMiddle4 = new Rectangle(350, 250, 60, 10);
        private Rectangle _wallMiddle5 = new Rectangle(410, 180, 10, 80);

        // Tee, slope, and obstacle
        private Rectangle _tee = new Rectangle(140, 370, 40, 15);
        private Rectangle _slope = new Rectangle(420, 220, 60, 40);
        private Rectangle _obstacle = new Rectangle(400, 280, 40, 40);

        // Slanted wall
        private Vector2 _slantStart = new Vector2(490, 330);
        private Vector2 _slantEnd = new Vector2(440, 410);

        // Hole
        private Vector2 _holePosition = new Vector2(380, 220);
        private int _holeRadius = 15;

        public CourseComponent(Game game) : base(game) { }

        public override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            // Outer Walls (black)
            _spriteBatch.Draw(_pixel, _wallTop, Color.Black);
            _spriteBatch.Draw(_pixel, _wallBottom, Color.Black);
            _spriteBatch.Draw(_pixel, _wallLeft, Color.Black);
            _spriteBatch.Draw(_pixel, _wallRight, Color.Black);

            // Inner Walls (black)
            _spriteBatch.Draw(_pixel, _wallMiddle1, Color.Black);
            _spriteBatch.Draw(_pixel, _wallMiddle2, Color.Black);
            _spriteBatch.Draw(_pixel, _wallMiddle3, Color.Black);
            _spriteBatch.Draw(_pixel, _wallMiddle4, Color.Black);
            _spriteBatch.Draw(_pixel, _wallMiddle5, Color.Black);

            // Tee (blue)
            _spriteBatch.Draw(_pixel, _tee, Color.Blue);

            // Slope divided into grid with direction arrows
            int cellSize = 15;
            Vector2 slopeForceDir = Vector2.Normalize(new Vector2(0, 0.2f));

            for (int row = 0; row < _slope.Height / cellSize; row++)
            {
                for (int col = 0; col < _slope.Width / cellSize; col++)
                {
                    Rectangle cell = new Rectangle(
                        _slope.X + col * cellSize,
                        _slope.Y + row * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );
                    _spriteBatch.Draw(_pixel, cell, Color.Red);

                    Vector2 cellCenter = new Vector2(
                        cell.X + cell.Width / 2f,
                        cell.Y + cell.Height / 2f
                    );
                    DrawArrow(cellCenter, slopeForceDir, Color.White);
                }
            }

            // Obstacle (dark gray)
            _spriteBatch.Draw(_pixel, _obstacle, Color.DarkGray);

            // Hole (blue circle)
            DrawCircle(_holePosition, _holeRadius, Color.Blue);

            // Slanted Wall
            DrawSlant(_slantStart, _slantEnd, Color.Black, 8);

            _spriteBatch.End();
        }

        private void DrawCircle(Vector2 center, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D circleTexture = new Texture2D(GraphicsDevice, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    data[y * diameter + x] = (dx * dx + dy * dy <= radius * radius)
                        ? color : Color.Transparent;
                }
            }

            circleTexture.SetData(data);
            _spriteBatch.Draw(circleTexture,
                new Vector2(center.X - radius, center.Y - radius), Color.White);
        }

        private void DrawSlant(Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            float angle = (float)System.Math.Atan2(diff.Y, diff.X);

            _spriteBatch.Draw(_pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        private void DrawArrow(Vector2 center, Vector2 direction, Color color)
        {
            Vector2 start = center - direction * 6;
            Vector2 end = center + direction * 6;
            DrawSlant(start, end - new Vector2(0, 1), color, 2);

            Vector2 perp = new Vector2(-direction.Y, direction.X);
            DrawSlant(end - new Vector2(1, 0), end - direction * 4 + perp * 3 - new Vector2(3, 1), color, 2);
            DrawSlant(end - new Vector2(2, 1), end - direction * 4 - perp * 3 - new Vector2(-2, 3), color, 2);
        }
    }
}