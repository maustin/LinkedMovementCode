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

        public string name {
            get => animationParams.name;
            set {
                animationParams.name = value;
            }
        }

        public string id {
            get => animationParams.id;
            set {
                animationParams.id = value;
            }
        }

        private bool _isEditing;
        public bool IsEditing {
            get => _isEditing;
            set {
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

        private SelectionHandler selectionHandler;

        //public LMAnimation() {
        //    LinkedMovement.Log("LMAnimation constructor");
        //    //animationParams = new LMAnimationParams();
        //}

        public LMAnimation(LMAnimationParams animationParams) {
            LinkedMovement.Log("LMAnimation constructor as NEW");
            this.animationParams = animationParams;
            this.isNewAnimation = true;
        }

        public LMAnimation(LMAnimationParams animationParams, GameObject target) {
            LinkedMovement.Log("LMAnimation constructor as EXISTING");
            this.animationParams = animationParams;

            var buildableObject = LMUtils.GetBuildableObjectFromGameObject(target);
            setTarget(buildableObject);
            UnityEngine.Object.Destroy(targetBuildableObject.GetComponent<ChunkedMesh>());
        }

        public void generateNewId() {
            id = Guid.NewGuid().ToString();
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

        public void stopPicking() {
            LinkedMovement.Log("LMAnimation.stopPicking");

            clearSelectionHandler();
        }

        public void startPicking() {
            LinkedMovement.Log("LMAnimation.startPicking");

            clearSelectionHandler();

            selectionHandler = LinkedMovement.GetLMController().gameObject.AddComponent<SelectionHandler>();
            selectionHandler.OnAddBuildableObject += handlePickerAddObject;
            // No OnRemove as this is not supported in this mode

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.Individual;
            selectionHandler.enabled = true;
        }

        public void setTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMAnimation.setTarget");
            removeTarget();

            targetBuildableObject = buildableObject;
            targetGameObject = targetBuildableObject.gameObject;

            getAnimationParams().setStartingValues(targetGameObject.transform);

            if (IsEditing) {
                LMUtils.AddObjectHighlight(targetBuildableObject, Color.red);
                UnityEngine.Object.Destroy(targetBuildableObject.GetComponent<ChunkedMesh>());
            }

            buildSequence();
        }

        public void removeTarget() {
            LinkedMovement.Log("LMAnimation.removeTarget");
            if (targetGameObject != null) {
                stopSequence();
                LMUtils.RemoveObjectHighlight(targetBuildableObject);
                targetGameObject = null;
                targetBuildableObject = null;
            }
        }

        public void discardChanges() {
            LinkedMovement.Log("LMAnimation.discardChanges");

            stopPicking();
            
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

            LinkedMovement.GetLMController().addAnimation(this);

            buildSequence();

            setCustomData();
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

            // TODO: Should this only happen if sequence is alive?
            var animationParams = getAnimationParams();
            LMUtils.ResetTransformLocals(targetGameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
        }

        public void buildSequence(bool passedIsEditing = false) {
            LinkedMovement.Log("LMAnimation.buildSequence");

            if (targetGameObject == null) {
                LinkedMovement.Log("ERROR: LMAnimation.buildSequence targetGameObject is null!");
                return;
            }

            var isEditing = passedIsEditing || IsEditing;

            var animationParams = getAnimationParams();
            if (!isEditing && animationParams.isTriggerable) {
                LinkedMovement.Log("Create trigger");
                targetGameObject.AddComponent<LMTrigger>().animationParams = animationParams;
                return;
            }

            stopSequence();

            sequence = LMUtils.BuildAnimationSequence(targetGameObject.transform, animationParams, isEditing);
        }

        public void reset() {
            LinkedMovement.Log("LMAnimation.reset");
            // Remove references
            targetGameObject = null;
            targetBuildableObject = null;
            animationParams = null;
        }

        private void handlePickerAddObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMAnimation.handlePickerAddObject");

            // TODO: Validate
            // Object not already target and doesn't have LMAnimationParams

            setTarget(buildableObject);
            stopPicking();
        }

        private void clearSelectionHandler() {
            LinkedMovement.Log("LMAnimation.clearSelectionHandler");
            if (selectionHandler != null) {
                selectionHandler.enabled = false;
                selectionHandler.OnAddBuildableObject -= handlePickerAddObject;

                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
        }

        private void setCustomData() {
            LinkedMovement.Log("LMAnimation.setCustomData");
            
            removeCustomData();

            targetBuildableObject.addCustomData(animationParams);
        }

        private void removeCustomData() {
            LinkedMovement.Log("LMAnimation.removeCustomData");
            
            targetBuildableObject.removeCustomData<LMAnimationParams>();
        }
    }
}
