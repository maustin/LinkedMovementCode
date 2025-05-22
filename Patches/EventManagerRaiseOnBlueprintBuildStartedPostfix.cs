using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class EventManagerRaiseOnBlueprintBuildStartedPostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(EventManager), "RaiseOnBlueprintBuildStarted");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuildStarted method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuildStarted method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void RaiseOnBlueprintBuildStarted() {
        LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuildStarted post @ " + DateTime.Now);
    }
}
