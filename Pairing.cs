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

        private bool connected = false;
                
        public void Destroy(Pairing pairing) {
            // This pairing has been cleared
            LinkedMovement.Log("Pairing.Destroy Pairing: " + pairing.getPairingName());
            var didRemove = LinkedMovement.GetController().removePairing(pairing);
            if (!didRemove) {
                LinkedMovement.Log("ERROR! Failed to find Pairing");
            }
        }

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
                LinkedMovement.Log("MISSING BASE GO!");
                return;
            }

            var baseBO = baseGO.GetComponent<BuildableObject>();
            if (baseBO == null) {
                LinkedMovement.Log("MISSING BASE BO!");
                return;
            }

            LinkedMovement.GetController().removeAnimatedBuildableObject(baseBO);

            LinkedMovement.Log("iterate targetGOs");
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
                        LinkedMovement.Log("MESHRENDERER NULL!");
                    }
                }

                PairBase pairBase;
                baseBO.tryGetCustomData(out pairBase);
                if (pairBase == null) {
                    LinkedMovement.Log("MISSING PAIRBASE!");
                    return;
                }

                var targetBO = targetGO.GetComponent<BuildableObject>();
                if (targetBO == null) {
                    LinkedMovement.Log("MISSING TARGET BO!");
                    return;
                }
                PairTarget pairTarget;
                targetBO.tryGetCustomData(out pairTarget);
                if (pairTarget == null) {
                    LinkedMovement.Log("MISSING PAIRTARGET!");
                    return;
                }

                LinkedMovement.GetController().removeAnimatedBuildableObject(targetBO);

                targetGO.transform.localPosition = Vector3.zero;

                var pairTargetOffset = new Vector3(pairTarget.offsetX, pairTarget.offsetY, pairTarget.offsetZ);
                var pairBaseOffset = new Vector3(pairBase.posOffsetX, pairBase.posOffsetY, pairBase.posOffsetZ);

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

            TAUtils.UpdateMouseColliders(baseGO.GetComponent<BuildableObject>());

            foreach (GameObject targetGO in targetGOs) {
                var targetBO = targetGO.GetComponent<BuildableObject>();
                if (targetBO == null) {
                    continue;
                }
                TAUtils.UpdateMouseColliders(targetBO);
            }
        }

        public void setCustomData(bool useTargetPositionOffset = false, Vector3 basePositionOffset = new Vector3(), Vector3 baseRotationOffset = new Vector3()) {
            //LinkedMovement.Log("Pairing.setCustomData useOffset: " + useTargetPositionOffset);
            var baseBO = baseGO.GetComponent<BuildableObject>();
            baseBO.addCustomData(getPairBase(basePositionOffset.x, basePositionOffset.y, basePositionOffset.z, baseRotationOffset.x, baseRotationOffset.y, baseRotationOffset.z));

            LinkedMovement.Log("setCustomData basePositionOffset: " + basePositionOffset.ToString());

            foreach (GameObject targetGO in targetGOs) {
                var offset = Vector3.zero;
                if (useTargetPositionOffset) {
                    offset = targetGO.transform.position;
                }
                
                LinkedMovement.Log("offset: " + offset.ToString());

                var targetBO = targetGO.GetComponent<BuildableObject>();
                targetBO.addCustomData(getPairTarget(offset.x, offset.y, offset.z));
            }
        }

        public PairBase getPairBase(float posOffsetX, float posOffsetY, float posOffsetZ, float rotOffsetX, float rotOffsetY, float rotOffsetZ) {
            return new PairBase(pairingId, pairingName, posOffsetX, posOffsetY, posOffsetZ, rotOffsetX, rotOffsetY, rotOffsetZ);
        }

        public PairTarget getPairTarget(float offsetX, float offsetY, float offsetZ) {
            return new PairTarget(pairingId, offsetX, offsetY, offsetZ);
        }

        public void removePairTarget(GameObject targetGO) {
            var didRemove = targetGOs.Remove(targetGO);
            if (!didRemove) {
                LinkedMovement.Log("ERROR! Pairing.removePairTarget failed to find target!!");
            }

            if (targetGOs.Count == 0) {
                LinkedMovement.Log("Pairing has no more children, destroy");
                Destroy(this);
            }
        }

        public void removePairTargets() {
            var cloneTargetGOs = new List<GameObject>(targetGOs);
            foreach (var targetGO in cloneTargetGOs) {
                removePairTarget(targetGO);
            }
        }

        public string getPairingName() {
            return pairingName != "" ? pairingName : pairingId;
        }
    }
}
