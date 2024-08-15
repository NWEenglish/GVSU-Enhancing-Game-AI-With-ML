using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class TransformExtension
    {
        public static float GetDistanceTo(this Transform transform, Transform other)
        {
            return Vector3.Distance(transform.position, other.position);
        }
    }
}
