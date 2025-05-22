namespace LinkedMovement
{
    public class PairTarget : SerializedRawObject {
        [Serialized]
        public string pairId;

        [Serialized]
        public float offsetX = 0;

        [Serialized]
        public float offsetY = 0;

        [Serialized]
        public float offsetZ = 0;

        public PairTarget() {
            LinkedMovement.Log("PairTarget DEFAULT CONSTRUCTOR: " + pairId);
            LinkedMovement.GetController().addPairTarget(this);
        }

        public PairTarget(string pId, float offX = 0, float offY = 0, float offZ = 0) {
            pairId = pId;
            offsetX = offX;
            offsetY = offY;
            offsetZ = offZ;
        }
    }
}
