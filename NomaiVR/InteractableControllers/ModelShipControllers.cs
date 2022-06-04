using NomaiVR.Assets;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.InteractableControllers
{
    //TODO add this to NomaiVR ApplyMod()
    public class ModelShipControllers : NomaiVRModule<ModelShipControllers.Behaviour, ModelShipControllers.Behaviour.Patch>
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
                modelShipControllers =  GameObject.Find("");
                SetUpModelShipControllers(modelShipControllers.transform);
            }


            private Transform SetUpModelShipControllers(Transform modelShipControllers)
            {
                interactalbeControllers = Instantiate(AssetLoader.ModelShipControllersPrefab).transform;
                controllers.parent = modelShipControllers;
                controllers.localScale = Vector3.one;
                controllers.localPosition = Vector3.zero;
                controllers.localRotation = Quaternion.identity;

                Transform xAxisValueAxis = controllers.Find("XAxisValueAxis");
                Transform yAxisValueAxis = controllers.Find("YAxisValueAxis");
                Transform stickTop = controllers.Find("StickTop");

                HoldJoystick joystick = stickTop.gameObject.AddComponent<HoldJoystick>();
                joystick.xAxisInputToSimulate = InputConsts.InputCommandType.THRUSTZ;
                joystick.yAxisInputToSimulate = InputConsts.InputCommandType.THRUSTX;
                joystick.xAxisValueAxis = xAxisValueAxis;
                joystick.yAxisValueAxis = yAxisValueAxis;

                return controllers;
            }

            public class Patch : NomaiVRPatch
            {
            }
        }
    }
}
