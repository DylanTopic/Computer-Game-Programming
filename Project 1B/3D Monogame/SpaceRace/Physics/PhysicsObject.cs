using BepuPhysics;
using Microsoft.Xna.Framework;
using NumericsVector3 = System.Numerics.Vector3;
using NumericsQuaternion = System.Numerics.Quaternion;

namespace SpaceRace.Physics
{
    /// <summary>
    /// Bridges a Bepu body to MonoGame rendering. Owns a BodyHandle and
    /// exposes the body's current world transform as a MonoGame Matrix.
    /// </summary>
    public class PhysicsObject
    {
        public BodyHandle BodyHandle { get; }
        protected readonly Simulation Simulation;

        public PhysicsObject(Simulation simulation, BodyHandle handle)
        {
            Simulation = simulation;
            BodyHandle = handle;
        }

        public Vector3 Position
        {
            get
            {
                var p = Simulation.Bodies[BodyHandle].Pose.Position;
                return new Vector3(p.X, p.Y, p.Z);
            }
        }

        public Quaternion Orientation
        {
            get
            {
                var q = Simulation.Bodies[BodyHandle].Pose.Orientation;
                return new Quaternion(q.X, q.Y, q.Z, q.W);
            }
        }

        /// <summary>
        /// World matrix combining current orientation and position from Bepu.
        /// Cube/ship/ring renderers all consume this.
        /// </summary>
        public Matrix WorldMatrix
        {
            get
            {
                var pose = Simulation.Bodies[BodyHandle].Pose;
                return Matrix.CreateFromQuaternion(new Quaternion(pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W))
                     * Matrix.CreateTranslation(new Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z));
            }
        }
    }
}