using HarmonyLib;
using LinkedMovement;
using LinkedMovement.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class ParkEventFixedStartPostfix {
    static MethodBase TargetMethod() {
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(Park), "eventFixedStart");
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("Park.eventFixedStart method found");
        }
        else {
            LinkedMovement.LinkedMovement.Log("Park.eventFixedStart method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void eventFixedStart() {
        LinkedMovement.LinkedMovement.Log("Park.eventFixedStart Postfix");

        var sos = GameController.Instance.getSerializedObjects();
        LinkedMovement.LinkedMovement.Log("SerializedObjects count: " + sos.Count);

        foreach (var so in sos) {
            PairBase pairBase = TAUtils.GetPairBaseFromSerializedMonoBehaviour(so);
            if (pairBase != null) {
                LinkedMovement.LinkedMovement.Log("Found pairBase");

                var pairTargets = FindPairTargetSOs(pairBase);
                if (pairTargets.Count > 0) {
                    var pairTargetGOs = new List<GameObject>();
                    foreach (var pairTarget in pairTargets) {
                        pairTargetGOs.Add(pairTarget.gameObject);
                    }

                    LinkedMovement.LinkedMovement.Log($"Creating Pairing with {pairTargetGOs.Count} targets");
                    var pairing = new Pairing(so.gameObject, pairTargetGOs, pairBase.pairId, pairBase.pairName);
                    pairing.connect();
                } else {
                    LinkedMovement.LinkedMovement.Log("No pair matches found, remove PairBase");
                    so.removeCustomData<PairBase>();
                }
            }
        }
    }

    static private List<SerializedMonoBehaviour> FindPairTargetSOs(PairBase pairBase) {
        var targets = new List<SerializedMonoBehaviour>();
        var sos = GameController.Instance.getSerializedObjects();
        foreach (var so in sos) {
            PairTarget pairTarget = TAUtils.GetPairTargetFromSerializedMonoBehaviour(so);
            if (pairTarget != null) {
                if (pairTarget.pairId == pairBase.pairId) {
                    LinkedMovement.LinkedMovement.Log("Same pairId!");
                    targets.Add(so);
                }
            }
        }
        return targets;
    }
}
