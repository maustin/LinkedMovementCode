using HarmonyLib;
using LinkedMovement.Utils;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class GameControllerEnableMouseToolPostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(GameController), "enableMouseTool");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("GameController.enableMouseTool method found");
        } else {
            LinkedMovement.LinkedMovement.Log("GameController.enableMouseTool method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void enableMouseTool(GameController __instance) {
        LinkedMovement.LinkedMovement.Log("GameController.enableMouseTool Postfix");
        LMUtils.UpdateGameMouseMode(__instance.getActiveMouseTool() != null);
    }
}
