// ATTRIB: HideScenery
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Selection {
    internal enum Visibility {
        /// <summary>
        /// Object is ignored and not collected.
        /// </summary>
        Ignore,
        /// <summary>
        /// Object is hidden by Parkitect (Park.hideSceneryEnabled/hidePathsEnabled/hideAttractionsEnabled).
        /// For Raycasting it acts like Ignore,
        /// But for canBeSelected it has a different meaning: can't be selected
        /// </summary>
        HiddenByParkitect,
        /// <summary>
        /// Object blocks view, but is not collected.
        /// </summary>
        Block,
        /// <summary>
        /// Object is visible.
        /// Closest Visible object gets collected.
        /// </summary>
        Visible,
        /// <summary>
        /// Object is hidden (->transparent).
        /// All hidden objects closer than the closest visible object get collected.
        /// </summary>
        Hidden,
    }
    internal static class HitUtility {
        public delegate Visibility CalcVisibility(BuildableObject o);
        private static int Compare(BuildableObjectBelowMouseInfo o1, BuildableObjectBelowMouseInfo o2) {
            return Comparer<float>.Default.Compare(o1.HitDistance, o2.HitDistance) switch {
                0 => -1 * Comparer<Visibility>.Default.Compare(o1.HitVisibility, o2.HitVisibility),
                var c => c,
            };
        }

        private static BuildableObjectBelowMouseInfo CreateInfo(MouseCollider.HitInfo hitInfo, BuildableObject buildableObject, Visibility visibility) {
            return new BuildableObjectBelowMouseInfo {
                HitObject = buildableObject,
                HitDistance = hitInfo.hitDistance,
                HitVisibility = visibility,
                HitLayer = hitInfo.hitObject.layer,
            };
        }

        private static readonly List<BuildableObjectBelowMouseInfo> tmpResults = new List<BuildableObjectBelowMouseInfo>();
        /// <summary>
        /// Returns object below the mouse.
        /// Unlike Utility.getObjectBelowMouse, which returns the closest canBeSelected() object,
        /// this returns the closest visible object and all hidden object between.
        ///
        /// The results list is sorted by closest to farthest object.
        /// Hidden objects are allways before visible object, even when same distance.
        /// The results contain either zero or one visible object.
        /// If there are multiple visible object with the same distance, the first visible object
        /// found by MouseCollisions.Instance.raycastAll is returned.
        /// If there's a visible object, it's the last element in the results list.
        ///
        /// This method does not handle any layers (unlike Utility.getObjectBelowMouse)!
        /// The results input list must be empty!
        /// -> when HideScenery or HidePath enabled, this method still finds the by Parkitect hidden objects.
        ///
        ///    -> exclude these objects via suitable calcVisibility method
        ///       (like `if (o is Deco && park.hideScenery) { return Ignore }`)
        /// </summary>
        public static void GetObjectsBelowMouse(CalcVisibility calcVisibility, List<BuildableObjectBelowMouseInfo> results) {
            Debug.Assert(results.Count == 0);

            try {
                GetAllObjectsBelowMouse(calcVisibility, tmpResults);
                // add hidden until first visible or block
                foreach (var hit in tmpResults) {
                    switch (hit.HitVisibility) {
                        case Visibility.Hidden:
                            results.Add(hit);
                            break;
                        case Visibility.Block:
                            return;
                        case Visibility.Visible:
                            results.Add(hit);
                            return;
                    }
                }
            }
            finally {
                tmpResults.Clear();
            }
        }

        public static void GetAllObjectsBelowMouse(CalcVisibility calcVisibility, List<BuildableObjectBelowMouseInfo> results) {
            Debug.Assert(results.Count == 0);

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (var hit in MouseCollisions.Instance.raycastAll(ray, float.MaxValue)) {
                var o = hit.hitObject.GetComponentInParent<BuildableObject>();
                if (o != null) {
                    // some objects are made of multiple objects and are therefore found multiple times
                    // what to do with these objects: use closest match or farthest match? (happens when inside other objects)
                    // -> use closest, because when hiding that's the first to hit
                    var idx = results.FindIndex(h => h.HitObject == o);
                    if (idx < 0) {
                        // new object
                        var visibility = calcVisibility(o);
                        var info = CreateInfo(hit, o, visibility);
                        results.Add(info);
                    }
                    else {
                        // already already found
                        var prevHit = results[idx];
                        if (hit.hitDistance < prevHit.HitDistance) {
                            // Visibility is same for all parts of an objects
                            var info = CreateInfo(hit, o, prevHit.HitVisibility);
                            results[idx] = info;
                        }
                    }
                }
            }

            results.Sort(comparison: Compare);
        }
    }

    internal struct BuildableObjectBelowMouseInfo {
        public BuildableObject HitObject;
        public float HitDistance;
        public Visibility HitVisibility;
        public int HitLayer;
    }
}
