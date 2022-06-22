using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Helpers;
using System;
using UnityEngine;
using static InputConsts;

namespace NomaiVR.InteractableControllers.Joysticks
{
    public class HoldTopJoystick : GlowyJoystick
    {
        public Func<bool> CheckEnabled { get; set; }

        private Transform joystickStickBase;
        public bool returnToCenterWhenReleased = true;
        public Transform xAxisValueAxis;
        public Transform yAxisValueAxis;

        public float MaxXAxisAngle = 90f;
        public float MaxYAxisAngle = 90f;

        private float phiAngle = 0f;
        private float thetaAngle = 0f;

        private SingleHandHoldablePoint holdablePoint;

        public InputCommandType xAxisInputToSimulate;
        public InputCommandType yAxisInputToSimulate = InputCommandType.UNDEFINED;
        public ControllerInput.InputOverrideType inputOverrideType = ControllerInput.InputOverrideType.NormalOverride;
        protected override void Initialize()
        {
            joystickStickBase = transform.parent;
        }

        protected void Start() 
        {
            holdablePoint = gameObject.GetComponent<SingleHandHoldablePoint>();
        }

        private void OnDisable()
        {
            DisableSimulatedInput();
        }
        private float CalculateHandDistance()
        {
            float distanceFromRightHand = holdablePoint.GetTrackedHandDistance(0);
            float distanceFromLeftHand = holdablePoint.GetTrackedHandDistance(1);
            if (distanceFromLeftHand < distanceFromRightHand)
            {
                HandsController.Behaviour.LeftHand.GetComponent<Hand>().NotifyPointable(distanceFromLeftHand < holdablePoint.InteractRadius ? 0 : distanceFromLeftHand);
                return distanceFromLeftHand;
            }
            HandsController.Behaviour.RightHand.GetComponent<Hand>().NotifyPointable(distanceFromRightHand < holdablePoint.InteractRadius ? 0 : distanceFromRightHand);
            return distanceFromRightHand;
        }
        public Vector2 GetJoystickInputValue()
        {
            Vector3 joystickValueDirection = joystickStickBase.forward;
            Vector3 xAxisValueAxisDirection = xAxisValueAxis.forward;
            Vector3 yAxisValueAxisDirection = yAxisValueAxis.forward;
            float xAxisValue = Mathf.Sin(phiAngle) * Mathf.Cos(thetaAngle);
            float yAxisValue = Mathf.Sin(phiAngle) * Mathf.Sin(thetaAngle);
            return new Vector2(xAxisValue, yAxisValue);
        }
        private void FollowHandDirection()
        {
            Vector3 direction = holdablePoint.GetHandTarget().position - joystickStickBase.position;
            MathHelper.ToSphericalCoordinates(direction, out float radius, out float phiAngle, out float thetaAngle);
            AngleClampFunction(phiAngle, thetaAngle);

            var clampedDirection = MathHelper.FromSphericalCoordinates(1f, this.phiAngle, this.thetaAngle);
            joystickStickBase.rotation = Quaternion.LookRotation(clampedDirection);
        }
        private void AngleClampFunction(float phiAngle, float thetaAngle) 
        {
            this.phiAngle = phiAngle;//Mathf.Clamp(phiAngle, 0f, Mathf.Deg2Rad * 45f);
            this.thetaAngle = thetaAngle;
        }
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
            if (holdablePoint.IsHandHolding())
                FollowHandDirection();
            else if (returnToCenterWhenReleased)
                joystickStickBase.localRotation = Quaternion.identity;
            SimulateInput();

            return holdablePoint.IsHandHolding();
        }

        protected override bool IsJoystickFocused()
        {
            return !holdablePoint.IsHandHolding() ? CalculateHandDistance() < Hand.minimumPointDistance * 1.5f : false;
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
