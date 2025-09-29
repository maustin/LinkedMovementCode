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

    // TODO: This necessary?
    [HarmonyPostfix]
    static void Initialize() {
        LinkedMovement.LinkedMovement.Log("Park.Initialize Postfix");
        LinkedMovement.LinkedMovement.GetController();
    }
}
