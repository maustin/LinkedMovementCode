namespace LinkedMovement
{
    public class PairBase : SerializedRawObject {
        [Serialized]
        public string pairId;

        public PairBase() {
            LinkedMovement.Log("PairBase DEFAULT CONTRUCTOR: " + pairId);
            LinkedMovement.GetController().addPairBase(this);
        }

        public PairBase(string pId) {
            pairId = pId;
        }
    }
}
