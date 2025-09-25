using LinkedMovement.Animation;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
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

        // TODO: Not entirely happy with how this is set (e.g. via enter/exit Create Animation state)
        [Serialized]
        public Vector3 rotationOffset = Vector3.zero;

        [Serialized]
        public List<LMAnimationStep> animationSteps = new List<LMAnimationStep>();

        [NonSerialized]
        public long timeOfLastStepsUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public LMAnimationParams() {
            LinkedMovement.Log("LMAnimationParams base constructor");
        }

        public void setOriginalValues(Transform originalTransform) {
            LinkedMovement.Log("LMAnimationParams.setOriginalValues for " + name);

            //LinkedMovement.Log("CHECK ME setOriginalValues");
            //LinkedMovement.Log(new StackTrace().ToString());

            LinkedMovement.Log("NEW original rotation: " + originalTransform.eulerAngles.ToString());
            LinkedMovement.Log("NEW original localRotation: " + originalTransform.localEulerAngles.ToString());
            LinkedMovement.Log("NEW original scale: " + originalTransform.localScale.ToString());

            LinkedMovement.Log("OLD original rotation: " + originalRotation.ToString());
            LinkedMovement.Log("OLD original localRotation: " + originalLocalRotation.ToString());
            LinkedMovement.Log("OLD original scale: " + originalScale.ToString());
            
            originalRotation = originalTransform.eulerAngles;
            originalLocalRotation = originalTransform.localEulerAngles;
            originalScale = originalTransform.localScale;
        }

        public void setStartingValues(Transform startingTransform) {
            LinkedMovement.Log("LMAnimationParams.setStartingValues for " + name);

            //LinkedMovement.Log("CHECK ME setStartingValues");
            //LinkedMovement.Log(new StackTrace().ToString());

            LinkedMovement.Log("NEW starting position: " + startingTransform.position.ToString());
            LinkedMovement.Log("NEW starting localPosition: " + startingTransform.localPosition.ToString());
            LinkedMovement.Log("NEW starting rotation: " + startingTransform.eulerAngles.ToString());
            LinkedMovement.Log("NEW starting localRotation: " + startingTransform.localEulerAngles.ToString());
            LinkedMovement.Log("NEW starting scale: " + startingTransform.localScale.ToString());

            LinkedMovement.Log("OLD starting position: " + startingPosition.ToString());
            LinkedMovement.Log("OLD starting localPosition: " + startingLocalPosition.ToString());
            LinkedMovement.Log("OLD starting rotation: " + startingRotation.ToString());
            LinkedMovement.Log("OLD starting localRotation: " + startingLocalRotation.ToString());
            LinkedMovement.Log("OLD starting scale: " + startingLocalScale.ToString());

            startingPosition = startingTransform.position;
            startingLocalPosition = startingTransform.localPosition;
            startingRotation = startingTransform.eulerAngles;
            startingLocalRotation = startingTransform.localEulerAngles;
            startingLocalScale = startingTransform.localScale;
        }

        // Calculate how the animation position target should 
        public void calculateRotationOffset() {
            LinkedMovement.Log("LMAnimationParams.calculateRotatationOffset");
            LinkedMovement.Log($"Starting startingRot: {startingLocalRotation.ToString()}, originalRot: {originalLocalRotation.ToString()}");

            LinkedMovement.Log("OLD rotationOffset: " + rotationOffset.ToString());

            rotationOffset = startingLocalRotation - originalLocalRotation;
            LinkedMovement.Log("NEW rotationOffset: " + rotationOffset.ToString());
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
            var index = animationSteps.IndexOf(step);
            if (index == 0) {
                return false;
            }
            animationSteps.Remove(step);
            animationSteps.Insert(--index, step);
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
            newAnimationParams.rotationOffset = animationParams.rotationOffset;
            newAnimationParams.animationSteps = new List<LMAnimationStep>();
            foreach (var step in animationParams.animationSteps) {
                newAnimationParams.animationSteps.Add(LMAnimationStep.Duplicate(step));
            }
            return newAnimationParams;
        }
    }
}
