using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement
{
    public class Pairing {
        public GameObject baseGO;
        public List<GameObject> targetGOs = new List<GameObject>();
        public string pairingId;

        public Pairing() {
            LinkedMovement.Log("Pairing DEFAULT CONTSTRUCTOR");
        }

        public Pairing(GameObject baseGO, List<GameObject> targetGOs, string pId = null) {
            LinkedMovement.Log("Pairing contstructor with options");
            this.baseGO = baseGO;
            this.targetGOs = targetGOs;

            if (pId != null) {
                pairingId = pId;
            }
            else {
                pairingId = Guid.NewGuid().ToString();
            }
            LinkedMovement.Log("Pairing ID: " + pairingId);

            foreach (GameObject targetGO in targetGOs) {
                // If ChunkedMesh, it's a built-in object and we need to handle it
                var targetChunkedMesh = targetGO.GetComponent<ChunkedMesh>();

                if (targetChunkedMesh != null) {
                    //LinkedMovement.Log("Target is built-in deco object, enable movement");
                    targetChunkedMesh.enabled = false;
                    var targetMeshRenderer = targetGO.GetComponent<MeshRenderer>();
                    targetMeshRenderer.enabled = true;
                }

                targetGO.transform.position = baseGO.transform.position;
                LinkedMovementController.AttachTargetToBase(baseGO.transform, targetGO.transform);
            }
        }

        public PairBase getPairBase() {
            return new PairBase(pairingId);
        }

        // TODO:
        public PairTarget getPairTarget() {
            return new PairTarget(pairingId);
        }

    }
}
