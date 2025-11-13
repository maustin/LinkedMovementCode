using LinkedMovement.Animation;
using LinkedMovement.Utils;
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
        public List<Color> startingCustomColors = null;
        
        [Serialized]
        public Quaternion forward = Quaternion.identity;

        [Serialized]
        public string name = string.Empty;
        [Serialized]
        public string id = string.Empty;
        
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
            LMLogger.Debug("LMAnimationParams base constructor");

            // TODO: Incremental naming
            name = "New Animation";
            id = LMUtils.GetNewId();
        }

        public void setStartingValues(Transform startingTransform) {
            LMLogger.Debug("LMAnimationParams.setStartingValues for " + name);

            LMLogger.Debug("OLD starting localPosition: " + startingLocalPosition.ToString());
            LMLogger.Debug("OLD starting localRotation: " + startingLocalRotation.ToString());
            LMLogger.Debug("OLD starting scale: " + startingLocalScale.ToString());

            LMLogger.Debug("NEW starting localPosition: " + startingTransform.localPosition.ToString());
            LMLogger.Debug("NEW starting rotation: " + startingTransform.eulerAngles.ToString());
            LMLogger.Debug("NEW starting localRotation: " + startingTransform.localEulerAngles.ToString());
            LMLogger.Debug("NEW starting scale: " + startingTransform.localScale.ToString());

            startingLocalPosition = startingTransform.localPosition;
            startingLocalRotation = startingTransform.localEulerAngles;
            startingLocalScale = startingTransform.localScale;

            if (startingCustomColors == null) {
                LMLogger.Debug("INIT starting custom colors");
                var customColors = LMUtils.GetCustomColors(startingTransform.gameObject);
                if (customColors != null && customColors.Length > 0) {
                    LMLogger.Debug($"Has {customColors.Length} custom colors");
                    startingCustomColors = new List<Color>(customColors);
                } else {
                    LMLogger.Debug("Has NO custom colors");
                }
            }
        }

        public float getAnimationLength() {
            float length = 0f;
            foreach (var animationStep in animationSteps) {
                length += animationStep.duration + animationStep.startDelay + animationStep.endDelay;
            }
            return length;
        }

        public void getStepAnimationProgressOffsets(LMAnimationStep animationStep, float animationLength, ref float stepProgressMin, ref float stepProgressMax) {
            float startTime = 0f;
            foreach (var step in animationSteps) {
                if (step != animationStep) {
                    startTime += step.duration + step.startDelay + step.endDelay;
                } else {
                    stepProgressMin = startTime / animationLength;
                    stepProgressMax = (startTime + step.duration + step.startDelay + step.endDelay) / animationLength;
                    break;
                }
            }
        }

        public void addNewAnimationStep() {
            addNewAnimationStep(new LMAnimationStep(this));
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

            if (animationParams.startingCustomColors != null) {
                newAnimationParams.startingCustomColors = new List<Color>(animationParams.startingCustomColors);
            }

            newAnimationParams.forward = animationParams.forward;

            // TODO: Incremental naming
            newAnimationParams.name = animationParams.name;
            newAnimationParams.id = LMUtils.GetNewId();

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
