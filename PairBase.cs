using PrimeTween;
using System;
using UnityEngine;

namespace LinkedMovement
{
    public class PairBase : SerializedRawObject {
        public static void Destroy(BuildableObject bo, PairBase pairBase) {
            LinkedMovement.Log("PairBase.Destroy");
            if (bo == null) {
                LinkedMovement.Log("ERROR: BuildableObject is null");
                return;
            }

            bo.removeCustomData<PairBase>();

            var pairing = LinkedMovement.GetController().findPairingByID(pairBase.pairId);
            if (pairing == null) {
                LinkedMovement.Log("Couldn't find pairing with ID: " + pairBase.pairId + ", likely already removing");
                return;
            }

            pairing.baseGO = null;

            LinkedMovement.GetController().tryToDeletePairing(pairing);
        }

        [Serialized]
        public string pairId;

        [Serialized]
        public string pairName;

        [Serialized]
        public Vector3 positionOffset;

        [Serialized]
        public Vector3 rotationOffset;

        [Serialized]
        public LMAnimationParams animParams;

        [NonSerialized]
        public Sequence sequence;

        // TODO: Show/Hide base object

        public PairBase() {
            LinkedMovement.Log("PairBase constructor");
        }

        public PairBase(string pId, string pName, Vector3 positionOffset, Vector3 rotationOffset, LMAnimationParams animParams = null) {
            LinkedMovement.Log("PairBase constructor w/ params");
            pairId = pId;
            pairName = pName;
            this.positionOffset = positionOffset;
            this.rotationOffset = rotationOffset;
            this.animParams = animParams;
        }

    }
}
