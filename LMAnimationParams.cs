using System;
using UnityEngine;

namespace LinkedMovement {
    public class LMAnimationParams : SerializedRawObject {
        [Serialized]
        public Vector3 startingPosition;
        [Serialized]
        public Vector3 targetPosition = Vector3.zero;
        //public Vector3 startingRotation;
        //public Vector3 targetRotation = Vector3.zero;
        [Serialized]
        public bool isTriggerable = false;
        [Serialized]
        public float toDuration = 1f;
        [Serialized]
        public string toEase = "";
        [Serialized]
        public float fromDelay = 0f;
        [Serialized]
        public float fromDuration = 1f;
        [Serialized]
        public string fromEase = "";
        [Serialized]
        public float restartDelay = 0f;

        public LMAnimationParams() {
            LinkedMovement.Log("LMAnimationParams base constructor");
        }

        public LMAnimationParams(Vector3 startingPosition) {
            LinkedMovement.Log("LMAnimationParams constructor with startingPosition");
            this.startingPosition = startingPosition;
        }
    }
}
