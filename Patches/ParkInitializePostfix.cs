using HarmonyLib;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class ParkInitializePostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(Park), "Initialize");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("Park.Initialize method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("Park.Initialize method NOT FOUND");
        }
        return methodBase;
    }

    // This ensures the controller is created at the start of park load
    // TODO: Is this needed? If the controller is always created elsewhere, can eliminate this patch.
    [HarmonyPostfix]
    static void Initialize() {
        LinkedMovement.LinkedMovement.Log("Park.Initialize Postfix");
        // Ensure Controller has been created
        LinkedMovement.LinkedMovement.GetController();
        LinkedMovement.LinkedMovement.GetLMController();
    }
}
