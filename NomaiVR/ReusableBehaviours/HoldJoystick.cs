using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.Tools;
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
        [SerializeField]
        private bool returnToCenterWhenReleased;
        public Vector3 xAxisValueAxis;
        public Vector3 yAxisValueAxis;

        private ProximityDetector proximityDetector;
        private bool isHandInside = false;
        private Transform interactingHand;

        public InputCommandType inputToSimulate;

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
            proximityDetector.SetTrackedObjects(HandsController.Behaviour.DominantHandBehaviour.transform, HandsController.Behaviour.OffHandBehaviour.transform);
            proximityDetector.OnEnter += HandEnter;
            proximityDetector.OnExit += HandExit;
        }

        private void OnDisable()
        {
            isHandInside = false;
            ControllerInput.SimulateInput(inputToSimulate, Vector2.zero, clearInput: true);
        }

        private void HandEnter(Transform hand)
        {
            if (!isHandInside && State != JoystickState.Disabled)
            {
                interactingHand = hand;
                var offHand = VRToolSwapper.NonInteractingHand ? VRToolSwapper.NonInteractingHand : HandsController.Behaviour.OffHandBehaviour;
                SteamVR_Actions.default_Haptic.Execute(0, 0.2f, 300, .2f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
                SteamVR_Actions.default_Haptic.Execute(0.1f, 0.2f, 100, .1f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
            }
            isHandInside = true;
        }

        private void HandExit(Transform hand)
        {
            if (isHandInside && State != JoystickState.Disabled)
            {
                interactingHand = hand;
                var offHand = VRToolSwapper.NonInteractingHand ? VRToolSwapper.NonInteractingHand : HandsController.Behaviour.OffHandBehaviour;
                SteamVR_Actions.default_Haptic.Execute(0, 0.1f, 100, .05f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
            }
            isHandInside = false;
        }

        private float CalculateHandDistance()
        {
            var offHand = VRToolSwapper.NonInteractingHand ? VRToolSwapper.NonInteractingHand : HandsController.Behaviour.OffHandBehaviour;
            var touchableCenter = transform.position + transform.TransformVector(proximityDetector.LocalOffset);
            var touchableClosestPoint = touchableCenter + (offHand.transform.position - touchableCenter).normalized * interactRadius;
            var distanceFromCenter = Vector3.Distance(offHand.transform.position, touchableCenter);
            var offHandDistance = Vector3.Distance(offHand.transform.position, touchableClosestPoint);
            offHand.NotifyPointable(distanceFromCenter < interactRadius ? 0 : offHandDistance);
            return offHandDistance;
        }
        public Vector2 GetJoystickInputValue()
        {
            Vector3 joystickValueDirection = joystickStickBase.forward;
            float xAxisValue = Vector3.Dot(xAxisValueAxis , joystickValueDirection);
            float yAxisValue = Vector3.Dot(yAxisValueAxis, joystickValueDirection);
            return new Vector2(xAxisValue, yAxisValue);
        }
        private void FollowHandDirection()
        {
            if (interactingHand != null)
                joystickStickBase.LookAt(interactingHand);
        }
        private bool wasEnabled = false;
        protected override bool IsJoystickEnabled() 
        { 
            bool isEnabled = CheckEnabled == null || CheckEnabled.Invoke();
            if (isEnabled)
            {
                if (isHandInside)
                    FollowHandDirection();
                else if (returnToCenterWhenReleased)
                    joystickStickBase.localRotation = Quaternion.identity;

                ControllerInput.SimulateInput(inputToSimulate, GetJoystickInputValue(), true);

            }
            else if (returnToCenterWhenReleased && wasEnabled)
            {
                joystickStickBase.localRotation = Quaternion.identity;
                ControllerInput.SimulateInput(inputToSimulate, Vector2.zero, clearInput: true);
            }
            wasEnabled = isEnabled;
            return isEnabled;
        }

        protected override bool IsJoystickActive()
        {
            if (isHandInside) CalculateHandDistance();
            return isHandInside;
        }

        protected override bool IsJoystickFocused()
        {
            return CalculateHandDistance() < Hand.minimumPointDistance * 1.5f;
        }
    }
}
