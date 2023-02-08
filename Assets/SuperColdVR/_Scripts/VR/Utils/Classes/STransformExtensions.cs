using System;
using UnityEngine;

namespace SuperColdVR.VR.Utils
{
    public static class STransformExtensions
    {
        public static Pose GetLocalPose(this Transform transform)
        {
            return new Pose(transform.localPosition, transform.localRotation);
        }

        public static Pose GetWorldPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        public static void SetLocalPose(this Transform transform, Pose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
        }

        public static void SetWorldPose(this Transform transform, Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }

        public static Pose TransformPose(this Transform transform, Pose pose)
        {
            return pose.GetTransformedBy(transform);
        }

        public static Pose InverseTransformPose(this Transform transform, Pose pose)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Pose
            {
                position = transform.InverseTransformPoint(pose.position),
                rotation = Quaternion.Inverse(transform.rotation) * pose.rotation
            };
        }

        public static Ray InverseTransformRay(this Transform transform, Ray ray)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Ray(
                transform.InverseTransformPoint(ray.origin),
                transform.InverseTransformDirection(ray.direction));
        }

    }
}
