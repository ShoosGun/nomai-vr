using NomaiVR.Assets;
using NomaiVR.InteractableControllers.Joysticks;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.InteractableControllers
{
    public class ModelShipControllers : NomaiVRModule<ModelShipControllers.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private Transform modelShipControllers;
            private Transform interactalbeControllers;

            internal void Start()
            {
                //TODO find model ship controllers object
                modelShipControllers = Locator.GetPlayerTransform();
                SetUpModelShipControllers(modelShipControllers);
            }

            private void SetUpModelShipControllers(Transform modelShipControllers)
            {
                SetUpStickHoldExample(modelShipControllers,InputConsts.InputCommandType.MOVE_Z,InputConsts.InputCommandType.MOVE_X);
                SetUpStickHoldExample(modelShipControllers, InputConsts.InputCommandType.LOOK_Y, InputConsts.InputCommandType.LOOK_X);
            }

            float previousPosition = 0.1f;
            private Transform SetUpTopHoldExample(Transform modelShipControllers, InputConsts.InputCommandType inputA, InputConsts.InputCommandType inputB)
            {
                previousPosition = -previousPosition;
                interactalbeControllers = Instantiate(AssetLoader.TopHoldJoystickPrefab).transform;
                interactalbeControllers.parent = modelShipControllers;
                interactalbeControllers.position = modelShipControllers.position + modelShipControllers.forward * 0.25f + modelShipControllers.right * previousPosition;
                interactalbeControllers.localRotation = Quaternion.identity;

                Transform xAxisValueAxis = interactalbeControllers.Find("XZJoystick/XAxisValueAxis");
                Transform yAxisValueAxis = interactalbeControllers.Find("XZJoystick/YAxisValueAxis");
                Transform stickTop = interactalbeControllers.Find("XZJoystick/StickBaseBase/StickBase/StickTop");
                stickTop.gameObject.AddComponent<SingleHandHoldablePoint>().LockHandRotation = false;
                HoldTopJoystick joystick = stickTop.gameObject.AddComponent<HoldTopJoystick>();
                joystick.xAxisInputToSimulate = inputA;
                joystick.yAxisInputToSimulate = inputB;
                joystick.xAxisValueAxis = xAxisValueAxis;
                joystick.yAxisValueAxis = yAxisValueAxis;
                return interactalbeControllers;
            }
            private Transform SetUpStickHoldExample(Transform modelShipControllers, InputConsts.InputCommandType inputA, InputConsts.InputCommandType inputB)
            {
                previousPosition = -previousPosition;
                interactalbeControllers = Instantiate(AssetLoader.StickHoldJoystickPrefab).transform;
                interactalbeControllers.parent = modelShipControllers;
                interactalbeControllers.position = modelShipControllers.position + modelShipControllers.forward * 0.25f + modelShipControllers.right * previousPosition;
                interactalbeControllers.localRotation = Quaternion.identity;

                Transform xAxisValueAxis = interactalbeControllers.Find("XZJoystick/XAxisValueAxis");
                Transform yAxisValueAxis = interactalbeControllers.Find("XZJoystick/YAxisValueAxis");
                Transform stickTop = interactalbeControllers.Find("XZJoystick/StickBaseBase/StickBase/Stick");

                stickTop.gameObject.AddComponent<SingleHandHoldablePoint>().LockHandRotation = true;

                HoldTopJoystick joystick = stickTop.gameObject.AddComponent<HoldTopJoystick>();
                joystick.xAxisInputToSimulate = inputA;
                joystick.yAxisInputToSimulate = inputB;
                joystick.xAxisValueAxis = xAxisValueAxis;
                joystick.MaxXAxisAngle = 0f;
                joystick.MaxXAxisAngle = 20f;
                joystick.yAxisValueAxis = yAxisValueAxis;
                return interactalbeControllers;
            }
        }
    }
}
