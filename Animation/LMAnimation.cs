using LinkedMovement.Utils;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Animation {
    public class LMAnimation {
        
        public GameObject targetGameObject;
        public BuildableObject targetBuildableObject;

        public Sequence sequence;

        private bool _isEditing;
        public bool IsEditing {
            get => _isEditing;
            set {
                LinkedMovement.Log("LMAnimation.IsEditing SET to " + value.ToString());
                _isEditing = value;

                if (_isEditing) {
                    // create temp
                    tempAnimationParams = LMAnimationParams.Duplicate(animationParams);
                } else {
                    // clear temp
                    tempAnimationParams = null;
                }
            }
        }

        private bool isNewAnimation = false;

        private LMAnimationParams animationParams;
        // Used when editing params. Allow ability to discard changes.
        // If discard, simply delete the temp. If save, replace params data on target and swap temp to base.
        private LMAnimationParams tempAnimationParams;

        public LMAnimation() {
            LinkedMovement.Log("LMAnimation constructor");
            //animationParams = new LMAnimationParams();
        }

        // TODO: Constructor that takes an object?

        public LMAnimation(LMAnimationParams animationParams, bool isNewAnimation) {
            LinkedMovement.Log("LMAnimation constructor w/ params");
            this.animationParams = animationParams;
            this.isNewAnimation = isNewAnimation;
        }

        public LMAnimationParams getAnimationParams() {
            if (tempAnimationParams != null) return tempAnimationParams;
            return animationParams;
        }

        public bool hasTarget() {
            return targetGameObject != null;
        }

        public bool isValid() {
            var animationParams = getAnimationParams();
            return hasTarget() && animationParams.animationSteps.Count > 0;
        }

        public void setTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMAnimation.setTarget");
            removeTarget();

            targetBuildableObject = buildableObject;
            targetGameObject = targetBuildableObject.gameObject;

            getAnimationParams().setStartingValues(targetGameObject.transform);

            LMUtils.AddObjectHighlight(targetBuildableObject, Color.red);
            LMUtils.SetChunkedMeshEnalbedIfPresent(targetBuildableObject, false);
        }

        public void removeTarget() {
            LinkedMovement.Log("LMAnimation.removeTarget");
            if (targetGameObject != null) {
                LMUtils.RemoveObjectHighlight(targetBuildableObject);
                targetGameObject = null;
                targetBuildableObject = null;
            }
        }

        public void discardChanges() {
            LinkedMovement.Log("LMAnimation.discardChanges");
            
            if (targetBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(targetBuildableObject);
            }

            if (isNewAnimation) {
                // Was creating a new animation, just stop the sequence
                stopSequence();
                IsEditing = false;
                reset();
            } else {
                // Was editing animation, rebuild the sequence
                IsEditing = false;
                buildSequence();
            }
        }

        public void saveChanges() {
            LinkedMovement.Log("LMAnimation.saveChanges");

            LMUtils.RemoveObjectHighlight(targetBuildableObject);

            stopSequence();
            animationParams = tempAnimationParams;

            IsEditing = false;
            isNewAnimation = false;

            buildSequence();
        }

        // Remove the animation from the target
        public void removeAnimation() {
            LinkedMovement.Log("LMAnimation.removeAnimation");
            // TODO
        }

        public void stopSequence() {
            LinkedMovement.Log("LMAnimation.stopSequence");

            if (sequence.isAlive) {
                LinkedMovement.Log("Sequence is alive, stop!");
                sequence.progress = 0;
                sequence.Stop();
            }

            // TODO: Should this only happen is sequence is alive?
            var animationParams = getAnimationParams();
            LMUtils.ResetTransformLocals(targetGameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
        }

        public void buildSequence() {
            LinkedMovement.Log("LMAnimation.buildSequence");

            if (targetGameObject == null) {
                LinkedMovement.Log("ERROR: LMAnimation.buildSequence targetGameObject is null!");
                return;
            }

            stopSequence();

            // TODO: Restart associated

            var animationParams = getAnimationParams();
            sequence = LMUtils.BuildAnimationSequence(targetGameObject.transform, animationParams, IsEditing);
        }

        public void reset() {
            LinkedMovement.Log("LMAnimation.reset");
            // Remove references
            targetGameObject = null;
            targetBuildableObject = null;
            animationParams = null;
        }
    }
}
