using UnityEngine;

namespace LinkedMovement
{
    public class PairTarget : SerializedRawObject {
        public static void Destroy(BuildableObject bo, PairTarget pairTarget) {
            LinkedMovement.Log("PairTarget.Destroy");
            if (bo == null) {
                LinkedMovement.Log("ERROR: BuildableObject is null");
                return;
            }

            bo.removeCustomData<PairTarget>();

            var pairing = LinkedMovement.GetController().findPairingByID(pairTarget.pairId);
            if (pairing == null) {
                LinkedMovement.Log("Couldn't find pairing with ID: " + pairTarget.pairId + ", likely already removing");
                return;
            }

            pairing.removePairTarget(bo.gameObject);

            LinkedMovement.GetController().tryToDeletePairing(pairing);
        }

        [Serialized]
        public string pairId;

        [Serialized]
        public Vector3 positionOffset;

        public PairTarget() {
            LinkedMovement.Log("PairTarget constructor");
        }

        public PairTarget(string pId, Vector3 positionOffset) {
            LinkedMovement.Log("PairTarget constructor w/ params");
            pairId = pId;
            this.positionOffset = positionOffset;
        }
    }
}
