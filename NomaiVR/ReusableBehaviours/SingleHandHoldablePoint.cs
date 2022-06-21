using UnityEngine;
using Valve.VR;
using NomaiVR.Hands;

namespace NomaiVR.ReusableBehaviours
{
    internal class SingleHandHoldablePoint : MonoBehaviour
    {
        private ProximityDetector proximityDetector;
        private HandFollowTarget interactingHandFollowTarget;
        
        public float InteractRadius = 0.1f;
        public Vector3 InteractOffset = Vector3.zero;
        public bool LockHandRotation = false;

        protected void Awake()
        {
            var localSphereCollider = GetComponent<SphereCollider>();
            if (localSphereCollider != null)
            {
                InteractRadius = localSphereCollider.radius;
                InteractOffset = localSphereCollider.center;
                Destroy(localSphereCollider);
            }
        }
        protected void Start()
        {
            proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.MinDistance = InteractRadius;
            proximityDetector.LocalOffset = InteractOffset;
            proximityDetector.ExitThreshold = InteractRadius * 0.04f;
            proximityDetector.SetTrackedObjects(HandsController.Behaviour.RightHand, HandsController.Behaviour.LeftHand);
            Logs.Write("AAAAAAA", debugOnly: false);
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;

            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }
        protected void OnDestroy()
        {
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }
        protected void OnDisable() 
        {
            if (interactingHandFollowTarget != null)
                interactingHandFollowTarget.DetachHand();

            interactingHandFollowTarget = null;
        }

        public bool IsHandHolding() 
        {
            return interactingHandFollowTarget != null;
        }

        public Transform GetHandTarget()
        {
            return interactingHandFollowTarget.Target;
        }
        public float GetTrackedHandDistance(int index)
        {
            return proximityDetector.GetTrackedObjectDistance(index);
        }

        private void OnGripUpdated(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            if (!enabled)
                return;

            var handIndex = fromSource == SteamVR_Input_Sources.RightHand ? 0 : 1;
            HandFollowTarget interactingHandFollowTargetFromUpdate = proximityDetector.GetTrackedObject(handIndex).GetComponent<HandFollowTarget>();

            if (fromAction.GetState(fromSource) && proximityDetector.IsInside(handIndex))
            {
                if (interactingHandFollowTarget != null)
                    interactingHandFollowTarget.DetachHand();

                if (interactingHandFollowTargetFromUpdate.IsAttached)
                    return;

                interactingHandFollowTarget = interactingHandFollowTargetFromUpdate;
                interactingHandFollowTarget.AttachHand(transform, LockHandRotation);
            }
            else if (!fromAction.GetState(fromSource) && interactingHandFollowTarget == interactingHandFollowTargetFromUpdate)
            {
                interactingHandFollowTarget.DetachHand();
                interactingHandFollowTarget = null;
            }
        }
    }
}
