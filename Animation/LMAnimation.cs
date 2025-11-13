using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
using PrimeTween;
using UnityEngine;

namespace LinkedMovement.Animation {
    public class LMAnimation {
        
        public GameObject targetGameObject;
        public BuildableObject targetBuildableObject;

        public Sequence sequence;

        public string name {
            get {
                if (IsEditing) return tempAnimationParams.name;
                return animationParams.name;
            }
            set {
                if (IsEditing) {
                    tempAnimationParams.name = value;
                } else {
                    animationParams.name = value;
                }
            }
        }

        public string id {
            get {
                if (IsEditing) return tempAnimationParams.id;
                return animationParams.id;
            }
            set {
                if (IsEditing) {
                    tempAnimationParams.id = value;
                } else {
                    animationParams.id = value;
                }
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
                    if (targetBuildableObject != null) {
                        LMUtils.AddObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
                    }
                } else {
                    // clear temp
                    tempAnimationParams = null;
                    if (targetBuildableObject != null && !ForceShowHighlight) {
                        LMUtils.RemoveObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
                    }
                }
            }
        }

        private bool _forceShowHighlight;
        public bool ForceShowHighlight {
            get => _forceShowHighlight;
            set {
                _forceShowHighlight = value;

                if (_forceShowHighlight) {
                    LMUtils.AddObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
                } else {
                    LMUtils.RemoveObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
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
            LMLogger.Debug("LMAnimation constructor as NEW");
            this.animationParams = animationParams;
            this.isNewAnimation = true;
        }

        public LMAnimation(LMAnimationParams animationParams, GameObject target, bool delaySetup = false) {
            LMLogger.Debug("LMAnimation constructor as EXISTING");
            this.animationParams = animationParams;

            var buildableObject = LMUtils.GetBuildableObjectFromGameObject(target);
            LMUtils.DeleteChunkedMesh(buildableObject);

            setTarget(buildableObject, delaySetup);
        }

        public void setup() {
            getAnimationParams().setStartingValues(targetGameObject.transform);
            buildSequence();
        }

        // Remove the animation from the target. Currently assumes sequence is stopped elsewhere.
        public void destroyAnimation() {
            LMLogger.Debug($"LMAnimation.destroyAnimation name: {name}, id: {id}");

            LMUtils.RemoveObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);

            removeCustomData();

            animationParams = null;
            targetBuildableObject = null;
            targetGameObject = null;
        }

        public void generateNewId() {
            id = LMUtils.GetNewId();
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
            LMLogger.Debug("LMAnimation.stopPicking");

            clearSelectionHandler();
        }

        public void startPicking() {
            LMLogger.Debug("LMAnimation.startPicking");

            clearSelectionHandler();

            selectionHandler = LinkedMovement.GetLMController().gameObject.AddComponent<SelectionHandler>();
            selectionHandler.OnAddBuildableObject += handlePickerAddObject;
            // No OnRemove as this is not supported in this mode

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.Individual;
            selectionHandler.enabled = true;
        }

        public void setTarget(BuildableObject buildableObject, bool delaySetup) {
            LMLogger.Debug("LMAnimation.setTarget");
            removeTarget();

            targetBuildableObject = buildableObject;
            targetGameObject = targetBuildableObject.gameObject;

            if (IsEditing) {
                LMUtils.AddObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
                LMUtils.DeleteChunkedMesh(targetBuildableObject);
            }

            if (!delaySetup) {
                setup();
            }
        }

        public void removeTarget() {
            LMLogger.Debug("LMAnimation.removeTarget");
            if (targetGameObject != null) {
                stopSequence();
                LMUtils.RemoveObjectHighlight(targetBuildableObject, HighlightType.AnimationTarget);
                targetGameObject = null;
                targetBuildableObject = null;
            }
        }

        public void discardChanges() {
            LMLogger.Debug("LMAnimation.discardChanges");

            stopPicking();
            
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
            LMLogger.Debug("LMAnimation.saveChanges");

            stopSequence();
            animationParams = tempAnimationParams;

            IsEditing = false;
            isNewAnimation = false;

            LinkedMovement.GetLMController().addAnimation(this);

            buildSequence();

            setCustomData();
        }

        public void stopSequence() {
            LMLogger.Debug("LMAnimation.stopSequence");

            if (sequence.isAlive) {
                LMLogger.Debug("Sequence is alive, stop!");
                sequence.progress = 0;
                sequence.Stop();

                var animationParams = getAnimationParams();
                LMUtils.ResetTransformLocals(targetGameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
            }
        }

        public void stopSequenceImmediate() {
            sequence.emergencyStop();
        }

        public void buildSequence(bool passedIsEditing = false) {
            LMLogger.Debug("LMAnimation.buildSequence");

            if (targetGameObject == null) {
                LMLogger.Error("LMAnimation.buildSequence targetGameObject is null!");
                return;
            }

            var isEditing = passedIsEditing || IsEditing;

            stopSequence();

            var animationParams = getAnimationParams();
            if (!isEditing && animationParams.isTriggerable) {
                LMLogger.Debug("Create trigger");
                var existingTrigger = targetGameObject.GetComponent<LMTrigger>();
                if (existingTrigger == null) {
                    targetGameObject.AddComponent<LMTrigger>().animationParams = animationParams;
                } else {
                    existingTrigger.update(animationParams);
                }
                return;
            }

            sequence = LMUtils.BuildAnimationSequence(targetGameObject.transform, animationParams, isEditing);
        }

        public void reset() {
            LMLogger.Debug("LMAnimation.reset");
            // Remove references
            targetGameObject = null;
            targetBuildableObject = null;
            animationParams = null;
        }

        private void handlePickerAddObject(BuildableObject buildableObject) {
            LMLogger.Debug("LMAnimation.handlePickerAddObject");

            // Validate
            var gameObject = buildableObject.gameObject;
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                //string rejectMessage = $"Selection already has Animation '{animation.name}'";
                string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_ANIM_TARGET_EXISTS, animation.name);
                LMLogger.Debug(rejectMessage);
                // TODO: This call needs much cleaner access
                LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }

            setTarget(buildableObject, false);
            stopPicking();
        }

        private void clearSelectionHandler() {
            LMLogger.Debug("LMAnimation.clearSelectionHandler");
            if (selectionHandler != null) {
                selectionHandler.enabled = false;
                selectionHandler.OnAddBuildableObject -= handlePickerAddObject;

                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
        }

        public void setCustomData() {
            LMLogger.Debug("LMAnimation.setCustomData");
            
            removeCustomData();

            targetBuildableObject.addCustomData(animationParams);
        }

        private void removeCustomData() {
            LMLogger.Debug("LMAnimation.removeCustomData");
            
            targetBuildableObject.removeCustomData<LMAnimationParams>();
        }
    }
}
