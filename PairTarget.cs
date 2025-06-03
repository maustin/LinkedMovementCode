namespace LinkedMovement
{
    public class PairTarget : SerializedRawObject {
        // TODO: Move to utils?
        public static void Destroy(BuildableObject bo, PairTarget pairTarget) {
            PairBase pairBase;
            bo.tryGetCustomData(out pairBase);

            if (pairBase != null) {
                PairBase.Destroy(bo, pairBase);
            }

            // destroy target
            var pairing = LinkedMovement.GetController().findPairingByID(pairTarget.pairId);
            if (pairing != null) {
                pairing.removePairTarget(bo.gameObject);
            } else {
                // Shouldn't happen!
                LinkedMovement.Log("ERROR! PairTarget.Destroy failed to find Pairing!! " + bo.name);
            }

            bo.removeCustomData<PairTarget>();
        }

        [Serialized]
        public string pairId;

        [Serialized]
        public float offsetX = 0;

        [Serialized]
        public float offsetY = 0;

        [Serialized]
        public float offsetZ = 0;

        public PairTarget() {
            //LinkedMovement.Log("PairTarget DEFAULT CONSTRUCTOR: " + pairId);
            //LinkedMovement.GetController().addPairTarget(this);
        }

        public PairTarget(string pId, float offX = 0, float offY = 0, float offZ = 0) {
            pairId = pId;
            offsetX = offX;
            offsetY = offY;
            offsetZ = offZ;
        }
    }
}
