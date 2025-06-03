using HarmonyLib;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class BuilderUpdatePrefix {
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
            if (blueprintBuilder == LinkedMovement.LinkedMovement.GetController().selectedBlueprintBuilder) {
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
