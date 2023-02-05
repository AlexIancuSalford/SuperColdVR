namespace SuperColdVR.VR.Utils
{
    public static class SXRInteractionUpdateOrder
    {
        public enum EUpdatePhase
        {
            Fixed,
            Dynamic,
            Late,
            OnBeforeRender
        }

        public const int k_ControllerRecorder = -30000;

        public const int k_DeviceSimulator = -29991;

        public const int k_Controllers = -29990;

        public const int k_LocomotionProviders = -210;

        public const int k_TwoHandedGrabMoveProviders = -209;

        public const int k_UIInputModule = -200;

        public const int k_InteractionManager = -100;

        public const int k_Interactors = -99;

        public const int k_Interactables = -98;

        public const int k_LineVisual = 100;

        public const int k_BeforeRenderOrder = 100;

        public const int k_BeforeRenderLineVisual = 101;
    }
}
