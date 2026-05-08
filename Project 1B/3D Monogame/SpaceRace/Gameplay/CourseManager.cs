using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpaceRace.Gameplay
{
    /// <summary>
    /// Owns the ordered ring sequence, tracks the current target ring,
    /// detects fly-throughs via plane-crossing math, and counts misses.
    /// </summary>
    public class CourseManager
    {
        public IReadOnlyList<Ring> Rings => _rings;

        public float TotalElapsedSeconds { get; private set; }
        public int TargetIndex { get; private set; }
        public int RingsPassed { get; private set; }
        public int RingsMissed { get; private set; }
        public bool IsComplete => TargetIndex >= _rings.Count;

        private readonly List<Ring> _rings;
        private Vector3 _previousShipPosition;
        private bool _hasPreviousPosition;

        public CourseManager(List<Ring> rings)
        {
            _rings = rings;
            for (int i = 0; i < _rings.Count; i++)
            {
                _rings[i].IsHighlighted = (i == 0);
                _rings[i].IsPassed = false;
            }
            TargetIndex = 0;
        }

        public void Update(Ship ship, float dt)
        {
            if (!IsComplete)
                TotalElapsedSeconds += dt;

            if (IsComplete) return;

            Vector3 currentShipPosition = ship.Position;

            if (!_hasPreviousPosition)
            {
                _previousShipPosition = currentShipPosition;
                _hasPreviousPosition = true;
                return;
            }

            Ring target = _rings[TargetIndex];

            float prevSigned = Vector3.Dot(_previousShipPosition - target.Position, target.Forward);
            float currSigned = Vector3.Dot(currentShipPosition  - target.Position, target.Forward);

            bool crossed = (prevSigned < 0f && currSigned >= 0f) || (prevSigned > 0f && currSigned <= 0f);

            if (crossed)
            {
                float denom = prevSigned - currSigned;
                float t = denom != 0f ? prevSigned / denom : 0.5f;
                Vector3 crossingPoint = Vector3.Lerp(_previousShipPosition, currentShipPosition, t);

                float radialDistance = Vector3.Distance(crossingPoint, target.Position);

                target.IsPassed = true;
                target.IsHighlighted = false;

                if (radialDistance <= target.MajorRadius)
                    RingsPassed++;
                else
                    RingsMissed++;

                AdvanceTarget();
            }

            _previousShipPosition = currentShipPosition;
        }
        private void AdvanceTarget()
        {
            TargetIndex++;
            if (TargetIndex < _rings.Count)
                _rings[TargetIndex].IsHighlighted = true;
        }
        public void Reset()
        {
            TargetIndex = 0;
            RingsPassed = 0;
            RingsMissed = 0;
            TotalElapsedSeconds = 0f;
            _hasPreviousPosition = false;

            for (int i = 0; i < _rings.Count; i++)
            {
                _rings[i].IsPassed = false;
                _rings[i].IsHighlighted = (i == 0);
            }
        }
    }
}