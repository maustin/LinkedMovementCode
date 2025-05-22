using HarmonyLib;
using LinkedMovement;
using Parkitect.UI;
using System;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class BlueprintBuildingPanelInstantiateBuilderPostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(BlueprintBuildingPanel), "instantiateBuilder");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("BlueprintBuildingPanel.instantiateBuilder method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("BlueprintBuildingPanel.instantiateBuilder method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void instantiateBuilder() {
        LinkedMovement.LinkedMovement.Log("BlueprintBuildingPanel.instantiateBuilder post @ " + DateTime.Now);
    }
}
