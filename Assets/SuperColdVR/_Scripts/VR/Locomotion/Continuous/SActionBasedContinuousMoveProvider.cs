using SuperColdVR.VR.Locomotion.Continuous;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace SuperColdVR.VR.Locomotion
{
    public class SActionBasedContinuousMoveProvider : SContinuousMoveProviderBase
    {
        [SerializeField] InputActionProperty leftHandMoveAction;
        public InputActionProperty LeftHandMoveAction
        {
            get => leftHandMoveAction;
            set => SetInputActionProperty(ref leftHandMoveAction, value);
        }

        [SerializeField] private InputActionProperty rightHandMoveAction;
        public InputActionProperty RightHandMoveAction
        {
            get => rightHandMoveAction;
            set => SetInputActionProperty(ref rightHandMoveAction, value);
        }

        protected void OnEnable()
        {
            leftHandMoveAction.EnableDirectAction();
            rightHandMoveAction.EnableDirectAction();
        }

        protected void OnDisable()
        {
            leftHandMoveAction.DisableDirectAction();
            rightHandMoveAction.DisableDirectAction();
        }

        protected override Vector2 ReadInput()
        {
            Vector2 leftHandValue = leftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector2 rightHandValue = rightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

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
