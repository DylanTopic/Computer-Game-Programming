using System;
using Microsoft.Xna.Framework;
using SpaceRace.Gameplay;

namespace SpaceRace.Rendering
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = Vector3.Up;

        public float FieldOfView { get; set; } = MathHelper.PiOver4;
        public float AspectRatio { get; set; } = 16f / 9f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 5000f;

        public Matrix View => Matrix.CreateLookAt(Position, Target, Up);
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);

        // ---- Chase camera configuration ----
        public float ChaseDistance { get; set; } = 10f;   // how far behind the ship
        public float ChaseHeight   { get; set; } = 3f;    // how far above the ship
        public float LookAhead     { get; set; } = 5f;    // look-at point this far in front of the ship
        public float PositionSmoothing { get; set; } = 6f;  // higher = snappier
        public float TargetSmoothing   { get; set; } = 10f; // higher = snappier
        public float UpSmoothing       { get; set; } = 5f;

        /// <summary>
        /// Snap camera into a sensible starting position relative to the ship.
        /// Call once after the ship is created, before the first Update.
        /// </summary>
        public void SnapTo(Ship ship)
        {
            Position = ship.Position - ship.Forward * ChaseDistance + ship.Up * ChaseHeight;
            Target   = ship.Position + ship.Forward * LookAhead;
            Up       = ship.Up;
        }

        /// <summary>
        /// Each frame: ease toward the desired chase pose. dt-aware so it feels the same at any frame rate.
        /// </summary>
        public void UpdateChase(Ship ship, float dt)
        {
            Vector3 desiredPos    = ship.Position - ship.Forward * ChaseDistance + ship.Up * ChaseHeight;
            Vector3 desiredTarget = ship.Position + ship.Forward * LookAhead;
            Vector3 desiredUp     = ship.Up;

            Position = ExpLerp(Position, desiredPos,    PositionSmoothing, dt);
            Target   = ExpLerp(Target,   desiredTarget, TargetSmoothing,   dt);
            Up       = Vector3.Normalize(ExpLerp(Up, desiredUp, UpSmoothing, dt));
        }

        private static Vector3 ExpLerp(Vector3 current, Vector3 target, float rate, float dt)
        {
            float t = 1f - MathF.Exp(-rate * dt);
            return Vector3.Lerp(current, target, t);
        }
    }
}