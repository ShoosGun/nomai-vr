using NomaiVR.Assets;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.InteractableControllers
{
    //TODO add this to NomaiVR ApplyMod()
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
                interactalbeControllers = SetUpModelShipControllers(modelShipControllers);
            }

            private Transform SetUpModelShipControllers(Transform modelShipControllers)
            {
                interactalbeControllers = Instantiate(AssetLoader.ModelShipControllersPrefab).transform;
                interactalbeControllers.parent = modelShipControllers;
                interactalbeControllers.position = modelShipControllers.position + modelShipControllers.forward*0.25f;
                interactalbeControllers.localRotation = Quaternion.identity;
                interactalbeControllers.localScale = Vector3.one;

                Transform xAxisValueAxis = interactalbeControllers.Find("XZJoystick/XAxisValueAxis");
                Transform yAxisValueAxis = interactalbeControllers.Find("XZJoystick/YAxisValueAxis");
                Transform stickTop = interactalbeControllers.Find("XZJoystick/StickBaseBase/StickBase/StickTop");

                HoldJoystick joystick = stickTop.gameObject.AddComponent<HoldJoystick>();
                joystick.xAxisInputToSimulate = InputConsts.InputCommandType.MOVE_Z;
                joystick.yAxisInputToSimulate = InputConsts.InputCommandType.MOVE_X;
                joystick.xAxisValueAxis = xAxisValueAxis;
                joystick.yAxisValueAxis = yAxisValueAxis;
                return interactalbeControllers;
            }
        }
    }
}
