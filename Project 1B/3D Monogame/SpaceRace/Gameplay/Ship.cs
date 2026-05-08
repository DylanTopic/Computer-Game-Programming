using System;
using BepuPhysics;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceRace.Physics;

using NumericsVector3 = System.Numerics.Vector3;
using NumericsQuaternion = System.Numerics.Quaternion;

namespace SpaceRace.Gameplay
{
    public class Ship : PhysicsObject
    {
        // Tunable handling parameters. We'll iterate on these in Step 7.
        public float ThrustForce      { get; set; } = 35f;   // N along local -Z
        public float ReverseThrust    { get; set; } = 18f;   // N along local +Z
        public float PitchTorque      { get; set; } = 6f;    // N·m around local X
        public float YawTorque        { get; set; } = 6f;    // N·m around local Y
        public float RollTorque       { get; set; } = 8f;    // N·m around local Z
        public float MaxLinearSpeed   { get; set; } = 60f;   // m/s soft cap
        public float MaxAngularSpeed  { get; set; } = 4f;    // rad/s soft cap

        public Ship(Simulation simulation, BodyHandle handle) : base(simulation, handle) { }

        /// <summary>
        /// Factory: creates the Bepu body for the ship and wraps it.
        /// </summary>
        public static Ship Create(Simulation simulation, NumericsVector3 startPosition)
        {
            // For now the ship is a 2x1x3 box (width, height, length). We'll swap for a model later.
            var shape = new Box(2f, 1f, 3f);
            var inertia = shape.ComputeInertia(2f); // 2 kg
            var shapeIndex = simulation.Shapes.Add(shape);

            var description = BodyDescription.CreateDynamic(
                new RigidPose(startPosition, NumericsQuaternion.Identity),
                inertia,
                new CollidableDescription(shapeIndex, 0.1f),
                new BodyActivityDescription(0.01f));

            var handle = simulation.Bodies.Add(description);
            return new Ship(simulation, handle);
        }

        /// <summary>
        /// Per-frame input → physics impulses. Pure forces / torques only.
        /// </summary>
        public void ApplyInput(KeyboardState kb, float dt)
        {
            var bodyRef = Simulation.Bodies[BodyHandle];
            ref var pose = ref bodyRef.Pose;

            var rotation = System.Numerics.Matrix4x4.CreateFromQuaternion(pose.Orientation);
            var localRight   = new NumericsVector3(rotation.M11, rotation.M12, rotation.M13);
            var localUp      = new NumericsVector3(rotation.M21, rotation.M22, rotation.M23);
            var localForward = -new NumericsVector3(rotation.M31, rotation.M32, rotation.M33);

            // ---- Linear thrust ----
            NumericsVector3 thrust = NumericsVector3.Zero;
            if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift))
                thrust += localForward * ThrustForce;
            if (kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl))
                thrust -= localForward * ReverseThrust;

            // ---- Angular torques ----
            NumericsVector3 torque = NumericsVector3.Zero;
            bool pitchInput = false, yawInput = false, rollInput = false;

            if (kb.IsKeyDown(Keys.W)) { torque += localRight * -PitchTorque; pitchInput = true; }
            if (kb.IsKeyDown(Keys.S)) { torque += localRight *  PitchTorque; pitchInput = true; }
            if (kb.IsKeyDown(Keys.A)) { torque += localUp    *  YawTorque;   yawInput = true; }
            if (kb.IsKeyDown(Keys.D)) { torque += localUp    * -YawTorque;   yawInput = true; }
            if (kb.IsKeyDown(Keys.Q)) { torque += localForward *  RollTorque; rollInput = true; }
            if (kb.IsKeyDown(Keys.E)) { torque += localForward * -RollTorque; rollInput = true; }

            // ---- Auto-stabilization: counter-torque on axes with no input ----
            // For each local axis the player isn't actively driving, apply a torque proportional
            // to the current angular velocity component along that axis, in the opposite direction.
            // This rapidly bleeds off residual spin without overshooting (it's a P-controller).
            var angVel = bodyRef.Velocity.Angular;
            float stabilizerStrength = 4f; // tune to taste; higher = stiffer auto-level

            if (!pitchInput)
            {
                float spinAroundRight = NumericsVector3.Dot(angVel, localRight);
                torque -= localRight * (spinAroundRight * stabilizerStrength);
            }
            if (!yawInput)
            {
                float spinAroundUp = NumericsVector3.Dot(angVel, localUp);
                torque -= localUp * (spinAroundUp * stabilizerStrength);
            }
            if (!rollInput)
            {
                float spinAroundForward = NumericsVector3.Dot(angVel, localForward);
                torque -= localForward * (spinAroundForward * stabilizerStrength);
            }

            // ---- Apply ----
            bodyRef.Awake = true;
            if (thrust != NumericsVector3.Zero)
                bodyRef.ApplyLinearImpulse(thrust * dt);
            if (torque != NumericsVector3.Zero)
                bodyRef.ApplyAngularImpulse(torque * dt);

            ClampSpeed(ref bodyRef.Velocity.Linear, MaxLinearSpeed);
            ClampSpeed(ref bodyRef.Velocity.Angular, MaxAngularSpeed);
        }
        private static void ClampSpeed(ref NumericsVector3 v, float maxMagnitude)
        {
            float lenSq = v.LengthSquared();
            if (lenSq > maxMagnitude * maxMagnitude)
            {
                float len = MathF.Sqrt(lenSq);
                v *= maxMagnitude / len;
            }
        }
        // World-space basis vectors derived from the ship's current orientation.
        // Forward is -Z in local space (graphics convention).
        public Vector3 Forward
        {
            get
            {
                var q = Simulation.Bodies[BodyHandle].Pose.Orientation;
                var rot = System.Numerics.Matrix4x4.CreateFromQuaternion(q);
                return new Vector3(-rot.M31, -rot.M32, -rot.M33);
            }
        }

        public Vector3 Up
        {
            get
            {
                var q = Simulation.Bodies[BodyHandle].Pose.Orientation;
                var rot = System.Numerics.Matrix4x4.CreateFromQuaternion(q);
                return new Vector3(rot.M21, rot.M22, rot.M23);
            }
        }

        public Vector3 Right
        {
            get
            {
                var q = Simulation.Bodies[BodyHandle].Pose.Orientation;
                var rot = System.Numerics.Matrix4x4.CreateFromQuaternion(q);
                return new Vector3(rot.M11, rot.M12, rot.M13);
            }
        }
        
        public float Speed
        {
            get
            {
                var v = Simulation.Bodies[BodyHandle].Velocity.Linear;
                return v.Length();
            }
        }
    }
}