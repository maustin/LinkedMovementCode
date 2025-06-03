using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class SerializedMonoBehaviourInitializePostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(SerializedMonoBehaviour), "Initialize");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.Initialize method found");
        } else {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.Initialize method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void Initialize(SerializedMonoBehaviour __instance) {
        var bo = __instance as BuildableObject;
        if (bo != null) {
            var baseTransform = bo.transform;
            bool foundPlatform = false;

            var baseChildrenCount = baseTransform.childCount;
            for (var i = 0; i < baseChildrenCount; i++) {
                var child = baseTransform.GetChild(i);
                var childName = child.gameObject.name;
                if (childName.Contains("[Platform]")) {
                    foundPlatform = true;
                    break;
                }
            }
            if (foundPlatform) {
                LinkedMovement.LinkedMovement.Log("Found Platform, Initialize");
                LinkedMovement.LinkedMovement.GetController().addPlatformObject(bo);
            }
        }
    }
}
