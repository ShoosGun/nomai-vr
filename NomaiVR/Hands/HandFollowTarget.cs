using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.Hands
{
    public class HandFollowTarget : FollowTarget
    {
        private Transform transformToAttachTo;
        public bool IsAttached { get;private set; }

        private bool lockRotation;

        public bool ShouldFollowWithPhysics;
        private Rigidbody rigidbody;
        public float FollowSpeed = 30f;
        public float RotationSpeed = 100f;
        private FixedJoint joint1, joint2;

        protected override void Start()
        {
            base.Start();
            rigidbody = gameObject.GetComponent<Rigidbody>();
        }
        public void AttachHand(Transform target, bool lockRotation = true) 
        {
            if (ShouldFollowWithPhysics)
            {
                RegularAttach(target, lockRotation);
                return;
            }
            var targetRig = target.GetComponent<Rigidbody>();
            if (targetRig != null)
                PhysicsAttach(targetRig, lockRotation);
        }
        private void RegularAttach(Transform target, bool lockRotation)
        {
            IsAttached = true;
            this.lockRotation = lockRotation;
            transformToAttachTo = target;
        }
        private void PhysicsAttach(Rigidbody target, bool lockRotation)
        {
            target.velocity = Vector3.zero;
            target.angularVelocity = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            target.collisionDetectionMode = CollisionDetectionMode.Continuous;
            target.interpolation = RigidbodyInterpolation.Interpolate;

            joint1 = gameObject.AddComponent<FixedJoint>();
            joint1.connectedBody = target;
            joint1.breakForce = float.PositiveInfinity;
            joint1.breakTorque = float.PositiveInfinity;

            joint1.massScale = 1f;
            joint1.connectedMassScale = 1f;
            joint1.enableCollision = false;
            joint1.enablePreprocessing = false;

            joint2 = target.gameObject.AddComponent<FixedJoint>();
            joint2.connectedBody = rigidbody;
            joint2.breakForce = float.PositiveInfinity;
            joint2.breakTorque = float.PositiveInfinity;

            joint2.massScale = 1f;
            joint2.connectedMassScale = 1f;
            joint2.enableCollision = false;
            joint2.enablePreprocessing = false;

            this.lockRotation = lockRotation;

            transformToAttachTo = target.transform;
        }
        public void DetachHand()
        {
            if (ShouldFollowWithPhysics) 
            {
                PhysicsDetach();
                return;
            }
            RegularDetach();
        }

        private void RegularDetach() 
        {
            transformToAttachTo = null;
            IsAttached = false;
        }
        private void PhysicsDetach() 
        {
            if (joint1 != null)
                Destroy(joint1);
            if (joint2 != null)
                Destroy(joint2);
            var targetRig = transformToAttachTo.GetComponent<Rigidbody>();
            if (targetRig != null)
            {
                targetRig.collisionDetectionMode = CollisionDetectionMode.Discrete;
                targetRig.interpolation = RigidbodyInterpolation.None;
                transformToAttachTo = null;
            }
            IsAttached = false;
        }
        protected override void UpdateTransform()
        {
            if (IsAttached && transformToAttachTo == null)
                DetachHand();

            if (ShouldFollowWithPhysics)
                FollowWithPhysics();
            else
                FollowWithoutPhysics();
        }
        protected void FollowWithoutPhysics()
        {
            Transform transformToFollowPosition = IsAttached ? transformToAttachTo : Target;
            Transform transformToFollowRotation = IsAttached && lockRotation ? transformToAttachTo : Target;

            var targetRotation = transformToFollowRotation.rotation * LocalRotation;
            transform.rotation = RotationSmoothTime > 0
                ? MathHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, RotationSmoothTime)
                : targetRotation;

            var targetPosition = transformToFollowPosition.TransformPoint(LocalPosition);
            transform.position = PositionSmoothTime > 0
                ? MathHelper.SmoothDamp(transform.position, targetPosition, ref positionVelocity, RotationSmoothTime)
                : targetPosition;
        }
        //From https://www.youtube.com/watch?v=JR2-Qhs2vJc
        protected void FollowWithPhysics()
        {
            if (rigidbody == null)
                return;

            var targetPosition = Target.TransformPoint(LocalPosition);
            var targetDistance = Vector3.Distance(targetPosition, transform.position);
            rigidbody.velocity = (targetPosition - transform.position).normalized * (FollowSpeed * targetDistance);

            var targetRotation = Target.rotation * LocalRotation;
            var q = targetRotation * Quaternion.Inverse(rigidbody.rotation);
            q.ToAngleAxis(out float angle, out Vector3 axis);
            rigidbody.angularVelocity = axis * (angle * Mathf.Deg2Rad * RotationSpeed);
        }
    }
}
