using SuperColdVR.VR.Utils.Enums;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SuperColdVR.VR.Locomotion.Base
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_LocomotionProviders)]
    public abstract partial class SLocomotionProvider : MonoBehaviour
    {
        public event Action<SLocomotionSystem> beginLocomotion;
        public event Action<SLocomotionSystem> endLocomotion;

        public SLocomotionSystem LocomotionSystem { get; private set; }
        public ELocomotionPhase LocomotionPhase { get; protected set; }

        protected virtual void Awake()
        {
            LocomotionSystem = GetComponent<SLocomotionSystem>();
        }

        protected bool CanBeginLocomotion()
        {
            if (LocomotionSystem == null)
                return false;

            return !LocomotionSystem.IsBusy;
        }

        protected bool BeginLocomotion()
        {
            if (LocomotionSystem == null)
                return false;

            var success = LocomotionSystem.RequestExclusiveOperation(this) == ERequestResult.Success;
            if (success)
                beginLocomotion?.Invoke(LocomotionSystem);

            return success;
        }

        protected bool EndLocomotion()
        {
            if (LocomotionSystem == null)
                return false;

            var success = LocomotionSystem.FinishExclusiveOperation(this) == ERequestResult.Success;
            if (success)
                endLocomotion?.Invoke(LocomotionSystem);

            return success;
        }
    }
}
