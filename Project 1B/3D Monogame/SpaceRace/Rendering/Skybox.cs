using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceRace.Rendering
{
    public class Skybox
    {
        private readonly GraphicsDevice _gd;
        private readonly BasicEffect _effect;
        private readonly VertexBuffer _vertexBuffer;
        private readonly Texture2D[] _faceTextures;
        private readonly float _scale;

        public Skybox(GraphicsDevice gd, float scale = 2000f, int textureSize = 1024)
        {
            _gd = gd;
            _scale = scale;

            // Six independent procedural starfield textures, one per face.
            _faceTextures = new Texture2D[6];
            var rng = new Random(1337);
            for (int i = 0; i < 6; i++)
                _faceTextures[i] = GenerateStarfieldTexture(textureSize, rng);

            // Build 36 vertices
            var vertices = BuildFaceVertices();
            _vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);

            _effect = new BasicEffect(gd)
            {
                TextureEnabled = true,
                LightingEnabled = false,
                VertexColorEnabled = false,
            };
        }

        private static VertexPositionTexture[] BuildFaceVertices()
        {
            // Each face: 4 corner positions in world space
            // Order matches +X, -X, +Y, -Y, +Z, -Z
            Vector3[][] faces =
            {
                new[] { new Vector3( 1,  1,  1), new Vector3( 1,  1, -1), new Vector3( 1, -1, -1), new Vector3( 1, -1,  1) }, // +X
                new[] { new Vector3(-1,  1, -1), new Vector3(-1,  1,  1), new Vector3(-1, -1,  1), new Vector3(-1, -1, -1) }, // -X
                new[] { new Vector3(-1,  1, -1), new Vector3( 1,  1, -1), new Vector3( 1,  1,  1), new Vector3(-1,  1,  1) }, // +Y
                new[] { new Vector3(-1, -1,  1), new Vector3( 1, -1,  1), new Vector3( 1, -1, -1), new Vector3(-1, -1, -1) }, // -Y
                new[] { new Vector3(-1,  1,  1), new Vector3( 1,  1,  1), new Vector3( 1, -1,  1), new Vector3(-1, -1,  1) }, // +Z
                new[] { new Vector3( 1,  1, -1), new Vector3(-1,  1, -1), new Vector3(-1, -1, -1), new Vector3( 1, -1, -1) }, // -Z
            };

            Vector2 uvTL = new Vector2(0, 0);
            Vector2 uvTR = new Vector2(1, 0);
            Vector2 uvBR = new Vector2(1, 1);
            Vector2 uvBL = new Vector2(0, 1);

            var verts = new VertexPositionTexture[6 * 6];
            for (int f = 0; f < 6; f++)
            {
                Vector3 TL = faces[f][0], TR = faces[f][1], BR = faces[f][2], BL = faces[f][3];
                int b = f * 6;
                verts[b + 0] = new VertexPositionTexture(TL, uvTL);
                verts[b + 1] = new VertexPositionTexture(BL, uvBL);
                verts[b + 2] = new VertexPositionTexture(BR, uvBR);
                verts[b + 3] = new VertexPositionTexture(TL, uvTL);
                verts[b + 4] = new VertexPositionTexture(BR, uvBR);
                verts[b + 5] = new VertexPositionTexture(TR, uvTR);
            }
            return verts;
        }

        private Texture2D GenerateStarfieldTexture(int size, Random rng)
        {
            var pixels = new Color[size * size];

            // Deep space base = nearly black with a faint blue cast, more honest than pure black.
            Color spaceColor = new Color(2, 2, 8);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = spaceColor;

            // Stars = small bright pixels at random positions, with brightness and color variation.
            int starCount = size * size / 280;
            for (int i = 0; i < starCount; i++)
            {
                int x = rng.Next(size);
                int y = rng.Next(size);

                float brightness = 0.4f + (float)rng.NextDouble() * 0.6f;
                float colorRoll = (float)rng.NextDouble();
                Color starColor =
                    colorRoll < 0.70f ? new Color(brightness, brightness, brightness)                           : // white
                    colorRoll < 0.88f ? new Color(brightness * 0.7f, brightness * 0.85f, brightness)            : // bluish
                                        new Color(brightness, brightness * 0.85f, brightness * 0.6f);             // orangish

                pixels[y * size + x] = starColor;

                // ~15% of stars get a small halo for visual variety
                if (rng.NextDouble() < 0.15)
                {
                    float halo = 0.35f;
                    Color haloColor = new Color(
                        (byte)(starColor.R * halo),
                        (byte)(starColor.G * halo),
                        (byte)(starColor.B * halo));
                    SetIfInRange(pixels, x - 1, y, size, haloColor);
                    SetIfInRange(pixels, x + 1, y, size, haloColor);
                    SetIfInRange(pixels, x, y - 1, size, haloColor);
                    SetIfInRange(pixels, x, y + 1, size, haloColor);
                }
            }

            var tex = new Texture2D(_gd, size, size);
            tex.SetData(pixels);
            return tex;
        }

        private static void SetIfInRange(Color[] pixels, int x, int y, int size, Color c)
        {
            if (x < 0 || y < 0 || x >= size || y >= size) return;
            Color existing = pixels[y * size + x];
            pixels[y * size + x] = new Color(
                (byte)Math.Min(255, existing.R + c.R),
                (byte)Math.Min(255, existing.G + c.G),
                (byte)Math.Min(255, existing.B + c.B));
        }

        public void Draw(Vector3 cameraPosition, Matrix view, Matrix projection)
        {
            _effect.World = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(cameraPosition);
            _effect.View = view;
            _effect.Projection = projection;

            var savedDepth   = _gd.DepthStencilState;
            var savedRaster  = _gd.RasterizerState;
            var savedSampler = _gd.SamplerStates[0];

            // Skybox-typical state
            _gd.DepthStencilState = DepthStencilState.None;
            _gd.RasterizerState   = RasterizerState.CullNone;
            _gd.SamplerStates[0]  = SamplerState.LinearClamp;

            _gd.SetVertexBuffer(_vertexBuffer);

            // Draw the 6 faces, swapping textures between draws
            for (int f = 0; f < 6; f++)
            {
                _effect.Texture = _faceTextures[f];
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _gd.DrawPrimitives(PrimitiveType.TriangleList, f * 6, 2);
                }
            }

            // Restore.
            _gd.DepthStencilState = savedDepth;
            _gd.RasterizerState   = savedRaster;
            _gd.SamplerStates[0]  = savedSampler;
        }
    }
}