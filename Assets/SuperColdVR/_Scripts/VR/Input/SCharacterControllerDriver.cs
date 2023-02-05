using SuperColdVR.VR.Core;
using SuperColdVR.VR.Locomotion.Base;
using SuperColdVR.VR.Locomotion.Continuous;
using UnityEngine;

namespace SuperColdVR.VR.Input
{
    public partial class SCharacterControllerDriver : MonoBehaviour
    {
        [SerializeField]
        SLocomotionProvider m_LocomotionProvider;
        public SLocomotionProvider locomotionProvider
        {
            get => m_LocomotionProvider;
            set
            {
                Unsubscribe(m_LocomotionProvider);
                m_LocomotionProvider = value;
                Subscribe(m_LocomotionProvider);

                SetupCharacterController();
                UpdateCharacterController();
            }
        }

        [SerializeField]
        float m_MinHeight;
        public float minHeight
        {
            get => m_MinHeight;
            set => m_MinHeight = value;
        }

        [SerializeField]
        float m_MaxHeight = float.PositiveInfinity;
        public float maxHeight
        {
            get => m_MaxHeight;
            set => m_MaxHeight = value;
        }

        SXROrigin m_XROrigin;
        protected SXROrigin xrOrigin => m_XROrigin;

        CharacterController m_CharacterController;
        protected CharacterController characterController => m_CharacterController;

        protected void Awake()
        {
            if (m_LocomotionProvider == null)
            {
                m_LocomotionProvider = GetComponent<SContinuousMoveProviderBase>();
                if (m_LocomotionProvider == null)
                {
                    m_LocomotionProvider = FindObjectOfType<SContinuousMoveProviderBase>();
                    if (m_LocomotionProvider == null)
                        Debug.LogWarning("Unable to drive properties of the Character Controller without the locomotion events of a Locomotion Provider." +
                            " Set Locomotion Provider or ensure a Continuous Move Provider component is in your scene.", this);
                }
            }
        }

        protected void OnEnable()
        {
            Subscribe(m_LocomotionProvider);
        }

        protected void OnDisable()
        {
            Unsubscribe(m_LocomotionProvider);
        }

        protected void Start()
        {
            SetupCharacterController();
            UpdateCharacterController();
        }

        protected virtual void UpdateCharacterController()
        {
            if (m_XROrigin == null || m_CharacterController == null)
                return;

            var height = Mathf.Clamp(m_XROrigin.CameraInOriginSpaceHeight, m_MinHeight, m_MaxHeight);

            Vector3 center = m_XROrigin.CameraInOriginSpacePos;
            center.y = height / 2f + m_CharacterController.skinWidth;

            m_CharacterController.height = height;
            m_CharacterController.center = center;
        }

        void Subscribe(SLocomotionProvider provider)
        {
            if (provider != null)
            {
                provider.beginLocomotion += OnBeginLocomotion;
                provider.endLocomotion += OnEndLocomotion;
            }
        }

        void Unsubscribe(SLocomotionProvider provider)
        {
            if (provider != null)
            {
                provider.beginLocomotion -= OnBeginLocomotion;
                provider.endLocomotion -= OnEndLocomotion;
            }
        }

        void SetupCharacterController()
        {
            if (m_LocomotionProvider == null || m_LocomotionProvider.LocomotionSystem.XRRig == null)
                return;

            m_XROrigin = m_LocomotionProvider.LocomotionSystem.XRRig;
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            m_CharacterController = m_XROrigin != null ? m_XROrigin.OriginBaseGameObject.GetComponent<CharacterController>() : null;
#pragma warning restore IDE0031

            if (m_CharacterController == null && m_XROrigin != null)
            {
                Debug.LogError($"Could not get CharacterController on {m_XROrigin.OriginBaseGameObject}, unable to drive properties." +
                    $" Ensure there is a CharacterController on the \"Rig\" GameObject of {m_XROrigin}.",
                    this);
            }
        }

        void OnBeginLocomotion(SLocomotionSystem system)
        {
            UpdateCharacterController();
        }

        void OnEndLocomotion(SLocomotionSystem system)
        {
            UpdateCharacterController();
        }
    }
}
