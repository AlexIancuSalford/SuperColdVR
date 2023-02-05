using SuperColdVR.VR.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace SuperColdVR.VR.Controller
{
    public abstract partial class SXRBaseController : MonoBehaviour
    {
        [SerializeField]
        EUpdateType m_UpdateTrackingType = EUpdateType.UpdateAndBeforeRender;
        public EUpdateType updateTrackingType
        {
            get => m_UpdateTrackingType;
            set => m_UpdateTrackingType = value;
        }

        [SerializeField]
        bool m_EnableInputTracking = true;
        public bool enableInputTracking
        {
            get => m_EnableInputTracking;
            set => m_EnableInputTracking = value;
        }

        [SerializeField]
        bool m_EnableInputActions = true;
        public bool enableInputActions
        {
            get => m_EnableInputActions;
            set => m_EnableInputActions = value;
        }

        [SerializeField]
        Transform m_ModelPrefab;
        public Transform modelPrefab
        {
            get => m_ModelPrefab;
            set => m_ModelPrefab = value;
        }

        [SerializeField, FormerlySerializedAs("m_ModelTransform")]
        Transform m_ModelParent;
        public Transform modelParent
        {
            get => m_ModelParent;
            set
            {
                m_ModelParent = value;

                if (m_Model != null)
                    m_Model.parent = m_ModelParent;
            }
        }

        [SerializeField]
        Transform m_Model;
        public Transform model
        {
            get => m_Model;
            set => m_Model = value;
        }

        [SerializeField]
        bool m_AnimateModel;
        public bool animateModel
        {
            get => m_AnimateModel;
            set => m_AnimateModel = value;
        }

        [SerializeField]
        string m_ModelSelectTransition;
        public string modelSelectTransition
        {
            get => m_ModelSelectTransition;
            set => m_ModelSelectTransition = value;
        }

        [SerializeField]
        string m_ModelDeSelectTransition;
        public string modelDeSelectTransition
        {
            get => m_ModelDeSelectTransition;
            set => m_ModelDeSelectTransition = value;
        }

        bool m_HideControllerModel;
        public bool hideControllerModel
        {
            get => m_HideControllerModel;
            set
            {
                m_HideControllerModel = value;
                if (m_Model != null)
                    m_Model.gameObject.SetActive(!m_HideControllerModel);
            }
        }

        SInteractionState m_SelectInteractionState;
        public SInteractionState selectInteractionState => m_SelectInteractionState;

        SInteractionState m_ActivateInteractionState;
        public SInteractionState activateInteractionState => m_ActivateInteractionState;

        SInteractionState m_UIPressInteractionState;
        public SInteractionState uiPressInteractionState => m_UIPressInteractionState;

        SXRControllerState m_ControllerState;
        public SXRControllerState currentControllerState
        {
            get
            {
                SetupControllerState();
                return m_ControllerState;
            }

            set
            {
                m_ControllerState = value;
                m_CreateControllerState = false;
            }
        }

        bool m_CreateControllerState = true;

#if ANIMATION_MODULE_PRESENT
        /// <summary>
        /// The <see cref="Animator"/> on <see cref="model"/>.
        /// </summary>
        Animator m_ModelAnimator;
#endif

        bool m_HasWarnedAnimatorMissing;

        bool m_PerformSetup = true;

        protected virtual void Awake()
        {
            // Create empty container transform for the model if none specified.
            // This is not strictly necessary to create since this GameObject could be used
            // as the parent for the instantiated prefab, but doing so anyway for backwards compatibility.
            if (m_ModelParent == null)
            {
                m_ModelParent = new GameObject($"[{gameObject.name}] Model Parent").transform;
                m_ModelParent.SetParent(transform, false);
                m_ModelParent.localPosition = Vector3.zero;
                m_ModelParent.localRotation = Quaternion.identity;
            }
        }

        protected virtual void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        protected void Update()
        {
            UpdateController();
        }

        void SetupModel()
        {
            if (m_Model == null)
            {
                var prefab = GetModelPrefab();
                if (prefab != null)
                    m_Model = Instantiate(prefab, m_ModelParent).transform;
            }

            if (m_Model != null)
                m_Model.gameObject.SetActive(!m_HideControllerModel);
        }

        void SetupControllerState()
        {
            if (m_ControllerState == null && m_CreateControllerState)
                m_ControllerState = new SXRControllerState();
        }

        protected virtual GameObject GetModelPrefab()
        {
            return m_ModelPrefab != null ? m_ModelPrefab.gameObject : null;
        }

        protected virtual void UpdateController()
        {
            if (m_PerformSetup)
            {
                SetupModel();
                SetupControllerState();
                m_PerformSetup = false;
            }

            if (m_EnableInputTracking &&
                (m_UpdateTrackingType == EUpdateType.Update ||
                    m_UpdateTrackingType == EUpdateType.UpdateAndBeforeRender))
            {
                UpdateTrackingInput(m_ControllerState);
            }

            if (m_EnableInputActions)
            {
                UpdateInput(m_ControllerState);
                UpdateControllerModelAnimation();
            }

            ApplyControllerState(SXRInteractionUpdateOrder.EUpdatePhase.Dynamic, m_ControllerState);
        }

        protected virtual void OnBeforeRender()
        {
            if (m_EnableInputTracking &&
                (m_UpdateTrackingType == EUpdateType.BeforeRender ||
                    m_UpdateTrackingType == EUpdateType.UpdateAndBeforeRender))
            {
                UpdateTrackingInput(m_ControllerState);
            }

            ApplyControllerState(SXRInteractionUpdateOrder.EUpdatePhase.OnBeforeRender, m_ControllerState);
        }

        protected virtual void FixedUpdate()
        {
            if (m_EnableInputTracking && m_UpdateTrackingType == EUpdateType.Fixed)
            {
                UpdateTrackingInput(m_ControllerState);
            }

            ApplyControllerState(SXRInteractionUpdateOrder.EUpdatePhase.Fixed, m_ControllerState);
        }

        protected virtual void ApplyControllerState(SXRInteractionUpdateOrder.EUpdatePhase updatePhase, SXRControllerState controllerState)
        {
            if (controllerState == null)
                return;

            if (updatePhase == SXRInteractionUpdateOrder.EUpdatePhase.Dynamic)
            {
                // Sync the controller actions from the interaction state in the controller
                m_SelectInteractionState = controllerState.selectInteractionState;
                m_ActivateInteractionState = controllerState.activateInteractionState;
                m_UIPressInteractionState = controllerState.uiPressInteractionState;
            }

            if (updatePhase == SXRInteractionUpdateOrder.EUpdatePhase.Dynamic ||
                updatePhase == SXRInteractionUpdateOrder.EUpdatePhase.OnBeforeRender ||
                updatePhase == SXRInteractionUpdateOrder.EUpdatePhase.Fixed)
            {
                if ((controllerState.inputTrackingState & InputTrackingState.Position) != 0)
                {
                    transform.localPosition = controllerState.position;
                }

                if ((controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
                {
                    transform.localRotation = controllerState.rotation;
                }
            }
        }

        protected virtual void UpdateTrackingInput(SXRControllerState controllerState)
        {
        }

        protected virtual void UpdateInput(SXRControllerState controllerState)
        {
        }

        protected virtual void UpdateControllerModelAnimation()
        {
#if ANIMATION_MODULE_PRESENT
            if (m_AnimateModel && m_Model != null)
            {
                // Update the Animator reference if necessary
                if (m_ModelAnimator == null || m_ModelAnimator.gameObject != m_Model.gameObject)
                {
                    if (!m_Model.TryGetComponent(out m_ModelAnimator))
                    {
                        if (!m_HasWarnedAnimatorMissing)
                        {
                            Debug.LogWarning("Animate Model is enabled, but there is no Animator component on the model." +
                                " Unable to activate named triggers to animate the model.", this);
                            m_HasWarnedAnimatorMissing = true;
                        }

                        return;
                    }
                }

                if (m_SelectInteractionState.activatedThisFrame)
                    m_ModelAnimator.SetTrigger(m_ModelSelectTransition);
                else if (m_SelectInteractionState.deactivatedThisFrame)
                    m_ModelAnimator.SetTrigger(m_ModelDeSelectTransition);
            }
#endif
        }

        public virtual bool SendHapticImpulse(float amplitude, float duration) => false;
    }
}
