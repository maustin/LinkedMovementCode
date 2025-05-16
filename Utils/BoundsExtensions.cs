// ATTRIB: HideScenery
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LinkedMovement.Utils {
    public static class BoundsExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsCompletely(this Bounds bounds, Bounds testBound)
          => bounds.Contains(testBound.min) && bounds.Contains(testBound.max);

        public static bool ContainsPoint(this Bounds bounds, Vector3 point) {
            var (min, max) = (bounds.min, bounds.max);
            return
              min.x <= point.x && min.y <= point.y && min.z <= point.z
              &&
              point.x <= max.x && point.y <= max.y && point.z <= max.z
              ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsCompletelyApproximately(this Bounds bounds, Bounds testBound, float epsilon)
          => bounds.ContainsPointApproximately(testBound.min, epsilon) && bounds.ContainsPointApproximately(testBound.max, epsilon);
        public static bool ContainsPointApproximately(this Bounds bounds, Vector3 point, float epsilon) {
            var (min, max) = (bounds.min, bounds.max);
            bool ApproximatelyLessThan(float a, float b) => (a - epsilon) <= b;
            bool ApproximatelyGreaterThan(float a, float b) => (a + epsilon) >= b;
            bool ApproximatelyBetween(float min, float max, float value) => ApproximatelyLessThan(min, value) && ApproximatelyGreaterThan(max, value);

            return
              ApproximatelyBetween(min.x, max.x, point.x)
              &&
              ApproximatelyBetween(min.y, max.y, point.y)
              &&
              ApproximatelyBetween(min.z, max.z, point.z)
              ;
        }
    }
}
