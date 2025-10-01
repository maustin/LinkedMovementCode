using HarmonyLib;
using LinkedMovement.Utils;
using System.Reflection;

#nullable disable
[HarmonyPatch]
class GameControllerRemoveMouseToolPostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(GameController), "removeMouseTool");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("GameController.removeMouseTool method found");
        } else {
            LinkedMovement.LinkedMovement.Log("GameController.removeMouseTool method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void removeMouseTool(GameController __instance) {
        LinkedMovement.LinkedMovement.Log("GameController.removeMouseTool Postfix");
        LMUtils.UpdateGameMouseMode(__instance.getActiveMouseTool() != null);
    }
}
