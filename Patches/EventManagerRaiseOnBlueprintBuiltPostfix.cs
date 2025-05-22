using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class EventManagerRaiseOnBlueprintBuiltPostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(EventManager), "RaiseOnBlueprintBuilt");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuilt method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuilt method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void RaiseOnBlueprintBuilt() {
        LinkedMovement.LinkedMovement.Log("EventManager.RaiseOnBlueprintBuilt post @ " + DateTime.Now);
    }
}
