// ATTRIB: HideScenery
using System.Collections.Generic;
using System.Reflection;
using LinkedMovement.Selection;
using HarmonyLib;
using UnityEngine;

namespace LinkedMovement
{
    internal sealed class Injector {
        private static Injector _injector = null;
        public static Injector Instance {
            get {
                if (_injector == null) {
                    _injector = new Injector();
                    _injector.InitializePatches();
                }
                return _injector;
            }
        }

        private Injector() { }

        private readonly List<MethodPatch> patches = new();
        private void InitializePatches() {
            if (LinkedMovement.Harmony != null) {
                return;
            }

            LinkedMovement.Log("Injector initialize patches");

            // deco
            {
                var original = typeof(Deco).GetMethod(nameof(Deco.canBeSelected), BindingFlags.Instance | BindingFlags.Public);
                var prefix = typeof(PatchFunctions).GetMethod(nameof(PatchFunctions.Deco_CanBeSelected_Prefix), BindingFlags.Static | BindingFlags.Public);
                patches.Add(MethodPatch.Create(original, prefix));
            }
            // path
            {
                var original = typeof(Path).GetMethod(nameof(Path.canBeSelected), BindingFlags.Instance | BindingFlags.Public);
                var prefix = typeof(PatchFunctions).GetMethod(nameof(PatchFunctions.Path_CanBeSelected_Prefix), BindingFlags.Static | BindingFlags.Public);
                patches.Add(MethodPatch.Create(original, prefix));
            }
        }

        public void Apply(HitUtility.CalcVisibility calcVisibility) {
            Debug.Assert(LinkedMovement.Harmony != null);

            PatchFunctions.CalcVisibility = calcVisibility;
            foreach (var patch in patches) {
                patch.Apply(LinkedMovement.Harmony);
            }
        }
        public void Remove() {
            Debug.Assert(LinkedMovement.Harmony != null);

            foreach (var patch in patches) {
                patch.Remove(LinkedMovement.Harmony);
            }
            PatchFunctions.CalcVisibility = null;
        }

        private sealed class MethodPatch {
            public readonly MethodBase Original;
            public readonly HarmonyMethod Prefix;
            private MethodInfo Patch;
            public bool IsPatched => Patch != null;

            private MethodPatch(MethodBase original, HarmonyMethod prefix) {
                Original = original;
                Prefix = prefix;
            }
            public static MethodPatch Create(MethodInfo original, MethodInfo prefix)
              => new(original, new HarmonyMethod(prefix));

            public void Apply(Harmony harmony) {
                if (IsPatched) {
                    return;
                }

                Patch = harmony.Patch(Original, Prefix);
            }
            public void Remove(Harmony harmony) {
                if (!IsPatched) {
                    return;
                }

                harmony.Unpatch(Original, Patch);
                Patch = null;
            }
        }

        private static class PatchFunctions {
            public static HitUtility.CalcVisibility CalcVisibility = null;

            public static bool Deco_CanBeSelected_Prefix(Deco __instance, ref bool __result)
              => CanBeSelected_Prefix(__instance, ref __result);
            public static bool Path_CanBeSelected_Prefix(Path __instance, ref bool __result)
              => CanBeSelected_Prefix(__instance, ref __result);

            private static bool CanBeSelected_Prefix(BuildableObject __instance, ref bool __result) {
                // return: 
                //    true: call original function
                //    false: don't call and use __result

                if (__instance.isPreview) {
                    return true;
                }

                var cv = CalcVisibility;
                if (cv == null) {
                    return true;
                }

                var visibility = cv(__instance);
                __result = visibility switch {
                    Visibility.HiddenByParkitect or Visibility.Hidden => false,
                    Visibility.Ignore or Visibility.Visible or Visibility.Block => true,
                    _ => throw new System.InvalidOperationException($"Unhandled visibility {visibility}"),
                };
                return false;
            }
        }
    }
}
