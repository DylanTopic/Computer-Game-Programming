using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuUtilities;

namespace SpaceRace.Physics
{
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        public float LinearDamping;
        public float AngularDamping;

        Vector3Wide _gravityWideDt;
        Vector<float> _linearDampingDt;
        Vector<float> _angularDampingDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public bool AllowSubstepsForUnconstrainedBodies => false;
        public bool IntegrateVelocityForKinematics => false;

        public PoseIntegratorCallbacks(Vector3 gravity, float linearDamping = 0f, float angularDamping = 0f) : this()
        {
            Gravity = gravity;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
        }

        public void Initialize(Simulation simulation) { }

        public void PrepareForIntegration(float dt)
        {
            _linearDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt));
            _angularDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt));
            _gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear = (velocity.Linear + _gravityWideDt) * _linearDampingDt;
            velocity.Angular *= _angularDampingDt;
        }
    }
}