//using HarmonyLib;
//using System.Reflection;
//using UnityEngine;

//#nullable disable
//[HarmonyPatch]
//class SerializedMonoBehaviourInitializePostfix {
//    static MethodBase TargetMethod() {
//        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(SerializedMonoBehaviour), "Initialize");
//        if (methodBase != null) {
//            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.Initialize method found");
//        } else {
//            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.Initialize method NOT FOUND");
//        }
//        return methodBase;
//    }

//    [HarmonyPostfix]
//    static void Initialize(SerializedMonoBehaviour __instance) {
//        var bo = __instance as BuildableObject;
//        //if (bo.isPreview) return;
//        if (bo != null) {
//            var animator = bo.GetComponent<Animator>();
//            if (animator != null) {
//                //LinkedMovement.LinkedMovement.Log("HERE!!!");
//                LinkedMovement.LinkedMovement.GetController().addAnimatedBuildableObject(bo);
//            }
//        }
//    }
//}
