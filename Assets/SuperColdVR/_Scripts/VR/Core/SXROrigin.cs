using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using SuperColdVR.VR.Utils;
#if INCLUDE_INPUT_SYSTEM
using UnityEngine.InputSystem.XR;
#endif
#if INCLUDE_LEGACY_INPUT_HELPERS
using UnityEngine.SpatialTracking;
#endif

namespace SuperColdVR.VR.Core
{
    [DisallowMultipleComponent]
    public class SXROrigin : MonoBehaviour
    {
        [field : SerializeField] public Camera Camera { get; private set; }

        public Transform TrackablesParent { get; private set; }
        public event Action<SARTrackablesParentTransformChangedEventArgs> TrackablesParentTransformChanged;

        //This is the average seated height, which is 44 inches.
        private const float defaultCameraYOffset = 1.1176f;

        [field : SerializeField, FormerlySerializedAs("m_RigBaseGameObject")]
        public GameObject OriginBaseGameObject { get; private set; }

        [SerializeField]
        GameObject cameraFloorOffsetObject;
        public GameObject CameraFloorOffsetObject
        {
            get => cameraFloorOffsetObject;
            set
            {
                cameraFloorOffsetObject = value;
                MoveOffsetHeight();
            }
        }

        [SerializeField]
        private ETrackingOriginMode requestedTrackingOriginMode = ETrackingOriginMode.NotSpecified;

        public ETrackingOriginMode RequestedTrackingOriginMode
        {
            get => requestedTrackingOriginMode;
            set
            {
                requestedTrackingOriginMode = value;
                TryInitializeCamera();
            }
        }

        [SerializeField]
        private float cameraYOffset = defaultCameraYOffset;

        public float CameraYOffset
        {
            get => cameraYOffset;
            set
            {
                cameraYOffset = value;
                MoveOffsetHeight();
            }
        }

        public TrackingOriginModeFlags CurrentTrackingOriginMode { get; private set; }
        public Vector3 OriginInCameraSpacePos => Camera.transform.InverseTransformPoint(OriginBaseGameObject.transform.position);
        public Vector3 CameraInOriginSpacePos => OriginBaseGameObject.transform.InverseTransformPoint(Camera.transform.position);
        public float CameraInOriginSpaceHeight => CameraInOriginSpacePos.y;
        private static readonly List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();

        // Bookkeeping to track lazy initialization of the tracking origin mode type.
        private bool cameraInitialized;
        private bool cameraInitializing;

        private void MoveOffsetHeight()
        {
            if (!Application.isPlaying) { return; }

            switch (CurrentTrackingOriginMode)
            {
                case TrackingOriginModeFlags.Floor:
                    MoveOffsetHeight(0f);
                    break;
                case TrackingOriginModeFlags.Device:
                    MoveOffsetHeight(cameraYOffset);
                    break;
                default:
                    return;
            }
        }

        private void MoveOffsetHeight(float y)
        {
            if (cameraFloorOffsetObject != null)
            {
                Transform offsetTransform = cameraFloorOffsetObject.transform;
                Vector3 desiredPosition = offsetTransform.localPosition;
                desiredPosition.y = y;
                offsetTransform.localPosition = desiredPosition;
            }
        }

        private void TryInitializeCamera()
        {
            if (!Application.isPlaying) { return; }

            cameraInitialized = SetupCamera();
            if (!cameraInitialized & !cameraInitializing) { StartCoroutine(RepeatInitializeCamera()); }
        }

        private bool SetupCamera()
        {
            bool initialized = true;

            SubsystemManager.GetInstances(inputSubsystems);
            if (inputSubsystems.Count > 0)
            {
                foreach (XRInputSubsystem inputSubsystem in inputSubsystems)
                {
                    if (SetupCamera(inputSubsystem))
                    {
                        // It is possible this could happen more than
                        // once so unregister the callback first just in case.
                        inputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated;
                        inputSubsystem.trackingOriginUpdated += OnInputSubsystemTrackingOriginUpdated;
                    }
                    else
                    {
                        initialized = false;
                    }
                }
            }

            return initialized;
        }

        private bool SetupCamera(XRInputSubsystem inputSubsystem)
        {
            if (inputSubsystem == null) { return false; }

            bool successful = true;

            switch (requestedTrackingOriginMode)
            {
                case ETrackingOriginMode.NotSpecified:
                    CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
                    break;
                case ETrackingOriginMode.Device:
                case ETrackingOriginMode.Floor:
                    {
                        TrackingOriginModeFlags supportedModes = inputSubsystem.GetSupportedTrackingOriginModes();

                        // We need to check for Unknown because we may not be in a state where we can read this data yet.
                        if (supportedModes == TrackingOriginModeFlags.Unknown) { return false; }

                        // Convert from the request enum to the flags enum that is used by the subsystem
                        TrackingOriginModeFlags equivalentFlagsMode = requestedTrackingOriginMode == ETrackingOriginMode.Device
                            ? TrackingOriginModeFlags.Device
                            : TrackingOriginModeFlags.Floor;

                        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags -- Treated like Flags enum when querying supported modes
                        if ((supportedModes & equivalentFlagsMode) == 0)
                        {
                            requestedTrackingOriginMode = ETrackingOriginMode.NotSpecified;
                            CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
                            Debug.LogWarning($"Attempting to set the tracking origin mode to {equivalentFlagsMode}, but that is not supported by the SDK." +
                                $" Supported types: {supportedModes:F}. Using the current mode of {CurrentTrackingOriginMode} instead.", this);
                        }
                        else
                        {
                            successful = inputSubsystem.TrySetTrackingOriginMode(equivalentFlagsMode);
                        }
                    }
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(ETrackingOriginMode)}={requestedTrackingOriginMode}");
                    return false;
            }

            if (successful) { MoveOffsetHeight(); }

            if (CurrentTrackingOriginMode == TrackingOriginModeFlags.Device || requestedTrackingOriginMode == ETrackingOriginMode.Device)
            { 
                successful = inputSubsystem.TryRecenter(); 
            }

            return successful;
        }

        private IEnumerator RepeatInitializeCamera()
        {
            cameraInitializing = true;
            while (!cameraInitialized)
            {
                yield return null;
                if (!cameraInitialized) { cameraInitialized = SetupCamera(); }
            }
            cameraInitializing = false;
        }

        private void OnInputSubsystemTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
        {
            CurrentTrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
            MoveOffsetHeight();
        }

        public bool RotateAroundCameraUsingOriginUp(float angleDegrees)
        {
            return RotateAroundCameraPosition(OriginBaseGameObject.transform.up, angleDegrees);
        }

        public bool RotateAroundCameraPosition(Vector3 vector, float angleDegrees)
        {
            if (Camera == null || OriginBaseGameObject == null) { return false; }

            // Rotate around the camera position
            OriginBaseGameObject.transform.RotateAround(Camera.transform.position, vector, angleDegrees);

            return true;
        }

        public bool MatchOriginUp(Vector3 destinationUp)
        {
            if (OriginBaseGameObject == null) { return false; }

            if (OriginBaseGameObject.transform.up == destinationUp) { return true; }

            Quaternion rigUp = Quaternion.FromToRotation(OriginBaseGameObject.transform.up, destinationUp);
            OriginBaseGameObject.transform.rotation = rigUp * transform.rotation;

            return true;
        }

        public bool MatchOriginUpCameraForward(Vector3 destinationUp, Vector3 destinationForward)
        {
            if (Camera != null && MatchOriginUp(destinationUp))
            {
                // Project current camera's forward vector on the destination plane, whose normal vector is destinationUp.
                Vector3 projectedCamForward = Vector3.ProjectOnPlane(Camera.transform.forward, destinationUp).normalized;

                // The angle that we want the XROrigin to rotate is the signed angle between projectedCamForward and destinationForward, after the up vectors are matched.
                float signedAngle = Vector3.SignedAngle(projectedCamForward, destinationForward, destinationUp);

                RotateAroundCameraPosition(destinationUp, signedAngle);

                return true;
            }

            return false;
        }

        public bool MatchOriginUpOriginForward(Vector3 destinationUp, Vector3 destinationForward)
        {
            if (OriginBaseGameObject != null && MatchOriginUp(destinationUp))
            {
                // The angle that we want the XR Origin to rotate is the signed angle between the origin's forward and destinationForward, after the up vectors are matched.
                float signedAngle = Vector3.SignedAngle(OriginBaseGameObject.transform.forward, destinationForward, destinationUp);

                RotateAroundCameraPosition(destinationUp, signedAngle);

                return true;
            }

            return false;
        }

        public bool MoveCameraToWorldLocation(Vector3 desiredWorldLocation)
        {
            if (Camera == null) { return false; }

            Matrix4x4 rot = Matrix4x4.Rotate(Camera.transform.rotation);
            Vector3 delta = rot.MultiplyPoint3x4(OriginInCameraSpacePos);
            OriginBaseGameObject.transform.position = delta + desiredWorldLocation;

            return true;
        }

        protected void Awake()
        {
            if (cameraFloorOffsetObject == null)
            {
                Debug.LogWarning("No Camera Floor Offset Object specified for XR Origin, using attached GameObject.", this);
                cameraFloorOffsetObject = gameObject;
            }

            if (Camera == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null) { Camera = mainCamera; }
                else { Debug.LogWarning("No Main Camera is found for XR Origin, please assign the Camera field manually.", this); }
            }

            // This will be the parent GameObject for any trackables (such as planes) for which
            // we want a corresponding GameObject.
            TrackablesParent = (new GameObject("Trackables")).transform;
            TrackablesParent.SetParent(transform, false);
            TrackablesParent.localPosition = Vector3.zero;
            TrackablesParent.localRotation = Quaternion.identity;
            TrackablesParent.localScale = Vector3.one;

            if (Camera)
            {
#if INCLUDE_INPUT_SYSTEM && INCLUDE_LEGACY_INPUT_HELPERS
                var trackedPoseDriver = m_Camera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                var trackedPoseDriverOld = m_Camera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                if (trackedPoseDriver == null && trackedPoseDriverOld == null)
                {
                    Debug.LogWarning(
                        $"Camera \"{m_Camera.name}\" does not use a Tracked Pose Driver (Input System), " +
                        "so its transform will not be updated by an XR device.  In order for this to be " +
                        "updated, please add a Tracked Pose Driver (Input System) with bindings for position and rotation of the center eye.", this);
                }
#elif !INCLUDE_INPUT_SYSTEM && INCLUDE_LEGACY_INPUT_HELPERS
                var trackedPoseDriverOld = m_Camera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                if (trackedPoseDriverOld == null)
                {
                    Debug.LogWarning(
                        $"Camera \"{m_Camera.name}\" does not use a Tracked Pose Driver, and com.unity.xr.legacyinputhelpers is installed. " +
                        "Although the Tracked Pose Driver from Legacy Input Helpers can be used, it is recommended to " +
                        "install com.unity.inputsystem instead and add a Tracked Pose Driver (Input System) with bindings for position and rotation of the center eye.", this);
                }
#elif INCLUDE_INPUT_SYSTEM && !INCLUDE_LEGACY_INPUT_HELPERS
                var trackedPoseDriver = m_Camera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (trackedPoseDriver == null)
                {
                    Debug.LogWarning(
                        $"Camera \"{m_Camera.name}\" does not use a Tracked Pose Driver (Input System), " +
                        "so its transform will not be updated by an XR device.  In order for this to be " +
                        "updated, please add a Tracked Pose Driver (Input System) with bindings for position and rotation of the center eye.", this);
                }
#elif !INCLUDE_INPUT_SYSTEM && !INCLUDE_LEGACY_INPUT_HELPERS
                Debug.LogWarning(
                    $"Camera \"{Camera.name}\" does not use a Tracked Pose Driver and com.unity.inputsystem is not installed, " +
                    "so its transform will not be updated by an XR device.  In order for this to be " +
                    "updated, please install com.unity.inputsystem and add a Tracked Pose Driver (Input System) with bindings for position and rotation of the center eye.", this);
#endif
            }
        }

        private Pose GetCameraOriginPose()
        {
            Pose localOriginPose = Pose.identity;
            Transform parent = Camera.transform.parent;

            return parent
                ? parent.TransformPose(localOriginPose)
                : localOriginPose;
        }

        protected void OnEnable() => Application.onBeforeRender += OnBeforeRender;

        protected void OnDisable() => Application.onBeforeRender -= OnBeforeRender;

        private void OnBeforeRender()
        {
            if (Camera)
            {
                Pose pose = GetCameraOriginPose();
                TrackablesParent.SetPositionAndRotation(pose.position, pose.rotation);
            }

            if (TrackablesParent.hasChanged)
            {
                TrackablesParentTransformChanged?.Invoke(
                    new SARTrackablesParentTransformChangedEventArgs(this, TrackablesParent));
                TrackablesParent.hasChanged = false;
            }
        }

        protected void OnValidate()
        {
            if (OriginBaseGameObject == null)
                OriginBaseGameObject = gameObject;

            if (Application.isPlaying && isActiveAndEnabled)
            {
                // Respond to the mode changing by re-initializing the camera,
                // or just update the offset height in order to avoid recentering.
                if (IsModeStale()) { TryInitializeCamera(); }
                else { MoveOffsetHeight(); }
            }

            bool IsModeStale()
            {
                if (inputSubsystems.Count > 0)
                {
                    foreach (XRInputSubsystem inputSubsystem in inputSubsystems)
                    {
                        // Convert from the request enum to the flags enum that is used by the subsystem
                        TrackingOriginModeFlags equivalentFlagsMode;
                        switch (requestedTrackingOriginMode)
                        {
                            case ETrackingOriginMode.NotSpecified:
                                // Don't need to initialize the camera since we don't set the mode when NotSpecified (we just keep the current value)
                                return false;
                            case ETrackingOriginMode.Device:
                                equivalentFlagsMode = TrackingOriginModeFlags.Device;
                                break;
                            case ETrackingOriginMode.Floor:
                                equivalentFlagsMode = TrackingOriginModeFlags.Floor;
                                break;
                            default:
                                Assert.IsTrue(false, $"Unhandled {nameof(ETrackingOriginMode)}={requestedTrackingOriginMode}");
                                return false;
                        }

                        if (inputSubsystem != null && inputSubsystem.GetTrackingOriginMode() != equivalentFlagsMode)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        protected void Start()
        {
            TryInitializeCamera();
        }

        protected void OnDestroy()
        {
            foreach (XRInputSubsystem inputSubsystem in inputSubsystems)
            {
                if (inputSubsystem != null) { inputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated; }
            }
        }
    }
}
