using NomaiVR.Hands;
using NomaiVR.Input;
using System;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.ReusableBehaviours
{
    public class HoldJoystick : GlowyJoystick
    {
        public Func<bool> CheckEnabled { get; set; }

        private float interactRadius = 0.1f;
        private Vector3 interactOffset = Vector3.zero;

        
        private Transform joystickStickBase;
        public bool returnToCenterWhenReleased = true;
        public Transform xAxisValueAxis;
        public Transform yAxisValueAxis;

        private ProximityDetector proximityDetector;

        private Hand interactingHand;
        private HandAttachPoint handAttachPoint;

        public InputCommandType xAxisInputToSimulate;
        public InputCommandType yAxisInputToSimulate = InputCommandType.UNDEFINED;
        public ControllerInput.InputOverrideType inputOverrideType = ControllerInput.InputOverrideType.NormalOverride;

        protected override void Initialize()
        {
            var localSphereCollider = GetComponent<SphereCollider>();
            if (localSphereCollider != null)
            {
                interactRadius = localSphereCollider.radius;
                interactOffset = localSphereCollider.center;
                Destroy(localSphereCollider);
            }
            joystickStickBase = transform.parent;
        }


        private void Start()
        {
            proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.MinDistance = interactRadius;
            proximityDetector.LocalOffset = interactOffset;
            proximityDetector.ExitThreshold = interactRadius * 0.04f;
            proximityDetector.SetTrackedObjects(HandsController.Behaviour.RightHand, HandsController.Behaviour.LeftHand);
            proximityDetector.OnExit += HandExit;

            handAttachPoint = gameObject.AddComponent<HandAttachPoint>();
            handAttachPoint.PositionSmoothTime = 0.02f;
            handAttachPoint.RotationSmoothTime = 0.02f;
            handAttachPoint.AttachOffset = Vector3.zero;

            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }
        internal void OnDestroy()
        {
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }

        private void OnDisable()
        {
            DisableSimulatedInput();
        }
        private void OnGripUpdated(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            var handIndex = fromSource == SteamVR_Input_Sources.RightHand ? 0 : 1;
            var thisHand = proximityDetector.GetTrackedObject(handIndex);
            if (fromAction.GetState(fromSource) && proximityDetector.IsInside(handIndex))
            {
                interactingHand = thisHand.GetComponent<Hand>();
                handAttachPoint.AttachHand(interactingHand, false);
            }
            else if (!fromAction.GetState(fromSource) && interactingHand == thisHand)
            {
                handAttachPoint.DettachHand();
                interactingHand = null;
            }

        }

        private void HandExit(Transform hand)
        {
            if(interactingHand == null)
                return;
            if (interactingHand.transform == hand)
            {
                handAttachPoint.DettachHand();
                interactingHand = null;
            }
        }

        private float CalculateHandDistance()
        {
            float distanceFromRightHand = proximityDetector.GetTrackedObjectDistance(0);
            float distanceFromLeftHand = proximityDetector.GetTrackedObjectDistance(1);
            if (distanceFromLeftHand < distanceFromRightHand)
            {
                HandsController.Behaviour.LeftHand.GetComponent<Hand>().NotifyPointable(distanceFromLeftHand < interactRadius ? 0 : distanceFromLeftHand);
                return distanceFromLeftHand;
            }
            HandsController.Behaviour.RightHand.GetComponent<Hand>().NotifyPointable(distanceFromRightHand < interactRadius ? 0 : distanceFromRightHand);
            return distanceFromRightHand;
        }
        public Vector2 GetJoystickInputValue()
        {
            Vector3 joystickValueDirection = joystickStickBase.forward;
            Vector3 xAxisValueAxisDirection = xAxisValueAxis.forward;
            Vector3 yAxisValueAxisDirection = yAxisValueAxis.forward;
            float xAxisValue = Vector3.Dot(xAxisValueAxisDirection, joystickValueDirection);
            float yAxisValue = Vector3.Dot(yAxisValueAxisDirection, joystickValueDirection);
            return new Vector2(xAxisValue, yAxisValue);
        }
        private void FollowHandDirection()
        {
            joystickStickBase.LookAt(handAttachPoint.GetAttachedHandDriverTransform());
        }
        private bool IsHandHolding() => interactingHand != null;
        protected override bool IsJoystickEnabled() 
        { 
            bool isEnabled = CheckEnabled == null || CheckEnabled.Invoke();
            if (returnToCenterWhenReleased && !isEnabled)
            {
                joystickStickBase.localRotation = Quaternion.identity;
                DisableSimulatedInput();
            }
            return isEnabled;
        }

        protected override bool IsJoystickActive()
        {
            if (IsHandHolding())
                FollowHandDirection();
            else if (returnToCenterWhenReleased)
                joystickStickBase.localRotation = Quaternion.identity;
            SimulateInput();

            return IsHandHolding();
        }

        protected override bool IsJoystickFocused()
        {
            return !IsHandHolding() ? CalculateHandDistance() < Hand.minimumPointDistance * 1.5f : false;
        }
        private void SimulateInput()
        {
            Vector2 joysticInputValue = GetJoystickInputValue();

            if (xAxisInputToSimulate == yAxisInputToSimulate)
                ControllerInput.SimulateInput(xAxisInputToSimulate, joysticInputValue, forOneFrame: false, inputOverrideType: inputOverrideType);
            else
            {
                ControllerInput.SimulateInput(xAxisInputToSimulate, new Vector2(joysticInputValue.x, 0f), forOneFrame: false, inputOverrideType: inputOverrideType);
                if (yAxisInputToSimulate != InputCommandType.UNDEFINED)
                    ControllerInput.SimulateInput(yAxisInputToSimulate, new Vector2(joysticInputValue.y, 0f), forOneFrame: false, inputOverrideType: inputOverrideType);
            }
        }
        private void DisableSimulatedInput() 
        {
            ControllerInput.SimulateInput(xAxisInputToSimulate, Vector2.zero, clearInput: true);
            if (xAxisInputToSimulate != yAxisInputToSimulate && yAxisInputToSimulate != InputCommandType.UNDEFINED)
                ControllerInput.SimulateInput(yAxisInputToSimulate, Vector2.zero, clearInput: true);
        }
    }
}
