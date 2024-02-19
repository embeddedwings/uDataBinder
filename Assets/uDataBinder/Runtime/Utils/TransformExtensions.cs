using UnityEngine;

namespace uDataBinder.Utils
{
    public static class TransformExtensions
    {
        public static T GetComponentInParentIncludeInactive<T>(this Transform transform) where T : MonoBehaviour
        {
            while (transform != null)
            {
                if (transform.TryGetComponent<T>(out var component))
                {
                    return component;
                }
                transform = transform.parent;
            }
            return default;
        }
    }
}