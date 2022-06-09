using NomaiVR.Hands;
using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class HandAttachPoint : MonoBehaviour
	{
		private Transform handTransform;
		private Transform handDriverTransform;
		private Hand hand;
		private FollowTarget handFollowTarget;
		private bool lockHandRotation;

		public Vector3 AttachOffset;

		public float PositionSmoothTime;
		public float RotationSmoothTime;
		private Quaternion rotationVelocity;
		private Vector3 positionVelocity;

		public Transform GetAttachedHandDriverTransform() => handDriverTransform;

		private void OnDestroy()
		{
			if(handFollowTarget!= null)
				DettachHand();
		}
		public void AttachHand(Hand hand, bool lockHandRotation = true)
		{
			if(this.hand != null) 
			{
				Logs.Write($"Hand {this.hand.pose} is already attached", MessageType.Warning);
				return;
			}
			this.lockHandRotation = lockHandRotation;
			this.hand = hand;
			handFollowTarget = hand.GetComponent<FollowTarget>();
			handTransform = hand.transform;
			handFollowTarget.followType = lockHandRotation ?  FollowTarget.FollowType.None : FollowTarget.FollowType.OnlyRotation;
			handDriverTransform = handFollowTarget.Target.transform;
		}
		public void DettachHand()
		{
			handFollowTarget.followType = FollowTarget.FollowType.PositionAndRotation;
			hand = null;
			handDriverTransform = null;
			handFollowTarget = null;
			handTransform = null;
		}
		private void LateUpdate()
		{
			if(handTransform != null)
				UpdateHandAttach();
		}
		private void UpdateHandAttach()
		{
			if (lockHandRotation)
			{
				handTransform.rotation = RotationSmoothTime > 0
					? MathHelper.SmoothDamp(handTransform.rotation, transform.rotation, ref rotationVelocity, RotationSmoothTime)
					: transform.rotation;
			}
            var thisLocalPosition = transform.TransformPoint(AttachOffset);
			handTransform.position = PositionSmoothTime > 0
                ? MathHelper.SmoothDamp(handTransform.position, thisLocalPosition, ref positionVelocity, RotationSmoothTime)
                : thisLocalPosition;

        }
	}
}
