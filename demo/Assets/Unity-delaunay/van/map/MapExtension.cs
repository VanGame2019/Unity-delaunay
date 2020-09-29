using UnityEngine;

namespace van.map
{
    public static class MapExtension
    {
        public static bool MyEquals(this Vector2 v1, Vector2 v2)
        {
            if (Mathf.Abs(v1.x - v2.x) < 0.001f && Mathf.Abs(v1.y - v2.y) < 0.001f)
            {
                return true;
            }

            return false;
        }
        
        public static bool AnyMatch(this Vector2 v1, float v)
        {
            if (Mathf.Abs(v1.x - v) < 0.001f || Mathf.Abs(v1.y - v) < 0.001f)
            {
                return true;
            }

            return false;
        }
    }
}