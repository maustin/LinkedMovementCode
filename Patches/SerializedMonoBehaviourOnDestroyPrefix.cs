using HarmonyLib;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class SerializedMonoBehaviourOnDestroyPrefix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(SerializedMonoBehaviour), "OnDestroy");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method found");
        } else {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPrefix]
    static bool OnDestroy(SerializedMonoBehaviour __instance) {
        var bo = __instance as BuildableObject;
        if (bo != null && !bo.isPreview) {
            LinkedMovement.LinkedMovement.GetLMController().handleBuildableObjectDestroyed(bo);
        }
        
        return true;
    }
}
