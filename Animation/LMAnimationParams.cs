using LinkedMovement.Animation;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LinkedMovement {
    public class LMAnimationParams : SerializedRawObject {
        [NonSerialized]
        public Vector3 startingPosition = Vector3.zero;
        [NonSerialized]
        public Vector3 startingLocalPosition = Vector3.zero;
        [NonSerialized]
        public Vector3 startingRotation = Vector3.zero;
        [NonSerialized]
        public Vector3 startingLocalRotation = Vector3.zero;
        [NonSerialized]
        public Vector3 startingLocalScale = Vector3.one;

        [Serialized]
        public Vector3 originalRotation = Vector3.zero;
        [Serialized]
        public Vector3 originalLocalRotation = Vector3.zero;
        [Serialized]
        public Vector3 originalScale = Vector3.one;

        [Serialized]
        public string name = string.Empty;
        
        [Serialized]
        public bool isTriggerable = false;
        
        [Serialized]
        public bool useInitialStartDelay = false;
        [Serialized]
        public float initialStartDelayMin = 0f;
        [Serialized]
        public float initialStartDelayMax = 0f;

        [Serialized]
        public List<LMAnimationStep> animationSteps = new List<LMAnimationStep>();

        [NonSerialized]
        public long timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public LMAnimationParams() {
            LinkedMovement.Log("LMAnimationParams base constructor");
        }

        public void setOriginalValues(Transform originTransform) {
            LinkedMovement.Log("LMAnimationParams constructor with params");
            
            originalRotation = originTransform.eulerAngles;
            originalLocalRotation = originTransform.localEulerAngles;
            originalScale = originTransform.localScale;

            //LinkedMovement.Log("original eulerAngles: " + originalRotation.ToString());
            //LinkedMovement.Log("original localEulerAngles: " + originalLocalRotation.ToString());
        }

        // TODO: originIsGenerated doesn't seem to be carrying after save/load
        public void setStartingValues(Transform originTransform) {
            LinkedMovement.Log("LMAnimationParams setStartingValues");
            //generatedOrigin = originIsGenerated;
            startingPosition = originTransform.position;
            startingLocalPosition = originTransform.localPosition;
            startingRotation = originTransform.eulerAngles;
            startingLocalRotation = originTransform.localEulerAngles;
            startingLocalScale = originTransform.localScale;

            //LinkedMovement.Log("starting rotation: " + startingRotation.ToString());
            //LinkedMovement.Log("starting local rotation: " + startingLocalRotation.ToString());
        }

        // TODO: Do we need to ensure this only runs once?
        public void calculateRotationOffset() {
            // When object is built rotated, we need to adjust the target position
            LinkedMovement.Log("calculateRotatationOffset");

            Vector3 rotationOffset = startingLocalRotation - originalLocalRotation;
            LinkedMovement.Log("rotationOffset: " + rotationOffset.ToString());

            foreach (var step in animationSteps) {
                Vector3 rotatedPositionTarget = Quaternion.Euler(rotationOffset) * step.targetPosition;
                LinkedMovement.Log("Original targetPosition: " + step.targetPosition.ToString());
                LinkedMovement.Log("New targetPosition: " + rotatedPositionTarget.ToString());
                step.targetPosition = rotatedPositionTarget;
            }
        }

        public bool isStepFirst(LMAnimationStep step) {
            return animationSteps.IndexOf(step) == 0;
        }

        public bool isStepLast(LMAnimationStep step) {
            return animationSteps.IndexOf(step) == animationSteps.Count - 1;
        }

        public void addNewAnimationStep() {
            animationSteps.Add(new LMAnimationStep());
            timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void deleteAnimationStep(LMAnimationStep step) {
            animationSteps.Remove(step);
            timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public bool moveAnimationStepUp(LMAnimationStep step) {
            if (isStepFirst(step))
                return false;
            
            animationSteps.Remove(step);
            animationSteps.Insert(0, step);
            timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return true;
        }

        public bool moveAnimationStepDown(LMAnimationStep step) {
            var index = animationSteps.IndexOf(step);
            if (index == animationSteps.Count - 1) {
                return false;
            }
            animationSteps.Remove(step);
            animationSteps.Insert(++index, step);
            timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return true;
        }

        public override string ToString() {
            var sb = new StringBuilder("LMAnimationParams\n");
            sb.AppendLine("name: " + name);
            sb.AppendLine("startingPosition: " + startingPosition.ToString());
            sb.AppendLine("startingRotation: " + startingRotation.ToString());
            //sb.AppendLine("targetPosition: " + targetPosition.ToString());
            //sb.AppendLine("targetRotation: " + targetRotation.ToString());
            //sb.AppendLine("isTriggerable: " + isTriggerable.ToString());
            //sb.AppendLine("toDuration: " + toDuration.ToString());
            //sb.AppendLine("toEase: " + toEase);
            //sb.AppendLine("fromDelay: " + fromDelay.ToString());
            //sb.AppendLine("fromDuration: " + fromDuration.ToString());
            //sb.AppendLine("fromEase: " + fromEase);
            //sb.AppendLine("restartDelay: " + restartDelay.ToString());
            sb.AppendLine("useInitialStartDelay: " + useInitialStartDelay.ToString());
            sb.AppendLine("initialStartDelayMin: " + initialStartDelayMin.ToString());
            sb.AppendLine("initialStartDelayMax: " + initialStartDelayMax.ToString());
            return sb.ToString();
        }

        public static LMAnimationParams Duplicate(LMAnimationParams animationParams) {
            var newAnimationParams = new LMAnimationParams();
            newAnimationParams.startingPosition = animationParams.startingPosition;
            newAnimationParams.startingLocalPosition = animationParams.startingLocalPosition;
            newAnimationParams.startingRotation = animationParams.startingRotation;
            newAnimationParams.startingLocalRotation = animationParams.startingLocalRotation;
            newAnimationParams.startingLocalScale = animationParams.startingLocalScale;
            newAnimationParams.originalRotation = animationParams.originalRotation;
            newAnimationParams.originalLocalRotation = animationParams.originalLocalRotation;
            newAnimationParams.originalScale = animationParams.originalScale;
            newAnimationParams.name = animationParams.name;
            newAnimationParams.isTriggerable = animationParams.isTriggerable;
            newAnimationParams.useInitialStartDelay = animationParams.useInitialStartDelay;
            newAnimationParams.initialStartDelayMin = animationParams.initialStartDelayMin;
            newAnimationParams.initialStartDelayMax = animationParams.initialStartDelayMax;
            newAnimationParams.animationSteps = new List<LMAnimationStep>();
            foreach (var step in animationParams.animationSteps) {
                newAnimationParams.animationSteps.Add(LMAnimationStep.Duplicate(step));
            }
            return newAnimationParams;
        }
    }
}
