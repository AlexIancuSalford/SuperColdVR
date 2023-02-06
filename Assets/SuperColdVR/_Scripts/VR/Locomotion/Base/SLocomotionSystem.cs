using SuperColdVR.VR.Core;
using SuperColdVR.VR.Utils;
using UnityEngine;

namespace SuperColdVR.VR.Locomotion.Base
{
    public partial class SLocomotionSystem : MonoBehaviour
    {
        private SLocomotionProvider CurrentExclusiveProvider { get; set; }
        private float TimeMadeExclusive { get; set; }
        public SXROrigin XRRig { get; private set; } = null;

        [field : SerializeField] public float OperationTimeout { get; private set; } = 10f;

        public bool IsBusy => CurrentExclusiveProvider != null;

        protected void Awake()
        {
            XRRig = GetComponent<SXROrigin>();
        }

        protected void Update()
        {
            if (CurrentExclusiveProvider != null && Time.time > TimeMadeExclusive + OperationTimeout)
            {
                ResetExclusivity();
            }
        }

        public ERequestResult RequestExclusiveOperation(SLocomotionProvider provider)
        {
            if (provider == null) { return ERequestResult.Error; }

            if (CurrentExclusiveProvider == null)
            {
                CurrentExclusiveProvider = provider;
                TimeMadeExclusive = Time.time;
                return ERequestResult.Success;
            }

            return CurrentExclusiveProvider != provider ? ERequestResult.Busy : ERequestResult.Error;
        }

        private void ResetExclusivity()
        {
            CurrentExclusiveProvider = null;
            TimeMadeExclusive = 0f;
        }

        public ERequestResult FinishExclusiveOperation(SLocomotionProvider provider)
        {
            if (provider == null || CurrentExclusiveProvider == null)
                return ERequestResult.Error;

            if (CurrentExclusiveProvider == provider)
            {
                ResetExclusivity();
                return ERequestResult.Success;
            }

            return ERequestResult.Error;
        }
    }
}
