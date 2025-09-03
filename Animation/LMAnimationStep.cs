using LinkedMovement.UI.Utils;
using System;
using UnityEngine;

namespace LinkedMovement.Animation {
    public class LMAnimationStep : SerializedRawObject {
        [Serialized]
        public string name = "";
        [Serialized]
        public float duration = 1f;
        [Serialized]
        public string ease = LMEase.Values.InOutQuad.ToString();
        [Serialized]
        public float startDelay = 0f;
        [Serialized]
        public float endDelay = 0f;

        [Serialized]
        public Vector3 targetPosition = Vector3.zero;
        [Serialized]
        public Vector3 targetRotation = Vector3.zero;
        [Serialized]
        public Vector3 targetScale = Vector3.zero;

        [NonSerialized]
        public bool uiIsOpen = true;

        public static LMAnimationStep Duplicate(LMAnimationStep step) {
            var newAnimationStep = new LMAnimationStep();
            newAnimationStep.name = step.name;
            newAnimationStep.duration = step.duration;
            newAnimationStep.ease = step.ease;
            newAnimationStep.startDelay = step.startDelay;
            newAnimationStep.endDelay = step.endDelay;
            newAnimationStep.targetPosition = step.targetPosition;
            newAnimationStep.targetRotation = step.targetRotation;
            newAnimationStep.targetScale = step.targetScale;
            return newAnimationStep;
        }
    }
}
