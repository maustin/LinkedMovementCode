using LinkedMovement.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement
{
    public class Pairing {
        public GameObject baseGO;
        // TODO: add getter/setter?
        public List<GameObject> targetGOs = new List<GameObject>();
        public string pairingId;
        public string pairingName;
        // TODO: Show original blueprint name

        private bool connected = false;

        public Pairing() {
            LinkedMovement.Log("Pairing DEFAULT CONTSTRUCTOR");
        }

        public Pairing(GameObject baseGO, List<GameObject> targetGOs, string pId = null, string name = "") {
            LinkedMovement.Log("Pairing contstructor with options");
            this.baseGO = baseGO;
            this.targetGOs = new List<GameObject>(targetGOs);

            if (pId != null) {
                pairingId = pId;
            }
            else {
                pairingId = Guid.NewGuid().ToString();
            }
            LinkedMovement.Log("Pairing ID: " + pairingId);

            pairingName = name;

            LinkedMovement.GetController().addPairing(this);
        }

        public void connect() {
            LinkedMovement.Log("Pairing connect, # targets: " + targetGOs.Count);

            if (baseGO == null) {
                LinkedMovement.Log("connect MISSING BASE GO!");
                return;
            }

            var baseBO = TAUtils.GetBuildableObjectFromGameObject(baseGO);

            LinkedMovement.GetController().removeAnimatedBuildableObject(baseBO);

            PairBase pairBase = TAUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);

            var baseAnimParams = pairBase.animParams;
            LinkedMovement.Log("Has Sequence Animation: " + (baseAnimParams != null));
            if (baseAnimParams != null) {
                LinkedMovement.Log("Base target pos: " + baseAnimParams.targetPosition.ToString());
                pairBase.sequence = TAUtils.BuildAnimationSequence(baseBO.transform, baseAnimParams);
            }

            LinkedMovement.Log("connect iterate targetGOs");
            foreach (GameObject targetGO in targetGOs) {
                // If ChunkedMesh, it's a built-in object and we need to handle it
                var targetChunkedMesh = targetGO.GetComponent<ChunkedMesh>();

                if (targetChunkedMesh != null) {
                    //LinkedMovement.Log("Target is built-in deco object, enable movement");
                    targetChunkedMesh.enabled = false;
                    var targetMeshRenderer = targetGO.GetComponent<MeshRenderer>();
                    if (targetMeshRenderer != null) {
                        targetMeshRenderer.enabled = true;
                    } else {
                        LinkedMovement.Log("connect MESHRENDERER NULL!");
                    }
                }

                var targetBO = TAUtils.GetBuildableObjectFromGameObject(targetGO);
                PairTarget pairTarget = TAUtils.GetPairTargetFromSerializedMonoBehaviour(targetBO);

                LinkedMovement.GetController().removeAnimatedBuildableObject(targetBO);

                targetGO.transform.localPosition = Vector3.zero;

                var pairTargetOffset = pairTarget.positionOffset;
                var pairBaseOffset = pairBase.positionOffset;

                LinkedMovement.Log("Base pos: " + baseGO.transform.position.ToString());
                LinkedMovement.Log("PairTargetOffset: " + pairTargetOffset.ToString());
                LinkedMovement.Log("PairBaseOffset: " + pairBaseOffset.ToString());

                targetGO.transform.position = baseGO.transform.position + pairTargetOffset + pairBaseOffset;
                LinkedMovement.Log($"World x: {targetGO.transform.position.x}, y: {targetGO.transform.position.y}, z: {targetGO.transform.position.z}");
                LinkedMovement.Log($"Local x: {targetGO.transform.localPosition.x}, y: {targetGO.transform.localPosition.y}, z: {targetGO.transform.localPosition.z}");
                TAUtils.AttachTargetToBase(baseGO.transform, targetGO.transform);
            }

            connected = true;
        }

        public void update() {
            if (!connected) return;
            // TODO: Skip if sequence and not animating

            var bo = TAUtils.GetBuildableObjectFromGameObject(baseGO);
            TAUtils.UpdateMouseColliders(bo);

            foreach (GameObject targetGO in targetGOs) {
                var targetBO = TAUtils.GetBuildableObjectFromGameObject(targetGO);
                TAUtils.UpdateMouseColliders(targetBO);
            }
        }

        public void setCustomData(bool useTargetPositionOffset = false, Vector3 basePositionOffset = new Vector3(), Vector3 baseRotationOffset = new Vector3(), LMAnimationParams animationParams = null) {
            var baseBO = TAUtils.GetBuildableObjectFromGameObject(baseGO);
            baseBO.addCustomData(getPairBase(basePositionOffset, baseRotationOffset, animationParams));

            LinkedMovement.Log("setCustomData basePositionOffset: " + basePositionOffset.ToString());

            foreach (GameObject targetGO in targetGOs) {
                var offset = Vector3.zero;
                if (useTargetPositionOffset) {
                    offset = targetGO.transform.position;
                }
                
                LinkedMovement.Log("offset: " + offset.ToString());

                var targetBO = TAUtils.GetBuildableObjectFromGameObject(targetGO);
                targetBO.addCustomData(getPairTarget(offset));
            }
        }

        public PairBase getPairBase(Vector3 positionOffset, Vector3 rotationOffset, LMAnimationParams animationParams) {
            LinkedMovement.Log("Pairing getPairBase");
            return new PairBase(pairingId, pairingName, positionOffset, rotationOffset, animationParams);
        }

        // TODO: Better name? Should this be a static? Theoretically this should never be null.
        public PairBase getExistingPairBase() {
            LinkedMovement.Log("Pairing getExistingPairBase");
            if (baseGO == null) {
                LinkedMovement.Log("ERROR: getExistingPairBase has no existing baseGO");
                return null;
            }

            var baseBO = TAUtils.GetBuildableObjectFromGameObject(baseGO);
            PairBase pairBase = TAUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);

            return pairBase;
        }

        public PairTarget getPairTarget(Vector3 offset) {
            LinkedMovement.Log("Pairing getPairTarget");
            return new PairTarget(pairingId, offset);
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

            var baseBO = TAUtils.GetBuildableObjectFromGameObject(baseGO);
            PairBase pairBase = TAUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);

            pairBase.pairName = newPairingName;
        }

        public void updatePairingBaseOffset(Vector3 newPositionOffset) {
            LinkedMovement.Log("Pairing.updatePairingBaseOffset");
            var baseBO = TAUtils.GetBuildableObjectFromGameObject(baseGO);

            var baseAnimator = baseBO.GetComponent<Animator>();
            baseAnimator.Rebind();
            baseAnimator.Update(0f);

            var pairBase = TAUtils.GetPairBaseFromSerializedMonoBehaviour(baseBO);
            pairBase.setPositionOffset(newPositionOffset);

            foreach (var targetGO in targetGOs) {
                var targetBO = TAUtils.GetBuildableObjectFromGameObject(targetGO);
                if (targetBO == null) continue;

                LinkedMovement.Log("Pairing.updatePairingBaseOffset update target position");

                targetBO.transform.position = baseGO.transform.position + newPositionOffset;
            }
        }

        public void destroy() {
            LinkedMovement.Log("Pairing.Destroy Pairing: " + getPairingName());

            // Guess the easiest is to just delete the base and let the various handlers do the rest
            if (baseGO != null) {
                GameObject.Destroy(baseGO);
            }
        }
    }
}
