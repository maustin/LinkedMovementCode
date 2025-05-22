using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class BuilderUpdatePrefix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(Builder), "Update");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("Builder.Update method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("Builder.Update method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPrefix]
    static bool Update(Builder __instance) {
        var blueprintBuilder = __instance as BlueprintBuilder;
        if (blueprintBuilder != null) {
            //LinkedMovement.LinkedMovement.Log("Is BlueprintBuilder");
            if (blueprintBuilder == LinkedMovement.LinkedMovement.GetController().selectedBlueprintBuilder) {
                //LinkedMovement.LinkedMovement.Log("SAME BlueprintBuilder");
                //if (!LinkedMovement.LinkedMovement.GetController().didFirstBlueprintBuilderUpdate) {
                //    LinkedMovement.LinkedMovement.Log("Do first BlueprintBuilder Update");
                //    LinkedMovement.LinkedMovement.Log("Pre: " + blueprintBuilder.transform.position.ToString());
                //    blueprintBuilder.transform.position = LinkedMovement.LinkedMovement.GetController().baseObject.transform.position;
                //    LinkedMovement.LinkedMovement.Log("Pst: " + blueprintBuilder.transform.position.ToString());
                //    LinkedMovement.LinkedMovement.GetController().didFirstBlueprintBuilderUpdate = true;
                //    return true;
                //}
                return false;
            }
            else {
                //LinkedMovement.LinkedMovement.Log("DIFFERENT BlueprintBuilder");
            }
        }
        else {
            //LinkedMovement.LinkedMovement.Log("Not BlueprintBuilder");
        }
        return true;
    }
}
