using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class BlueprintBuildCommandInstantiateBuilderImplementationPostfix {
    [HarmonyTargetMethod]
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(BlueprintBuildCommand), "instantiateBuilderImplementation");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("BlueprintBuildCommand.instantiateBuilderImplementation method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("BlueprintBuildCommand.instantiateBuilderImplementation method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void instantiateBuilderImplementation() {
        LinkedMovement.LinkedMovement.Log("BlueprintBuildCommand.instantiateBuilderImplementation post @ " + DateTime.Now);
    }
}
