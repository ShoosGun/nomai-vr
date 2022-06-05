using System.Collections.Generic;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.Tools;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Input
{
    public class ControllerInput : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private class SimulatedInput 
        {
            public Vector2 AxisValue;
            public InputOverrideType InputOverrideType;
        }
        public enum InputOverrideType 
        {
            NormalOverride,
            ChooseGreater,
            CombineInputs
        }

        private static readonly Dictionary<int, SimulatedInput> simulatedInputs = new Dictionary<int, SimulatedInput>();
        private static readonly List<InputCommandType> inputsToClear = new List<InputCommandType>();

        public static void SimulateInput(InputCommandType commandType)
        {
            SimulateInput(commandType, true);
        }

        public static void SimulateInput(InputCommandType commandType, bool value, bool forOneFrame = true, bool clearInput = false)
        {
            Vector2 valueForTrueBool = new Vector2(1f, 0f);
            Vector2 valueForFalseBool = Vector2.zero;
            SimulateInput(commandType, value ? valueForTrueBool : valueForFalseBool, forOneFrame, clearInput);
        }
        private static void AddSimulatedInput(InputCommandType commandType,Vector2 value, InputOverrideType inputOverrideType)
        {
            if (simulatedInputs.TryGetValue((int)commandType, out SimulatedInput simulatedInput))
            {
                simulatedInput.AxisValue = ChooseInput(simulatedInput.AxisValue, value, inputOverrideType);
            }
            else
            {
                simulatedInputs[(int)commandType] = new SimulatedInput() { AxisValue = value, InputOverrideType = inputOverrideType };
            }
        }
        private static Vector2 ChooseInput(Vector2 originalInput, Vector2 newInput, InputOverrideType inputOverrideType)
        {
            switch (inputOverrideType)
            {
                case InputOverrideType.NormalOverride:
                    return newInput;
                case InputOverrideType.CombineInputs:
                    return Vector2.ClampMagnitude(originalInput + newInput, 1f);
                case InputOverrideType.ChooseGreater:
                    return (newInput.sqrMagnitude > originalInput.sqrMagnitude) ? originalInput : originalInput;
                default:
                    return originalInput;
            }
        }

        public static void SimulateInput(InputCommandType commandType, Vector2 value, bool forOneFrame = true, bool clearInput = false, InputOverrideType inputOverrideType = InputOverrideType.NormalOverride)
        {
            if (clearInput)
            {
                simulatedInputs.Remove((int)commandType);
                return;
            }
            if (forOneFrame)
            {                
                inputsToClear.Add(commandType);
                AddSimulatedInput(commandType, value, inputOverrideType);
            }
            else
            {
                AddSimulatedInput(commandType, value, inputOverrideType);
            }
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Postfix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<InputManager>(nameof(InputManager.Rumble), nameof(DoRumble));
                Postfix<InputManager>(nameof(InputManager.IsGamepadEnabled), nameof(ForceGamepadEnabled));
                Postfix<InputManager>(nameof(InputManager.UsingGamepad), nameof(ForceGamepadEnabled));
                
                Postfix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.HasSameBinding), nameof(PreventSimulatedHasSameBinding));
                Postfix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.HasSameBinding), nameof(PreventSimulatedHasSameBinding));

                VRToolSwapper.ToolEquipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += OnToolUnequipped;
            }

            private static void OnToolUnequipped()
            {
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.RightHand);
            }

            private static void OnToolEquipped()
            {
                if (VRToolSwapper.InteractingHand != null)
                {
                    SteamVR_Actions.tools.Activate(VRToolSwapper.InteractingHand.isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand, 1);
                }
            }
            //Returns true if it has a value to be simulated, which is then returned by axisValue
            private static bool GetSimulatedInput(InputCommandType commandType, out Vector2 axisValue)
            {
                axisValue = Vector2.zero;
                if (!simulatedInputs.TryGetValue((int)commandType, out SimulatedInput simulatedInput))
                    return false;

                axisValue = ChooseInput(axisValue, simulatedInput.AxisValue, simulatedInput.InputOverrideType);
                return true;
            }

            private static void ClearSimulatedInputs()
            {
                foreach (var inputToClear in inputsToClear)
                {
                    simulatedInputs.Remove((int)inputToClear);
                }
                inputsToClear.Clear();
            }
            
            private static void PatchInputCommands(AbstractCommands __instance)
            {
                var commandType = __instance.CommandType;
                if (GetSimulatedInput(commandType, out Vector2 axisValue))
                {
                    ClearSimulatedInputs();
                    __instance.AxisValue = axisValue;
                    return;
                }

                var actionInput = InputMap.GetActionInput(commandType);
                if (actionInput == null) return;
                __instance.AxisValue = actionInput.Value;
            }

            public static void DoRumble(float hiPower, float lowPower)
            {
                if (hiPower <= float.Epsilon && lowPower <= float.Epsilon) return;

                hiPower *= 1.42857146f;
                lowPower *= 1.42857146f;
                var haptic = SteamVR_Actions.default_Haptic;
                var frequency = 0.1f;
                var amplitudeY = lowPower * ModSettings.VibrationStrength;
                var amplitudeX = hiPower * ModSettings.VibrationStrength;
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.LeftHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.LeftHand);
            }

            private static void ForceGamepadEnabled(ref bool __result)
            {
                __result = true;
            }
            
            private static void PreventSimulatedHasSameBinding(ref bool __result, IInputCommands __instance, IInputCommands compare)
            {
                if (GetSimulatedInput(__instance.CommandType,out _) || GetSimulatedInput(compare.CommandType, out _))
                {
                    __result = false;
                }
            }
        }
    }
}