using UnityEngine;

namespace SuperColdVR.VR.Utils
{
    public static class SCardinalUtility
    {
        public static ECardinal GetNearestCardinal(Vector2 value)
        {
            var angle = Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;
            var absAngle = Mathf.Abs(angle);
            if (absAngle < 45f) { return ECardinal.East; }
            if (absAngle > 135f) { return ECardinal.West; }
            
            return angle >= 0f ? ECardinal.North : ECardinal.South;
        }
    }
}
