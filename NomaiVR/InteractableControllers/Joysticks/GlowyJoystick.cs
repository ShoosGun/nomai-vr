using UnityEngine;

namespace NomaiVR.InteractableControllers.Joysticks
{
    public abstract class GlowyJoystick : MonoBehaviour
    {
        public enum JoystickState
        {
            PreInit,
            Disabled,
            Enabled,
            Focused,
            Active,
        }

        private static readonly int shaderColor = Shader.PropertyToID("_Color");
        private static readonly Color disabledColor = new Color(0, 0, 0, 0);
        private static readonly Color enabledColor = new Color(2.12f, 1.57f, 1.33f, 0.04f);
        private static readonly Color hoverColor = new Color(2.12f, 1.67f, 1.33f, 0.1f);
        private static readonly Color activeColor = new Color(2.11f, 1.67f, 1.33f, 0.2f);
        private JoystickState joystickState = JoystickState.PreInit;
        private Material joystickMaterial;
        private Collider collider;

        public JoystickState State => joystickState;

        private void Awake()
        {
            joystickMaterial = GetComponent<Renderer>().material;
            collider = GetComponent<Collider>();
            Initialize();
        }

        protected void Update()
        {
            if (IsJoystickEnabled())
            {
                if (IsJoystickActive())
                {
                    SetState(JoystickState.Active);
                }
                else if (IsJoystickFocused())
                {
                    SetState(JoystickState.Focused);
                }
                else
                {
                    SetState(JoystickState.Enabled);
                }
            }
            else
            {
                SetState(JoystickState.Disabled);
            }
        }

        protected abstract void Initialize();
        protected abstract bool IsJoystickActive();
        protected abstract bool IsJoystickFocused();
        protected abstract bool IsJoystickEnabled();

        private void SetJoystickTopColor(Color color)
        {
            joystickMaterial.SetColor(shaderColor, color);
        }

        private void SetState(JoystickState nextState, JoystickState? previousState = null)
        {
            if (nextState == joystickState) return;
            if (previousState != null && previousState != joystickState) return;

            if (collider != null) collider.enabled = nextState != JoystickState.Disabled;
            switch (nextState)
            {
                case JoystickState.Disabled:
                    SetJoystickTopColor(disabledColor);
                    break;
                case JoystickState.Focused:
                    SetJoystickTopColor(hoverColor);
                    break;
                case JoystickState.Active:
                    SetJoystickTopColor(activeColor);
                    break;
                default:
                    SetJoystickTopColor(enabledColor);
                    break;
            }

            joystickState = nextState;
        }
    }
}