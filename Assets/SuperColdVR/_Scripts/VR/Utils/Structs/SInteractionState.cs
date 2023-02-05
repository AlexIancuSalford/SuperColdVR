using System;
using UnityEngine;
using UnityEngine.XR;

namespace SuperColdVR.VR.Utils
{
    [Serializable]
    public partial struct SInteractionState
    {
        [Range(0f, 1f)]
        [SerializeField]
        float m_Value;
        public float value
        {
            get => m_Value;
            set => m_Value = value;
        }

        [SerializeField]
        bool m_Active;
        public bool active
        {
            get => m_Active;
            set => m_Active = value;
        }

        bool m_ActivatedThisFrame;
        public bool activatedThisFrame
        {
            get => m_ActivatedThisFrame;
            set => m_ActivatedThisFrame = value;
        }

        bool m_DeactivatedThisFrame;
        public bool deactivatedThisFrame
        {
            get => m_DeactivatedThisFrame;
            set => m_DeactivatedThisFrame = value;
        }
        public void SetFrameState(bool isActive)
        {
            SetFrameState(isActive, isActive ? 1f : 0f);
        }
        public void SetFrameState(bool isActive, float newValue)
        {
            value = newValue;
            activatedThisFrame = !active && isActive;
            deactivatedThisFrame = active && !isActive;
            active = isActive;
        }

        public void SetFrameDependent(bool wasActive)
        {
            activatedThisFrame = !wasActive && active;
            deactivatedThisFrame = wasActive && !active;
        }

        public void ResetFrameDependent()
        {
            activatedThisFrame = false;
            deactivatedThisFrame = false;
        }
    }

    [Serializable]
    public partial class SXRControllerState
    {
        public double time;
        public InputTrackingState inputTrackingState;
        public Vector3 position;
        public Quaternion rotation;
        public SInteractionState selectInteractionState;
        public SInteractionState activateInteractionState;
        public SInteractionState uiPressInteractionState;

        protected SXRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState)
        {
            this.time = time;
            this.position = position;
            this.rotation = rotation;
            this.inputTrackingState = inputTrackingState;
        }

        public SXRControllerState() : this(0d, Vector3.zero, Quaternion.identity, InputTrackingState.None)
        {
        }

        public SXRControllerState(SXRControllerState value)
        {
            this.time = value.time;
            this.position = value.position;
            this.rotation = value.rotation;
            this.inputTrackingState = value.inputTrackingState;
            this.selectInteractionState = value.selectInteractionState;
            this.activateInteractionState = value.activateInteractionState;
            this.uiPressInteractionState = value.uiPressInteractionState;
        }

        public SXRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState,
            bool selectActive, bool activateActive, bool pressActive)
            : this(time, position, rotation, inputTrackingState)
        {
            this.selectInteractionState.SetFrameState(selectActive);
            this.activateInteractionState.SetFrameState(activateActive);
            this.uiPressInteractionState.SetFrameState(pressActive);
        }

        public SXRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState,
            bool selectActive, bool activateActive, bool pressActive,
            float selectValue, float activateValue, float pressValue)
            : this(time, position, rotation, inputTrackingState)
        {
            this.selectInteractionState.SetFrameState(selectActive, selectValue);
            this.activateInteractionState.SetFrameState(activateActive, activateValue);
            this.uiPressInteractionState.SetFrameState(pressActive, pressValue);
        }

        public void ResetFrameDependentStates()
        {
            selectInteractionState.ResetFrameDependent();
            activateInteractionState.ResetFrameDependent();
            uiPressInteractionState.ResetFrameDependent();
        }

        public override string ToString() => $"time: {time}, position: {position}, rotation: {rotation}, selectActive: {selectInteractionState.active}, activateActive: {activateInteractionState.active}, pressActive: {uiPressInteractionState.active}";
    }
}
