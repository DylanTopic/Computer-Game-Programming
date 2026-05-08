using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf.Components
{
    public class CourseComponent : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        // Course elements XY Width Height
        // Outer walls
        private Rectangle _wallTop = new Rectangle(100, 100, 400, 20);
        private Rectangle _wallBottom = new Rectangle(100, 400, 400, 20);
        private Rectangle _wallLeft = new Rectangle(100, 100, 20, 300);
        private Rectangle _wallRight = new Rectangle(480, 100, 20, 300);

        // Inner walls (Numbers go left to right, bottom to top)
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

        // Hole position (drawn as circle)
        private Vector2 _holePosition = new Vector2(380, 220);
        private int _holeRadius = 15;

        public CourseComponent(Game game) : base(game) { }

        public override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture we can recolor and stretch
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

            // Slope (red = uphill)
            _spriteBatch.Draw(_pixel, _slope, Color.Red);

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
            // Creates a circle texture and draws it
            int diameter = radius * 2;
            Texture2D circleTexture = new Texture2D(GraphicsDevice, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    if (dx * dx + dy * dy <= radius * radius)
                        data[y * diameter + x] = color;
                    else
                        data[y * diameter + x] = Color.Transparent;
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
    }


}