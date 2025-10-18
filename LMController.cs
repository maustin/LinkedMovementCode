using LinkedMovement.Animation;
using LinkedMovement.Links;
using LinkedMovement.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LMController : MonoBehaviour {
        //public enum EditMode {
        //    NONE,
        //    ANIMATION,
        //    LINK,
        //}

        // TODO: 10-16
        // Implement Links
        // - Need to implement actual parenting
        // - Rebuild animations on parenting changed
        // - Support save/load cycle
        // - Support blueprint creation
        // - Affect associated

        public LMAnimation currentAnimation { get; private set; }
        public LMLink currentLink { get; private set; }
        
        //private EditMode editMode;

        private List<LMAnimation> animations = new List<LMAnimation>();
        private List<LMLink> links = new List<LMLink>();

        private HashSet<BuildableObject> buildableObjectsToUpdate = new HashSet<BuildableObject>();

        private void Awake() {
            LinkedMovement.Log("LMController Awake");
        }

        private void OnDisable() {
            LinkedMovement.Log("LMController OnDisable");
        }

        private void OnDestroy() {
            LinkedMovement.Log("LMController OnDestroy");
            
            animations.Clear();
            links.Clear();
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
                buildableObjectsToUpdate.Add(animation.targetBuildableObject);
            }
            foreach (var bo in buildableObjectsToUpdate) {
                LMUtils.UpdateMouseColliders(bo);
            }
        }

        public void onParkStarted() {
            LinkedMovement.Log("LMController.onParkStarted");
            // TODO: Links

            foreach (var animation in animations) {
                animation.buildSequence();
            }
        }

        public void clearEditMode() {
            LinkedMovement.Log("LMController.clearEditMode");
            if (currentAnimation != null) {
                currentAnimation.discardChanges();
                currentAnimation = null;
            }
            if (currentLink != null) {
                currentLink.discardChanges();
                currentLink = null;
            }

            //editMode = EditMode.NONE;
        }

        public LMAnimation findAnimationByGameObject(GameObject gameObject) {
            LinkedMovement.Log("LMController.findAnimationByGameObject");
            foreach (var animation in animations) {
                if (animation.targetGameObject == gameObject) {
                    return animation;
                }
            }
            LinkedMovement.Log("No animation found");
            return null;
        }

        public LMAnimation addAnimation(LMAnimationParams animationParams, GameObject target) {
            LinkedMovement.Log("LMController.addAnimation from LMAnimationParams");
            
            var animation = new LMAnimation(animationParams, target);
            addAnimation(animation);
            return animation;
        }

        public void addAnimation(LMAnimation animation) {
            LinkedMovement.Log("LMController.addAnimation from LMAnimation");

            if (animations.Contains(animation)) {
                LinkedMovement.Log("Animation already in controller list");
                return;
            }

            animations.Add(animation);
        }

        // TODO: Add link

        public void editAnimation(LMAnimation animation = null) {
            LinkedMovement.Log("LMController.editAnimation");
            clearEditMode();

            if (animation != null) {
                currentAnimation = animation;
            } else {
                // TODO: Set new animation name
                var animationParams = new LMAnimationParams();
                currentAnimation = new LMAnimation(animationParams);
            }

            currentAnimation.IsEditing = true;

            //editMode = EditMode.ANIMATION;
        }

        public void editLink(LMLink link = null) {
            LinkedMovement.Log("LMController.editLink");
            clearEditMode();

            if (link != null) {
                LinkedMovement.Log("Edit existing link");
                currentLink = link;
            } else {
                LinkedMovement.Log("Create new link");
                // TODO: set new link name
                currentLink = new LMLink();
            }

            currentLink.IsEditing = true;

            //editMode = EditMode.LINK;
        }

        public void commitEdit() {
            LinkedMovement.Log("LMController.commitEdit");
            
            if (currentAnimation != null) {
                currentAnimation.saveChanges();
                currentAnimation = null;
            }
            if (currentLink != null) {
                currentLink.saveChanges();
                currentLink = null;
            }

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

    }
}
