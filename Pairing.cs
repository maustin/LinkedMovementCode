using LinkedMovement.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement
{
    public class Pairing {
        public GameObject baseGO;
        public List<GameObject> targetGOs = new List<GameObject>();
        public PairBase pairBase;

        // TODO: Can pairingId and pairingName pull from PairBase?
        public string pairingId;
        public string pairingName;

        private bool connected = false;

        public static PairBase CreatePairBase(string pairingId, string pairingName, Vector3 positionOffset, Vector3 rotationOffset, LMAnimationParams animationParams) {
            LinkedMovement.Log("Pairing.CreatePairBase");
            return new PairBase(pairingId, pairingName, positionOffset, rotationOffset, animationParams);
        }

        // TODO: Can we eliminate offset?
        public static PairTarget CreatePairTarget(string pairingId, Vector3 offset) {
            LinkedMovement.Log("Pairing.CreatePairTarget");
            return new PairTarget(pairingId, offset);
        }

        public Pairing() {
            LinkedMovement.Log("Pairing DEFAULT CONTSTRUCTOR");
        }

        public Pairing(GameObject baseGO, List<GameObject> targetGOs, string pId = null, string name = "") {
            LinkedMovement.Log("Pairing contstructor with options");
            
            if (pId != null) {
                pairingId = pId;
            }
            else {
                pairingId = Guid.NewGuid().ToString();
            }
            LinkedMovement.Log("Pairing ID: " + pairingId);

            setupPairing(baseGO, targetGOs, name);
        }

        public void setupPairing(GameObject baseGO, List<GameObject> targetGOs, string name = "") {
            this.baseGO = baseGO;
            this.targetGOs = new List<GameObject>(targetGOs);
            pairingName = name;

            LinkedMovement.GetController().addPairing(this);
        }

        public void updatePairing(LMAnimationParams animationParams, List<BuildableObject> newTargetObjects) {
            LinkedMovement.Log($"Pairing.updatePairing {pairingId}");
            updatePairingName(animationParams.name);
            pairBase.animParams = animationParams;

            LMUtils.RemovePairTargetFromUnusedTargets(targetGOs, newTargetObjects);

            targetGOs = new List<GameObject>();
            foreach (var bo in newTargetObjects) {
                targetGOs.Add(bo.gameObject);
            }
            setCustomData(false, default, default, animationParams);
        }

        public void connect() {
            LinkedMovement.Log("Pairing.connect, # targets: " + targetGOs.Count);

            if (baseGO == null) {
                LinkedMovement.Log("connect MISSING BASE GO!");
                return;
            }

            var baseBO = LMUtils.GetBuildableObjectFromGameObject(baseGO);

            pairBase = LMUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);

            var baseAnimParams = pairBase.animParams;
            LinkedMovement.Log("Pair has animParams: " + (baseAnimParams != null));
            if (baseAnimParams != null) {
                if (baseAnimParams.isTriggerable) {
                    baseBO.gameObject.AddComponent<LMTrigger>().animationParams = baseAnimParams;
                } else {
                    pairBase.sequence = LMUtils.BuildAnimationSequence(baseBO.transform, baseAnimParams);
                }
            } else {
                LinkedMovement.Log("NO animParams!!!");
                return;
            }

            LinkedMovement.Log("connect iterate targetGOs");
            foreach (GameObject targetGO in targetGOs) {
                // TODO: Below doesn't seem to be necessary
                // If ChunkedMesh, it's a built-in object and we need to handle it
                //var targetChunkedMesh = targetGO.GetComponent<ChunkedMesh>();
                //if (targetChunkedMesh != null) {
                //    //LinkedMovement.Log("Target is built-in deco object, enable movement");
                //    targetChunkedMesh.enabled = false;
                //    var targetMeshRenderer = targetGO.GetComponent<MeshRenderer>();
                //    if (targetMeshRenderer != null) {
                //        targetMeshRenderer.enabled = true;
                //    } else {
                //        LinkedMovement.Log("connect MESHRENDERER NULL!");
                //    }
                //}

                var targetBO = LMUtils.GetBuildableObjectFromGameObject(targetGO);

                LMUtils.AttachTargetToBase(baseGO.transform, targetGO.transform);
            }

            connected = true;
        }

        public void disconnect() {
            LinkedMovement.Log("Pairing.disconnect pairing " + pairingName);
            if (pairBase.sequence.isAlive) {
                LinkedMovement.Log("Pairing is alive, stop and reset local");
                pairBase.sequence.Stop();
                var animParams = pairBase.animParams;
                LMUtils.ResetTransformLocals(baseGO.transform, animParams.startingLocalPosition, animParams.startingLocalRotation, animParams.startingLocalScale);
            }
            connected = false;
        }

        // TODO: Probably resource intensive. See when we can skip.
        // Can we determine if the objects are off-screen and skip?
        public void frameUpdate() {
            if (!connected || !pairBase.sequence.isAlive || pairBase.sequence.isPaused) return;
            
            var bo = LMUtils.GetBuildableObjectFromGameObject(baseGO);
            LMUtils.UpdateMouseColliders(bo);

            foreach (GameObject targetGO in targetGOs) {
                var targetBO = LMUtils.GetBuildableObjectFromGameObject(targetGO);
                LMUtils.UpdateMouseColliders(targetBO);
            }
        }

        // TODO: eliminate useTargetPositionOffset
        public void setCustomData(bool useTargetPositionOffset = false, Vector3 basePositionOffset = new Vector3(), Vector3 baseRotationOffset = new Vector3(), LMAnimationParams animationParams = null) {
            LinkedMovement.Log("Pairing.setCustomData");
            var baseBO = LMUtils.GetBuildableObjectFromGameObject(baseGO);
            var pairBase = LMUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);
            if (pairBase == null) {
                baseBO.addCustomData(CreatePairBase(pairingId, pairingName, basePositionOffset, baseRotationOffset, animationParams));
            }

            LinkedMovement.Log("setCustomData basePositionOffset: " + basePositionOffset.ToString());

            foreach (GameObject targetGO in targetGOs) {
                // TODO: Remove
                var offset = Vector3.zero;
                
                var targetBO = LMUtils.GetBuildableObjectFromGameObject(targetGO);
                var pairTarget = LMUtils.GetPairTargetFromSerializedMonoBehaviour(targetBO);
                if (pairTarget == null) {
                    targetBO.addCustomData(CreatePairTarget(pairingId, offset));
                }
            }
        }

        public void removePairTarget(GameObject targetGO) {
            var didRemove = targetGOs.Remove(targetGO);
            if (!didRemove) {
                LinkedMovement.Log("ERROR! Pairing.removePairTarget failed to find target!!");
            }
        }

        public string getPairingName() {
            return pairingName != "" ? pairingName : pairingId;
        }

        public void updatePairingName(string newPairingName) {
            pairingName = newPairingName;
            pairBase.pairName = newPairingName;
        }

        public void destroy() {
            LinkedMovement.Log("Pairing.destroy Pairing: " + getPairingName());

            // Guess the easiest is to just delete the base and let the various handlers do the rest
            if (baseGO != null) {
                GameObject.Destroy(baseGO);
            }
        }
    }
}
