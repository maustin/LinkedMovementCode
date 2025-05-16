//using HarmonyLib;
//using System.Reflection;

//#nullable disable
//[HarmonyPatch]
//class DecoInitializePostfix {
//    [HarmonyTargetMethod]
//    static MethodBase TargetMethod() {
//        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(Deco), "Initialize");
//        if (methodBase != null) {
//            LinkedMovement.LinkedMovement.Log("Deco.Initialize method found");
//        }
//        else {
//            LinkedMovement.LinkedMovement.Log("Deco.Initialize method NOT FOUND");
//        }
//        return methodBase;
//    }

//    [HarmonyPostfix]
//    static void Initialize(Deco __instance) {
//        LinkedMovement.LinkedMovement.Log("Deco.Initialize Postfix");
        
//        //__instance.Invoke()
//        __instance.StartCoroutine()
//    }

//    static void CallLater() {
//        LinkedMovement.LinkedMovement.Log("Deco.CallLater");
//    }
//}
