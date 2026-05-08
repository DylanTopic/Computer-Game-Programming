using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceRace.Rendering
{
    /// <summary>
    /// Procedural starfield rendered as tiny triangles on a sphere around the camera.
    /// Translating the geometry to follow the camera each frame makes stars feel
    /// infinitely far — translation no longer changes their apparent position,
    /// only rotation does, which matches real space.
    /// </summary>
    public class Starfield
    {
        private readonly VertexBuffer _vertexBuffer;
        private readonly BasicEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _triangleCount;

        public Starfield(GraphicsDevice graphicsDevice, int starCount = 1500, float distance = 2000f)
        {
            _graphicsDevice = graphicsDevice;
            // Fixed seed → reproducible starfield, easier to tell if a bug is geometry vs rendering.
            var rng = new Random(42);

            int vertexCount = starCount * 3;
            var vertices = new VertexPositionColor[vertexCount];

            for (int i = 0; i < starCount; i++)
            {
                // Uniformly distribute direction on a sphere using the (u, theta) trick:
                // u = cos(phi) uniform in [-1, 1] gives uniform area distribution.
                float u = (float)(rng.NextDouble() * 2.0 - 1.0);
                float theta = (float)(rng.NextDouble() * Math.PI * 2.0);
                float r = MathF.Sqrt(1f - u * u);
                Vector3 dir = new Vector3(r * MathF.Cos(theta), u, r * MathF.Sin(theta));

                Vector3 starCenter = dir * distance;

                // Brightness varies; most stars are white, a fraction are bluish or orangish.
                float brightness = 0.4f + (float)rng.NextDouble() * 0.6f;
                float colorRoll = (float)rng.NextDouble();
                Color c;
                if (colorRoll < 0.75f)
                    c = new Color(brightness, brightness, brightness);                              // white
                else if (colorRoll < 0.9f)
                    c = new Color(brightness * 0.7f, brightness * 0.85f, brightness);               // bluish
                else
                    c = new Color(brightness, brightness * 0.85f, brightness * 0.6f);               // orangish

                // Star size scaled to distance so apparent size is reasonable.
                // A small spread so a few stars look bigger / "closer."
                float starSize = distance * 0.0018f * (0.5f + (float)rng.NextDouble() * 0.7f);

                // Build a local frame perpendicular to dir so the triangle faces outward.
                Vector3 helper = MathF.Abs(Vector3.Dot(dir, Vector3.UnitY)) > 0.99f ? Vector3.UnitX : Vector3.UnitY;
                Vector3 right = Vector3.Normalize(Vector3.Cross(dir, helper));
                Vector3 up    = Vector3.Cross(right, dir);

                // Equilateral-ish triangle with starCenter as centroid.
                vertices[i * 3 + 0] = new VertexPositionColor(starCenter + up * starSize, c);
                vertices[i * 3 + 1] = new VertexPositionColor(starCenter + (right *  0.866f - up * 0.5f) * starSize, c);
                vertices[i * 3 + 2] = new VertexPositionColor(starCenter + (right * -0.866f - up * 0.5f) * starSize, c);
            }

            _triangleCount = starCount;
            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertexCount, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
            };
        }

        /// <summary>
        /// Call FIRST in your Draw, before any world geometry.
        /// </summary>
        public void Draw(Vector3 cameraPosition, Matrix view, Matrix projection)
        {
            // Translate the entire field so it follows the camera. Stars now appear infinitely far.
            _effect.World = Matrix.CreateTranslation(cameraPosition);
            _effect.View = view;
            _effect.Projection = projection;

            // Save current render states so we don't disturb the rest of the frame.
            var savedDepth   = _graphicsDevice.DepthStencilState;
            var savedRaster  = _graphicsDevice.RasterizerState;

            // Skybox state: no depth read, no depth write, draw both faces (triangles are tiny).
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState   = RasterizerState.CullNone;

            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _triangleCount);
            }

            // Restore so subsequent world rendering behaves normally.
            _graphicsDevice.DepthStencilState = savedDepth;
            _graphicsDevice.RasterizerState   = savedRaster;
        }
    }
}