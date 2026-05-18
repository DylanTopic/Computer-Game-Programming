using System;
using BepuPhysics;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using SpaceRace.Rendering;

using NumericsVector3 = System.Numerics.Vector3;
using NumericsQuaternion = System.Numerics.Quaternion;

namespace SpaceRace.Gameplay
{
    public class Ring
    {
        public Vector3 Position { get; }
        public Quaternion Orientation { get; }
        public float MajorRadius { get; }   // hole radius
        public float MinorRadius { get; }   // tube thickness
        public bool IsHighlighted { get; set; }
        public bool IsPassed { get; set; }

        public StaticHandle StaticHandle { get; }

        public Matrix WorldMatrix =>
            Matrix.CreateFromQuaternion(Orientation) *
            Matrix.CreateTranslation(Position);

        // World-space "forward" of the ring
        public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Orientation);

        public Ring(Simulation simulation, Vector3 position, Quaternion orientation, float majorRadius = 10f, float minorRadius = 0.6f)
        {
            Position = position;
            Orientation = orientation;
            MajorRadius = majorRadius;
            MinorRadius = minorRadius;
            var shape = new Sphere(0.05f);
            var shapeIndex = simulation.Shapes.Add(shape);

            var pose = new RigidPose(
                new NumericsVector3(position.X, position.Y, position.Z),
                new NumericsQuaternion(orientation.X, orientation.Y, orientation.Z, orientation.W));

            StaticHandle = simulation.Statics.Add(new StaticDescription(pose, shapeIndex));
        }

        public void Draw(TorusPrimitive mesh, Matrix view, Matrix projection, GameTime gameTime)
        {
            Color tint;
            if (IsPassed)
            {
                tint = new Color(0.25f, 0.25f, 0.25f);
            }
            else if (IsHighlighted)
            {
                float t = (float)gameTime.TotalGameTime.TotalSeconds;
                float pulse = 0.7f + 0.3f * MathF.Sin(t * 4f);
                tint = new Color(0f, pulse, pulse);
            }
            else
            {
                tint = new Color(0.85f, 0.85f, 0.85f);
            }

            mesh.Draw(WorldMatrix, view, projection, tint);
        }
    }
}