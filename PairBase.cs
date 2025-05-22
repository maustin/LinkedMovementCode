namespace LinkedMovement
{
    public class PairBase : SerializedRawObject {
        [Serialized]
        public string pairId;

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

        public PairBase() {
            //LinkedMovement.Log("PairBase DEFAULT CONTRUCTOR: " + pairId);
            //LinkedMovement.GetController().addPairBase(this);
        }

        public PairBase(string pId, float positionOffsetX = 0f, float positionOffsetY = 0f, float positionOffsetZ = 0f, float rotationOffsetX = 0f, float rotationOffsetY = 0f, float rotationOffsetZ = 0f) {
            pairId = pId;
            posOffsetX = positionOffsetX;
            posOffsetY = positionOffsetY;
            posOffsetZ = positionOffsetZ;
            rotOffsetX = rotationOffsetX;
            rotOffsetY = rotationOffsetY;
            rotOffsetZ = rotationOffsetZ;
        }
    }
}
