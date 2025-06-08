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
        public float offsetX = 0;

        [Serialized]
        public float offsetY = 0;

        [Serialized]
        public float offsetZ = 0;

        public PairTarget() {}

        public PairTarget(string pId, float offX = 0, float offY = 0, float offZ = 0) {
            pairId = pId;
            offsetX = offX;
            offsetY = offY;
            offsetZ = offZ;
        }
    }
}
