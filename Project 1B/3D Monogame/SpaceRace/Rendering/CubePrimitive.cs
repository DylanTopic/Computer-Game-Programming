using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceRace.Rendering
{
    public class CubePrimitive
    {
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly BasicEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _primitiveCount;

        public CubePrimitive(GraphicsDevice graphicsDevice, float size = 1f, Color? color = null)
        {
            _graphicsDevice = graphicsDevice;
            float h = size * 0.5f;
            Color c = color ?? Color.White;

            // 8 corners of the cube. We duplicate per-face later so each face can have its own normal/color shading,
            // but for VertexPositionColor we can share these 8 just fine.
            VertexPositionColor[] vertices = new[]
            {
                new VertexPositionColor(new Vector3(-h, -h, -h), c * 0.6f),
                new VertexPositionColor(new Vector3( h, -h, -h), c * 0.7f),
                new VertexPositionColor(new Vector3( h,  h, -h), c * 0.8f),
                new VertexPositionColor(new Vector3(-h,  h, -h), c * 0.7f),
                new VertexPositionColor(new Vector3(-h, -h,  h), c * 0.7f),
                new VertexPositionColor(new Vector3( h, -h,  h), c * 0.8f),
                new VertexPositionColor(new Vector3( h,  h,  h), c * 1.0f),
                new VertexPositionColor(new Vector3(-h,  h,  h), c * 0.9f),
            };

            // 12 triangles (2 per face), CCW winding when viewed from outside.
            short[] indices = new short[]
            {
                0,2,1, 0,3,2, // back  (-Z)
                4,5,6, 4,6,7, // front (+Z)
                0,4,7, 0,7,3, // left  (-X)
                1,2,6, 1,6,5, // right (+X)
                3,7,6, 3,6,2, // top   (+Y)
                0,1,5, 0,5,4, // bottom(-Y)
            };

            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _primitiveCount = indices.Length / 3;

            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
            };
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            _effect.World = world;
            _effect.View = view;
            _effect.Projection = projection;

            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
            }
        }
    }
}