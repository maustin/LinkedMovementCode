using LinkedMovement.Animation;
using LinkedMovement.UI;
using LinkedMovement.Utils;
using Parkitect.UI;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LMController : MonoBehaviour {
        public enum EditMode {
            NONE,
            ANIMATION,
            LINK,
        }

        public LMAnimation currentAnimation { get; private set; }
        // TODO: currentLink

        private EditMode editMode;

        private SelectionHandler selectionHandler;

        private List<LMAnimation> animations = new List<LMAnimation>();
        private HashSet<BuildableObject> buildableObjectsToUpdate = new HashSet<BuildableObject>();

        private void Awake() {
            LinkedMovement.Log("LMController Awake");
            selectionHandler = gameObject.AddComponent<SelectionHandler>();
            selectionHandler.enabled = false;
            selectionHandler.OnAddBuildableObject += handlePickerAddObject;
            selectionHandler.OnRemoveBuildableObject += handlePickerRemoveObject;
        }

        private void OnDisable() {
            LinkedMovement.Log("LMController OnDisable");
            selectionHandler.enabled = false;
        }

        private void OnDestroy() {
            LinkedMovement.Log("LMController OnDestroy");
            // TODO: reset controller
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler.gameObject);
                selectionHandler = null;
            }
            animations.Clear();
            LinkedMovement.ClearLMController();
        }

        private void Update() {
            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked() || GameController.Instance.isQuittingGame) {
                return;
            }

            // If there is no mouse tool active, we don't need to update mouse colliders
            var mouseTool = GameController.Instance.getActiveMouseTool();
            if (mouseTool == null) {
                return;
            }

            buildableObjectsToUpdate.Clear();
            foreach (var animation in animations) {
                //animation.update();
                buildableObjectsToUpdate.Add(animation.targetBuildableObject);
            }
            foreach (var bo in buildableObjectsToUpdate) {
                LMUtils.UpdateMouseColliders(bo);
            }
        }

        public void clearEditMode() {
            LinkedMovement.Log("LMController.clearEditMode");
            if (currentAnimation != null) {
                currentAnimation.discardChanges();
                currentAnimation = null;
            }
            // TODO: currentLink

            editMode = EditMode.NONE;
        }

        public void editAnimation(LMAnimation animation = null) {
            LinkedMovement.Log("LMController.editAnimation");
            clearEditMode();

            if (animation != null) {
                currentAnimation = animation;
            } else {
                // TODO: Set new animation name
                var animationParams = new LMAnimationParams();
                currentAnimation = new LMAnimation(animationParams, true);
            }

            currentAnimation.IsEditing = true;

            editMode = EditMode.ANIMATION;
        }

        public void editLink() {
            LinkedMovement.Log("LMController.editLink");
            clearEditMode();

            // TODO

            editMode = EditMode.LINK;
        }

        public void commitEdit() {
            LinkedMovement.Log("LMController.commitEdit");
            
            if (currentAnimation != null) {
                if (!animations.Contains(currentAnimation)) {
                    animations.Add(currentAnimation);
                }

                currentAnimation.saveChanges();
                currentAnimation = null;
            }
            // TODO: currentLink

            clearEditMode();
        }

        public void currentAnimationUpdated() {
            LinkedMovement.Log("LMController.currentAnimationUpdated");
            // TODO: Should this be an event handler subscribed to LMAnimation?
            // + Eliminates direct calls to controller
            // - Muddies control flow

            // Animation was updated, rebuild
            currentAnimation.buildSequence();
        }

        public void enableObjectPicker() {
            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.Individual;
            selectionHandler.enabled = true;
        }

        private void handlePickerAddObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handlePickerAddObject");
            // TODO
            if (editMode == EditMode.ANIMATION) {
                currentAnimation.setTarget(buildableObject);
                selectionHandler.enabled = false;
            }
        }

        private void handlePickerRemoveObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handlePickerRemoveObject");
            // TODO

            // Note, currentAnimation target cannot be removed this way
        }
    }
}
