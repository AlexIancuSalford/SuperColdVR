using UnityEngine.InputSystem;

namespace SuperColdVR.VR.Input
{
    public static class SInputActionPropertyExtensions
    {
        public static void EnableDirectAction(this InputActionProperty property)
        {
            if (property.reference != null) { return; }

            property.action?.Enable();
        }

        public static void DisableDirectAction(this InputActionProperty property)
        {
            if (property.reference != null) { return; }

            property.action?.Disable();
        }
    }
}
