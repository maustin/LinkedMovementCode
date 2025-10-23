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
                        LMUtils.AddObjectHighlight(targetBuildableObject, Color.red);
                    }
                } else {
                    // clear temp
                    tempAnimationParams = null;
                    if (targetBuildableObject != null) {
                        LMUtils.RemoveObjectHighlight(targetBuildableObject);
                    }
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

        public LMAnimation(LMAnimationParams animationParams, GameObject target, bool delaySetup = false) {
            LinkedMovement.Log("LMAnimation constructor as EXISTING");
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
            LinkedMovement.Log($"LMAnimation.destroyAnimation name: {name}, id: {id}");

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

        public void setTarget(BuildableObject buildableObject, bool delaySetup) {
            LinkedMovement.Log("LMAnimation.setTarget");
            removeTarget();

            targetBuildableObject = buildableObject;
            targetGameObject = targetBuildableObject.gameObject;

            if (IsEditing) {
                LMUtils.AddObjectHighlight(targetBuildableObject, Color.red);
                LMUtils.DeleteChunkedMesh(targetBuildableObject);
            }

            if (!delaySetup) {
                setup();
            }
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

            stopSequence();
            animationParams = tempAnimationParams;

            IsEditing = false;
            isNewAnimation = false;

            LinkedMovement.GetLMController().addAnimation(this);

            buildSequence();

            setCustomData();
        }

        public void stopSequence() {
            LinkedMovement.Log("LMAnimation.stopSequence");

            if (sequence.isAlive) {
                LinkedMovement.Log("Sequence is alive, stop!");
                sequence.progress = 0;
                sequence.Stop();

                var animationParams = getAnimationParams();
                LMUtils.ResetTransformLocals(targetGameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
            }

            // TODO: Should this only happen if sequence is alive?
            //var animationParams = getAnimationParams();
            //LMUtils.ResetTransformLocals(targetGameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
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

            // Validate
            var gameObject = buildableObject.gameObject;
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                string rejectMessage = $"Selection already has Animation '{animation.name}'";
                LinkedMovement.Log(rejectMessage);
                // TODO: This call needs much cleaner access
                LinkedMovement.GetController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }

            setTarget(buildableObject, false);
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

        public void setCustomData() {
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
