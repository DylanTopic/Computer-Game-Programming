using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceRace.Rendering
{
    public class ShipPrimitive
    {
        private readonly VertexBuffer _vertexBuffer;
        private readonly BasicEffect _effect;
        private readonly GraphicsDevice _gd;
        private readonly int _triangleCount;

        public ShipPrimitive(GraphicsDevice gd)
        {
            _gd = gd;

            var verts = BuildVertices();
            _triangleCount = verts.Length / 3;

            _vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColor), verts.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(verts);

            _effect = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
            };
        }

        private static VertexPositionColor[] BuildVertices()
        {
            // Body cross-sections
            // R = rear, F = front, T/B = top/bottom, L/R = left/right
            Vector3 bRTL = new Vector3(-0.5f,  0.25f,  1.2f);
            Vector3 bRTR = new Vector3( 0.5f,  0.25f,  1.2f);
            Vector3 bRBL = new Vector3(-0.5f, -0.25f,  1.2f);
            Vector3 bRBR = new Vector3( 0.5f, -0.25f,  1.2f);
            Vector3 bFTL = new Vector3(-0.3f,  0.15f, -0.8f);
            Vector3 bFTR = new Vector3( 0.3f,  0.15f, -0.8f);
            Vector3 bFBL = new Vector3(-0.3f, -0.15f, -0.8f);
            Vector3 bFBR = new Vector3( 0.3f, -0.15f, -0.8f);

            // Nose tip
            Vector3 nTip = new Vector3(0f, 0f, -1.8f);

            // Wing vertices
            Vector3 lwFront = new Vector3(-0.5f, 0f, -0.2f);
            Vector3 lwRear  = new Vector3(-0.5f, 0f,  0.8f);
            Vector3 lwTip   = new Vector3(-1.7f, 0f,  0.4f);
            Vector3 rwFront = new Vector3( 0.5f, 0f, -0.2f);
            Vector3 rwRear  = new Vector3( 0.5f, 0f,  0.8f);
            Vector3 rwTip   = new Vector3( 1.7f, 0f,  0.4f);

            // Tail fin 
            Vector3 tBaseFront = new Vector3(0f, 0.25f, 0.4f);
            Vector3 tBaseRear  = new Vector3(0f, 0.25f, 1.2f);
            Vector3 tTip       = new Vector3(0f, 1.1f,  0.9f);

            // Color 
            Color bodyColor   = new Color(150, 160, 175);
            Color noseColor   = new Color(220,  90,  50);
            Color wingColor   = new Color(190, 200, 210);
            Color tailColor   = new Color( 80, 180, 220);
            Color engineColor = new Color( 60,  60,  80);

            var list = new List<VertexPositionColor>(64);

            void Tri(Vector3 a, Vector3 b, Vector3 c, Color color)
            {
                list.Add(new VertexPositionColor(a, color));
                list.Add(new VertexPositionColor(b, color));
                list.Add(new VertexPositionColor(c, color));
            }

            // Body 
            // Top
            Tri(bRTL, bFTL, bFTR, bodyColor);
            Tri(bRTL, bFTR, bRTR, bodyColor);
            // Bottom
            Tri(bRBL, bFBL, bFBR, bodyColor);
            Tri(bRBL, bFBR, bRBR, bodyColor);
            // Left
            Tri(bRTL, bRBL, bFBL, bodyColor);
            Tri(bRTL, bFBL, bFTL, bodyColor);
            // Right
            Tri(bRTR, bFTR, bFBR, bodyColor);
            Tri(bRTR, bFBR, bRBR, bodyColor);
            // Rear engine face 
            Tri(bRTL, bRTR, bRBR, engineColor);
            Tri(bRTL, bRBR, bRBL, engineColor);

            // Nose pyramid 
            Tri(nTip, bFTL, bFTR, noseColor);
            Tri(nTip, bFTR, bFBR, noseColor);
            Tri(nTip, bFBR, bFBL, noseColor);
            Tri(nTip, bFBL, bFTL, noseColor);

            // Wings 
            Tri(lwFront, lwTip, lwRear, wingColor);
            Tri(rwFront, rwRear, rwTip, wingColor);

            // Tail fin 
            Tri(tBaseFront, tTip, tBaseRear, tailColor);

            return list.ToArray();
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            _effect.World = world;
            _effect.View = view;
            _effect.Projection = projection;

            var savedRaster = _gd.RasterizerState;
            _gd.RasterizerState = RasterizerState.CullNone;

            _gd.SetVertexBuffer(_vertexBuffer);
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawPrimitives(PrimitiveType.TriangleList, 0, _triangleCount);
            }

            _gd.RasterizerState = savedRaster;
        }
    }
}