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

        public void AttachHand(Transform target, bool lockRotation = true) 
        {
            IsAttached = true;
            this.lockRotation = lockRotation;
            transformToAttachTo = target;
        }
        public void DetachHand()
        {
            IsAttached = false;
        }
        protected override void UpdateTransform()
        {
            if (IsAttached && transformToAttachTo == null)
                DetachHand();

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

    }
}
