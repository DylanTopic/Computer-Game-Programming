using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceRace.Rendering
{
    /// <summary>
    /// Procedural torus mesh, generated once and reusable across all rings.
    /// Lies in the XY plane with the hole facing +Z so the ship flies through along Z.
    /// </summary>
    public class TorusPrimitive
    {
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly BasicEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _primitiveCount;

        public TorusPrimitive(GraphicsDevice graphicsDevice,
                              float majorRadius = 10f,
                              float minorRadius = 0.6f,
                              int majorSegments = 48,
                              int minorSegments = 12)
        {
            _graphicsDevice = graphicsDevice;

            int vertexCount = majorSegments * minorSegments;
            int indexCount  = majorSegments * minorSegments * 6;

            var vertices = new VertexPositionColor[vertexCount];
            var indices  = new short[indexCount];

            // ---- Vertices ----
            // Sweep the "tube center" around the major circle (XY plane).
            // At each tube-center sample, sweep a small circle around it to form the tube.
            for (int i = 0; i < majorSegments; i++)
            {
                float majorAngle = MathHelper.TwoPi * i / majorSegments;
                float cosM = MathF.Cos(majorAngle);
                float sinM = MathF.Sin(majorAngle);

                // Tube center, lying in XY plane.
                Vector3 tubeCenter = new Vector3(majorRadius * cosM, majorRadius * sinM, 0f);

                // Local frame at that tube center: outward (radial) and +Z (axial).
                Vector3 outward = new Vector3(cosM, sinM, 0f);
                Vector3 axial   = Vector3.UnitZ;

                for (int j = 0; j < minorSegments; j++)
                {
                    float minorAngle = MathHelper.TwoPi * j / minorSegments;
                    float cosm = MathF.Cos(minorAngle);
                    float sinm = MathF.Sin(minorAngle);

                    // Position on the tube surface.
                    Vector3 pos = tubeCenter + outward * (cosm * minorRadius) + axial * (sinm * minorRadius);

                    // Cheap shading: vary brightness with the minor angle so the tube reads as 3D.
                    float shade = 0.6f + 0.4f * (sinm * 0.5f + 0.5f);
                    Color c = new Color(shade, shade, shade);

                    vertices[i * minorSegments + j] = new VertexPositionColor(pos, c);
                }
            }

            // ---- Indices: stitch quads between adjacent (i, j), (i+1, j), (i, j+1), (i+1, j+1) ----
            int idx = 0;
            for (int i = 0; i < majorSegments; i++)
            {
                int iNext = (i + 1) % majorSegments;
                for (int j = 0; j < minorSegments; j++)
                {
                    int jNext = (j + 1) % minorSegments;

                    short a = (short)(i     * minorSegments + j);
                    short b = (short)(iNext * minorSegments + j);
                    short c = (short)(iNext * minorSegments + jNext);
                    short d = (short)(i     * minorSegments + jNext);

                    indices[idx++] = a; indices[idx++] = b; indices[idx++] = c;
                    indices[idx++] = a; indices[idx++] = c; indices[idx++] = d;
                }
            }

            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertexCount, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _primitiveCount = indexCount / 3;

            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
            };
        }

        /// <summary>
        /// Draw with a tint. Multiplies the per-vertex shading by the tint color.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection, Color tint)
        {
            _effect.World = world;
            _effect.View = view;
            _effect.Projection = projection;
            _effect.DiffuseColor = tint.ToVector3();

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