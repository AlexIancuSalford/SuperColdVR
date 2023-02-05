using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperColdVR.VR.Utils
{
    public enum EUpdateType
    {
        /// <summary>
        /// Sample input at both update and directly before rendering. For smooth controller pose tracking,
        /// we recommend using this value as it will provide the lowest input latency for the device.
        /// This is the default value for the UpdateType option.
        /// </summary>
        UpdateAndBeforeRender,

        /// <summary>
        /// Only sample input during the update phase of the frame.
        /// </summary>
        Update,

        /// <summary>
        /// Only sample input directly before rendering.
        /// </summary>
        BeforeRender,

        /// <summary>
        /// Sample input corresponding to <see cref="FixedUpdate"/>.
        /// </summary>
        Fixed,
    }
}
