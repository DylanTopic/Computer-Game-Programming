using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf.Components
{
    public class BallComponent : DrawableGameComponent
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _circleTexture;

        public Vector2 Position;
        public int Radius = 8;
        public bool IsMoving = false;
        public Vector2 Velocity = Vector2.Zero;

        // Starting position
        private Vector2 _startPosition = new Vector2(160, 362);

        public BallComponent(Game game) : base(game)
        {
            Position = _startPosition;
        }

        public override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _circleTexture = CreateCircleTexture(Radius);
        }

        public override void Update(GameTime gameTime) { }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_circleTexture,
                new Vector2(Position.X - Radius, Position.Y - Radius),
                Color.White);
            _spriteBatch.End();
        }

        private Texture2D CreateCircleTexture(int radius)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(GraphicsDevice, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    if (dx * dx + dy * dy <= radius * radius)
                        data[y * diameter + x] = Color.White;
                    else
                        data[y * diameter + x] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}