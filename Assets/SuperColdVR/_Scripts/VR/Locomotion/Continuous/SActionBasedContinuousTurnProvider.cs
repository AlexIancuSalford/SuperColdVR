using SuperColdVR.VR.Input;
using SuperColdVR.VR.Locomotion.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperColdVR.VR.Locomotion.Continuous
{
    public class SActionBasedContinuousTurnProvider : SContinuousTurnProviderBase
    {
        [SerializeField] private InputActionProperty leftHandTurnAction;
        public InputActionProperty LeftHandTurnAction
        {
            get => leftHandTurnAction;
            set => SetInputActionProperty(ref leftHandTurnAction, value);
        }

        [SerializeField] private InputActionProperty rightHandTurnAction;
        public InputActionProperty RightHandTurnAction
        {
            get => rightHandTurnAction;
            set => SetInputActionProperty(ref rightHandTurnAction, value);
        }

        protected void OnEnable()
        {
            leftHandTurnAction.EnableDirectAction();
            rightHandTurnAction.EnableDirectAction();
        }

        protected void OnDisable()
        {
            leftHandTurnAction.DisableDirectAction();
            rightHandTurnAction.DisableDirectAction();
        }

        protected override Vector2 ReadInput()
        {
            Vector2 leftHandValue = leftHandTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector2 rightHandValue = rightHandTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

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
