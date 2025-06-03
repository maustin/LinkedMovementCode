using HarmonyLib;
using LinkedMovement;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class SerializedMonoBehaviourOnDestroyPostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(SerializedMonoBehaviour), "OnDestroy");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method found");
        } else {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void OnDestroy(SerializedMonoBehaviour __instance) {
        var bo = __instance as BuildableObject;
        if (bo == null) return;

        PairBase pairBase;
        bo.tryGetCustomData(out pairBase);

        if (pairBase != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy destroy PairBase");
            PairBase.Destroy(bo, pairBase);
        }

        PairTarget pairTarget;
        bo.tryGetCustomData(out pairTarget);

        if (pairTarget != null) {
            LinkedMovement.LinkedMovement.Log("SerializedMonoBehaviour.OnDestroy destroy PairTarget");
            PairTarget.Destroy(bo, pairTarget);
        }
    }
}
