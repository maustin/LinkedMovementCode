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
        public float posOffsetX;

        [Serialized]
        public float posOffsetY;

        [Serialized]
        public float posOffsetZ;

        [Serialized]
        public float rotOffsetX;

        [Serialized]
        public float rotOffsetY;

        [Serialized]
        public float rotOffsetZ;

        public PairBase() { }

        public PairBase(string pId, string pName, float positionOffsetX = 0f, float positionOffsetY = 0f, float positionOffsetZ = 0f, float rotationOffsetX = 0f, float rotationOffsetY = 0f, float rotationOffsetZ = 0f) {
            pairId = pId;
            pairName = pName;
            posOffsetX = positionOffsetX;
            posOffsetY = positionOffsetY;
            posOffsetZ = positionOffsetZ;
            rotOffsetX = rotationOffsetX;
            rotOffsetY = rotationOffsetY;
            rotOffsetZ = rotationOffsetZ;
        }

        public Vector3 getPositionOffset() {
            return new Vector3(posOffsetX, posOffsetY, posOffsetZ);
        }

        public void setPositionOffset(Vector3 positionOffset) {
            posOffsetX = positionOffset.x;
            posOffsetY = positionOffset.y;
            posOffsetZ = positionOffset.z;
        }
    }
}
