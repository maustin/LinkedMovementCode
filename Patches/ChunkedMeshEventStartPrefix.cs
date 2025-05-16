using HarmonyLib;
using LinkedMovement;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class ChunkedMeshEventStartPrefix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(ChunkedMesh), "eventStart");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPrefix]
    static bool eventStart(ChunkedMesh __instance) {
        //LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart Postfix");
        if (__instance == null) {
            //LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart instance is null");
            return true;
        }
        if (__instance.gameObject == null) {
            //LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart gameObjecct is null");
            return true;
        }
        Deco deco = __instance.gameObject.GetComponent<Deco>();
        if (deco == null) {
            //LinkedMovement.LinkedMovement.Log("ChunkedMesh.eventStart deco is null");
            return true;
        }
        PairTarget pairTarget;
        deco.tryGetCustomData(out pairTarget);
        if (pairTarget != null) {
            //LinkedMovement.LinkedMovement.Log("Object is pair target, skipping base");
            return false;
        }
        return true;
    }
}
