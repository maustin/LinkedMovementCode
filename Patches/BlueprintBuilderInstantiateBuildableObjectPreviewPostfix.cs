using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class BlueprintBuilderInstantiateBuildableObjectPreviewPostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(BlueprintBuilder), "instantiateBuildableObjectPreview");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilder.instantiateBuildableObjectPreview method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilder.instantiateBuildableObjectPreview method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void instantiateBuildableObjectPreview(BlueprintBuilder __instance, GameObject ___ghost) {
        LinkedMovement.LinkedMovement.Log("BlueprintBuilder.instantiateBuildableObjectPreview post @ " + DateTime.Now);
        
        if (___ghost != null) {
            LinkedMovement.LinkedMovement.Log("GOT GHOST!");
            ___ghost.transform.position = LinkedMovement.LinkedMovement.GetController().baseObject.transform.position + LinkedMovement.LinkedMovement.GetController().basePositionOffset;
            ___ghost.transform.SetParent(LinkedMovement.LinkedMovement.GetController().baseObject.transform);
            LinkedMovement.LinkedMovement.Log(___ghost.transform.position.ToString());
        }
        else {
            LinkedMovement.LinkedMovement.Log("MISSING GHOST");
        }
    }
}
