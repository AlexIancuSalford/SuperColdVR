using SuperColdVR.VR.Core;
using SuperColdVR.VR.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperColdVR.VR.Utils
{
    public readonly struct SARTrackablesParentTransformChangedEventArgs : IEquatable<SARTrackablesParentTransformChangedEventArgs>
    {
        public SXROrigin Origin { get; }

        public Transform TrackablesParent { get; }

        public SARTrackablesParentTransformChangedEventArgs(SXROrigin origin, Transform trackablesParent)
        {
            if (origin == null)
            {
                throw new ArgumentNullException("origin");
            }

            if (trackablesParent == null)
            {
                throw new ArgumentNullException("trackablesParent");
            }

            Origin = origin;
            TrackablesParent = trackablesParent;
        }

        public bool Equals(SARTrackablesParentTransformChangedEventArgs other)
        {
            if (Origin == other.Origin)
            {
                return TrackablesParent == other.TrackablesParent;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is SARTrackablesParentTransformChangedEventArgs)
            {
                SARTrackablesParentTransformChangedEventArgs other = (SARTrackablesParentTransformChangedEventArgs)obj;
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return CHashCodeUtil.Combine(CHashCodeUtil.ReferenceHash(Origin), CHashCodeUtil.ReferenceHash(TrackablesParent));
        }

        public static bool operator ==(SARTrackablesParentTransformChangedEventArgs lhs, SARTrackablesParentTransformChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SARTrackablesParentTransformChangedEventArgs lhs, SARTrackablesParentTransformChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
