using UnityEngine;

namespace LinkedMovement
{
    public class PairTarget : SerializedRawObject {
        [Serialized]
        public string pairId;

        // TODO: Think this can be eliminated
        [Serialized]
        public Vector3 positionOffset;

        public PairTarget() {
            LinkedMovement.Log("PairTarget constructor");
        }

        public PairTarget(string pId, Vector3 positionOffset) {
            LinkedMovement.Log("PairTarget constructor w/ params, offset: " + positionOffset.ToString());
            pairId = pId;
            this.positionOffset = positionOffset;
        }
    }
}
