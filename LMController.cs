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
            selectionHandler.OnAddBuildableObject += handleAddObject;
            selectionHandler.OnRemoveBuildableObject += handleRemoveObject;
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
            if (currentAnimation != null) {
                currentAnimation.discardChanges();
                currentAnimation = null;
            }
            // TODO: currentLink

            editMode = EditMode.NONE;
        }

        public void editAnimation(LMAnimation animation = null) {
            clearEditMode();

            if (animation != null) {
                currentAnimation = animation;
            } else {
                currentAnimation = new LMAnimation();
            }

            editMode = EditMode.ANIMATION;
        }

        public void editLink() {
            clearEditMode();

            // TODO

            editMode = EditMode.LINK;
        }

        public void enableObjectPicker() {
            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.Individual;
            selectionHandler.enabled = true;
        }

        private void handleAddObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handleAddObject");
            // TODO
            if (editMode == EditMode.ANIMATION) {
                currentAnimation.setTarget(buildableObject);
                selectionHandler.enabled = false;
            }
        }

        private void handleRemoveObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handleRemoveObject");
            // TODO
            // currentAnimation cannot be removed this way
        }
    }
}
