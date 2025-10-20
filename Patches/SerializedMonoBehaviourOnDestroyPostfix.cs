using HarmonyLib;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class SerializedMonoBehaviourOnDestroyPostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(SerializedMonoBehaviour), "OnDestroy");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method found");
        } else {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void OnDestroy(SerializedMonoBehaviour __instance) {
        var bo = __instance as BuildableObject;
        if (bo == null) return;
        if (bo.isPreview) return;

        // OLD
        LinkedMovement.LinkedMovement.GetController().handleBuildableObjectDestroy(bo);
        // NEW
        LinkedMovement.LinkedMovement.GetLMController().handleBuildableObjectDestroyed(bo);
    }
}
