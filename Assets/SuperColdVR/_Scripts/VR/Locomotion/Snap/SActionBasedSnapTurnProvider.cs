using SuperColdVR.VR.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperColdVR.VR.Locomotion.Snap
{
    public class SActionBasedSnapTurnProvider : SnapTurnProviderBase
    {
        [SerializeField] private InputActionProperty leftHandSnapTurnAction;
        public InputActionProperty LeftHandSnapTurnAction
        {
            get => leftHandSnapTurnAction;
            set => SetInputActionProperty(ref leftHandSnapTurnAction, value);
        }

        [SerializeField] private InputActionProperty rightHandSnapTurnAction;
        public InputActionProperty RightHandSnapTurnAction
        {
            get => rightHandSnapTurnAction;
            set => SetInputActionProperty(ref rightHandSnapTurnAction, value);
        }

        protected void OnEnable()
        {
            leftHandSnapTurnAction.EnableDirectAction();
            rightHandSnapTurnAction.EnableDirectAction();
        }

        protected void OnDisable()
        {
            leftHandSnapTurnAction.DisableDirectAction();
            rightHandSnapTurnAction.DisableDirectAction();
        }

        protected override Vector2 ReadInput()
        {
            Vector2 leftHandValue = leftHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector2 rightHandValue = rightHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

            return leftHandValue + rightHandValue;
        }

        private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying) { property.DisableDirectAction(); }

            property = value;

            if (Application.isPlaying && isActiveAndEnabled) { property.EnableDirectAction(); }
        }
    }
}
