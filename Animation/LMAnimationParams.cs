using LinkedMovement.Animation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LMAnimationParams : SerializedRawObject {
        [Serialized]
        public Vector3 startingLocalPosition = Vector3.zero;
        [Serialized]
        public Vector3 startingLocalRotation = Vector3.zero;
        [Serialized]
        public Vector3 startingLocalScale = Vector3.one;

        [Serialized]
        public Vector3 originalLocalRotation = Vector3.zero;
        
        [Serialized]
        public Vector3 orientationOffset = Vector3.zero;

        [Serialized]
        public Vector3 forwardVec = Vector3.zero;

        [Serialized]
        public Quaternion forward = Quaternion.identity;

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

        public void setBuiltOrientation(Vector3 builtOrientation) {
            LinkedMovement.Log($"LMAnimationParams.setBuiltOrientation of {builtOrientation} for sequence: {name}");
            orientationOffset = builtOrientation - originalLocalRotation;
            //LinkedMovement.Log("originalLocalRotation: " + originalLocalRotation.ToString());
            LinkedMovement.Log("new orientationOffset: " + orientationOffset.ToString());
        }

        public void setOriginalValues(Transform originalTransform) {
            LinkedMovement.Log("LMAnimationParams.setOriginalValues for " + name);
            LinkedMovement.Log("New localRotation: " + originalTransform.localEulerAngles.ToString());

            originalLocalRotation = originalTransform.localEulerAngles;
        }

        public void setStartingValues(Transform startingTransform) {
            LinkedMovement.Log("LMAnimationParams.setStartingValues for " + name);

            LinkedMovement.Log("OLD starting localPosition: " + startingLocalPosition.ToString());
            LinkedMovement.Log("OLD starting localRotation: " + startingLocalRotation.ToString());
            LinkedMovement.Log("OLD starting scale: " + startingLocalScale.ToString());

            LinkedMovement.Log("NEW starting localPosition: " + startingTransform.localPosition.ToString());
            LinkedMovement.Log("NEW starting rotation: " + startingTransform.eulerAngles.ToString());
            LinkedMovement.Log("NEW starting localRotation: " + startingTransform.localEulerAngles.ToString());
            LinkedMovement.Log("NEW starting scale: " + startingTransform.localScale.ToString());

            startingLocalPosition = startingTransform.localPosition;
            startingLocalRotation = startingTransform.localEulerAngles;
            startingLocalScale = startingTransform.localScale;
        }

        public void addNewAnimationStep() {
            addNewAnimationStep(new LMAnimationStep());
        }

        public void addNewAnimationStep(LMAnimationStep newStep) {
            animationSteps.Add(newStep);
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

        public void addDuplicateAnimationStep(LMAnimationStep step) {
            var newStep = LMAnimationStep.Duplicate(step);
            addNewAnimationStep(newStep);
        }

        public void addInverseAnimationStep(LMAnimationStep step) {
            var newStep = LMAnimationStep.CreateInvertedStep(step);
            addNewAnimationStep(newStep);
        }

        //public override string ToString() {
        //    var sb = new StringBuilder("LMAnimationParams\n");
        //    sb.AppendLine("name: " + name);
        //    sb.AppendLine("startingPosition: " + startingPosition.ToString());
        //    sb.AppendLine("startingRotation: " + startingRotation.ToString());
        //    //sb.AppendLine("targetPosition: " + targetPosition.ToString());
        //    //sb.AppendLine("targetRotation: " + targetRotation.ToString());
        //    //sb.AppendLine("isTriggerable: " + isTriggerable.ToString());
        //    //sb.AppendLine("toDuration: " + toDuration.ToString());
        //    //sb.AppendLine("toEase: " + toEase);
        //    //sb.AppendLine("fromDelay: " + fromDelay.ToString());
        //    //sb.AppendLine("fromDuration: " + fromDuration.ToString());
        //    //sb.AppendLine("fromEase: " + fromEase);
        //    //sb.AppendLine("restartDelay: " + restartDelay.ToString());
        //    sb.AppendLine("useInitialStartDelay: " + useInitialStartDelay.ToString());
        //    sb.AppendLine("initialStartDelayMin: " + initialStartDelayMin.ToString());
        //    sb.AppendLine("initialStartDelayMax: " + initialStartDelayMax.ToString());
        //    return sb.ToString();
        //}

        public static LMAnimationParams Duplicate(LMAnimationParams animationParams) {
            var newAnimationParams = new LMAnimationParams();
            newAnimationParams.startingLocalPosition = animationParams.startingLocalPosition;
            newAnimationParams.startingLocalRotation = animationParams.startingLocalRotation;
            newAnimationParams.startingLocalScale = animationParams.startingLocalScale;

            newAnimationParams.originalLocalRotation = animationParams.originalLocalRotation;

            newAnimationParams.orientationOffset = animationParams.orientationOffset;
            newAnimationParams.forward = animationParams.forward;
            newAnimationParams.forwardVec = animationParams.forwardVec;
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
