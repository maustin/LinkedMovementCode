using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class BlueprintBuilderOnEnablePostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(BlueprintBuilder), "OnEnable");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilder.OnEnable method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilder.OnEnable method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void OnEnable(BlueprintBuilder __instance, GameObject ___ghost) {
        LinkedMovement.LinkedMovement.Log("BlueprintBuilder.OnEnable post @ " + DateTime.Now);
    }
}
