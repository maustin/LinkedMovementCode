using HarmonyLib;
using LinkedMovement;
using LinkedMovement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
[HarmonyPatch]
class BlueprintBuilderImplementationOnAfterBuildPostfix {
    static MethodBase TargetMethod() {
        Type[] p = new[] { typeof(List<BuildableObject>) };
        MethodBase methodBase = (MethodBase)AccessTools.Method(typeof(BlueprintBuilderImplementation), "onAfterBuild", p);
        if (methodBase != null) {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilderImplementation.onAfterBuild method found");
        } else {
            LinkedMovement.LinkedMovement.Log("BlueprintBuilderImplementation.onAfterBuild method NOT FOUND");
        }
        return methodBase;
    }

    [HarmonyPostfix]
    static void onAfterBuild(List<BuildableObject> builtObjectInstances) {
        LinkedMovement.LinkedMovement.Log("BlueprintBuilderImplementation.onAfterBuild post @ " + DateTime.Now);
        if (builtObjectInstances == null) {
            LinkedMovement.LinkedMovement.Log("NULL BUILT INSTANCES");
            return;
        }
        if (builtObjectInstances.Count == 0) {
            LinkedMovement.LinkedMovement.Log("Empty built instances!");
            return;
        }

        LinkedMovement.LinkedMovement.Log("# created instances: " + builtObjectInstances.Count);
        BuildableObject originObject = null;
        PairBase pairBase = null;
        List<BuildableObject> targets = new List<BuildableObject>();
        List<PairTarget> pairTargets = new List<PairTarget>();

        foreach (var bo in builtObjectInstances) {
            pairBase = LMUtils.GetPairBaseFromSerializedMonoBehaviour(bo);
            if (pairBase != null) {
                LinkedMovement.LinkedMovement.Log("Origin local rot: " + bo.transform.localEulerAngles.ToString());
                originObject = bo;
            }
            var pairTarget = LMUtils.GetPairTargetFromSerializedMonoBehaviour(bo);
            if (pairTarget != null) {
                targets.Add(bo);
                pairTargets.Add(pairTarget);
            }
        }

        if (originObject != null && targets.Count > 0) {
            LinkedMovement.LinkedMovement.Log("Create Pairing from blueprint");
            //pairBase.animParams.startingRotation = originObject.transform.localEulerAngles;
            pairBase.animParams.setStartingValues(originObject.transform, LMUtils.IsGeneratedOrigin(originObject));
            pairBase.animParams.calculateRotationOffset();
            // create new pairing ID so we don't collide with existing pairings
            var newPairingId = Guid.NewGuid().ToString();
            pairBase.pairId = newPairingId;
            foreach (var pairTarget in pairTargets) {
                pairTarget.pairId = newPairingId;
            }
            var targetGameObjects = targets.Select(t => t.gameObject).ToList();
            
            var pairing = new Pairing(originObject.gameObject, targetGameObjects, newPairingId, pairBase.pairName);
            pairing.connect();
        }
    }
}
