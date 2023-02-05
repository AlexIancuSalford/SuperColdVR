namespace SuperColdVR.VR.Utils
{
    public enum ETrackingOriginMode
    {
        /// <summary>
        /// Uses the default Tracking Origin Mode of the input device.
        /// </summary>
        /// <remarks>
        /// When changing to this value after startup, the Tracking Origin Mode will not be changed.
        /// </remarks>
        NotSpecified,

        /// <summary>
        /// Sets the Tracking Origin Mode to <see cref="TrackingOriginModeFlags.Device"/>.
        /// Input devices will be tracked relative to the first known location.
        /// </summary>
        /// <remarks>
        /// Represents a device-relative tracking origin. A device-relative tracking origin defines a local origin
        /// at the position of the device in space at some previous point in time, usually at a recenter event,
        /// power-on, or AR/VR session start. Pose data provided by the device will be in this space relative to
        /// the local origin. This means that poses returned in this mode will not include the user height (for VR)
        /// or the device height (for AR) and any camera tracking from the XR device will need to be manually offset accordingly.
        /// </remarks>
        /// <seealso cref="TrackingOriginModeFlags.Device"/>
        Device,

        /// <summary>
        /// Sets the Tracking Origin Mode to <see cref="TrackingOriginModeFlags.Floor"/>.
        /// Input devices will be tracked relative to a location on the floor.
        /// </summary>
        /// <remarks>
        /// Represents the tracking origin whereby (0, 0, 0) is on the "floor" or other surface determined by the
        /// XR device being used. The pose values reported by an XR device in this mode will include the height
        /// of the XR device above this surface, removing the need to offset the position of the camera tracking
        /// the XR device by the height of the user (VR) or the height of the device above the floor (AR).
        /// </remarks>
        /// <seealso cref="TrackingOriginModeFlags.Floor"/>
        Floor,
    }
}
