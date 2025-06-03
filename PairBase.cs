namespace LinkedMovement
{
    public class PairBase : SerializedRawObject {
        public static void Destroy(BuildableObject bo, PairBase pairBase) {
            LinkedMovement.Log("PairBase.Destroy");
            var pairing = LinkedMovement.GetController().findPairingByID(pairBase.pairId);
            if (pairing == null) {
                // Pairing has already been removed
                LinkedMovement.Log("PairBase.Destroy already destroyed pairing " + bo.name);
            } else {
                pairing.removePairTargets();
            }

            bo.removeCustomData<PairBase>();
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

        public PairBase() {}

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
    }
}
