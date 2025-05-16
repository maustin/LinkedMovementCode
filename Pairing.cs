using System;
using UnityEngine;

namespace LinkedMovement
{
    public class Pairing : SerializedRawObject {
        [NonSerialized]
        public GameObject baseGO;
        [NonSerialized]
        public GameObject targetGO;

        [Serialized]
        public string pairingId;

        public Pairing() {
            LinkedMovement.Log("Pairing DEFAULT CONTSTRUCTOR");
        }

        public Pairing(GameObject baseGO, GameObject targetGO, string pId = null) {
            LinkedMovement.Log("Pairing contstructor with options");
            this.baseGO = baseGO;
            this.targetGO = targetGO;

            // If ChunkedMesh, it's a built-in object and we need to handle it
            var targetChunkedMesh = targetGO.GetComponent<ChunkedMesh>();

            if (targetChunkedMesh != null) {
                LinkedMovement.Log("Target is built-in deco object, enable movement");
                targetChunkedMesh.enabled = false;
                var targetMeshRenderer = targetGO.GetComponent<MeshRenderer>();
                targetMeshRenderer.enabled = true;
            }

            targetGO.transform.position = baseGO.transform.position;
            LinkedMovementController.AttachTargetToBase(baseGO.transform, targetGO.transform);

            if (pId != null) {
                pairingId = pId;
            } else {
                pairingId = Guid.NewGuid().ToString();
            }
            LinkedMovement.Log("Pairing ID: " + pairingId);

            //var tcs = targetGO.GetComponents<Component>();
            //LinkedMovement.Log("Target components #: " + tcs.Length);
            //foreach (var c in tcs) {
            //    LinkedMovement.Log("Target c name: " + c.name + ", type: " + c.GetType().Name);
            //}
        }

        public PairBase getPairBase() {
            return new PairBase(pairingId);
        }

        public PairTarget getPairTarget() {
            return new PairTarget(pairingId);
        }

        //public void Update() {
        //    LinkedMovement.Log("Pairing Update");
        //    return;
        //    if (targetGO == null) return;

        //    var targetChunkedMesh = targetGO.GetComponent<ChunkedMesh>();
        //    var targetMeshRenderer = targetGO.GetComponent<MeshRenderer>();
        //    if (targetChunkedMesh != null) {
        //        LinkedMovement.Log("Update ChunkedMesh.enabled: " + targetChunkedMesh.enabled);
        //    }
        //    if (targetMeshRenderer != null) {
        //        //targetMeshRenderer.enabled = true;
        //        LinkedMovement.Log("Update MeshRenderer.enabled: " + targetMeshRenderer.enabled);
        //    }
        //}
    }
}
