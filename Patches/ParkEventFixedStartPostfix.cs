using HarmonyLib;
using LinkedMovement;
using LinkedMovement.Utils;
using System.Collections.Generic;
using System.Linq;
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

    // Post-park load, all objects should be created and we can now find pairings
    [HarmonyPostfix]
    static void eventFixedStart() {
        LinkedMovement.LinkedMovement.Log("Park.eventFixedStart Postfix");

        var sos = GameController.Instance.getSerializedObjects();
        LinkedMovement.LinkedMovement.Log("SerializedObjects count: " + sos.Count);
        // Have to reverse so animations are built in the correct order
        sos = sos.AsEnumerable().Reverse().ToList();

        var createdPairings = new List<Pairing>();

        for (int i = sos.Count - 1; i >= 0; i--) {
            var so = sos[i];
            PairBase pairBase = LMUtils.GetPairBaseFromSerializedMonoBehaviour(so);
            if (pairBase != null) {
                LinkedMovement.LinkedMovement.Log("Found pairBase");

                var pairTargets = LMUtils.FindPairTargetSOs(pairBase);
                if (pairTargets.Count > 0) {
                    var pairTargetGOs = new List<GameObject>();
                    foreach (var pairTarget in pairTargets) {
                        pairTargetGOs.Add(pairTarget.gameObject);
                    }

                    LinkedMovement.LinkedMovement.Log($"Creating Pairing with {pairTargetGOs.Count} targets");
                    var pairing = new Pairing(so.gameObject, pairTargetGOs, pairBase.pairId, pairBase.pairName);
                    pairBase.animParams.setStartingValues(so.transform);
                    pairing.connect(false);
                    createdPairings.Add(pairing);
                } else {
                    LinkedMovement.LinkedMovement.Log("No pair matches found, remove PairBase");
                    so.removeCustomData<PairBase>();
                }
            }
        }

        foreach (var pairing in createdPairings) {
            pairing.createSequence();
        }
        // TODO: Do we need to find orphaned PairTargets?
    }
}
