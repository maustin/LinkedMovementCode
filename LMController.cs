using LinkedMovement.Animation;
using LinkedMovement.Links;
using LinkedMovement.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LMController : MonoBehaviour {

        // TODO: 10-23
        //
        // Test layering
        // Delays
        // UI update
        // Check # Tweens being created matches expected

        // Triggers
        // - Changing triggered animation not updating the existing trigger in EffectsController
        //
        // non fatal exception
        // Appears to happen when selecting a triggered animation in the Effects Controller
        // This happens with OLD system as well. Possibly ignore.
        // NullReferenceException: Object reference not set to an instance of an object
        //   at AnimationTriggerEffectEditorPanel.initialize()[0x00011] in <eefe76887ca042e485a07fadc6c705a6>:0 
        
        // Once saw issue with animations, when at end of sequence, having 1 frame of child object mispositioned
        // Haven't seen since first occurrence.

        public LMAnimation currentAnimation { get; private set; }
        public LMLink currentLink { get; private set; }
        
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
            foreach (var link in links) {
                link.addObjectsToUpdateMouseColliders(buildableObjectsToUpdate);
            }
            foreach (var animation in animations) {
                buildableObjectsToUpdate.Add(animation.targetBuildableObject);
            }
            foreach (var bo in buildableObjectsToUpdate) {
                LMUtils.UpdateMouseColliders(bo);
            }
        }

        // Called via ParkEventStartPostFix
        public void setupPark(List<SerializedMonoBehaviour> serializedMonoBehaviours) {
            LinkedMovement.Log($"LMController.setPark with {serializedMonoBehaviours.Count} objects");
            
            var createdLinkParents = new List<LMLinkParent>();
            var createdLinkTargets = new List<LMLinkTarget>();

            // TODO: Does this need to be reversed?
            for (int i = serializedMonoBehaviours.Count - 1; i >= 0; i--) {
                var smb = serializedMonoBehaviours[i];

                LMLinkParent linkParent = LMUtils.GetLinkParentFromSerializedMonoBehaviour(smb);
                if (linkParent != null) {
                    LinkedMovement.Log($"Found LinkParent name: {linkParent.name}, id: {linkParent.id}");
                    linkParent.setTarget(smb.gameObject);
                    createdLinkParents.Add(linkParent);
                    LMUtils.DeleteChunkedMesh(smb as BuildableObject);
                }

                LMLinkTarget linkTarget = LMUtils.GetLinkTargetFromSerializedMonoBehaviour(smb);
                if (linkTarget != null) {
                    LinkedMovement.Log($"Found LinkTarget id: {linkTarget.id}");
                    linkTarget.setTarget(smb.gameObject);
                    createdLinkTargets.Add(linkTarget);
                    LMUtils.DeleteChunkedMesh(smb as BuildableObject);
                }

                LMAnimationParams animationParams = LMUtils.GetAnimationParamsFromSerializedMonoBehaviour(smb);
                if (animationParams != null) {
                    LinkedMovement.Log($"Found AnimationParams name: {animationParams.name}, id: {animationParams.id}");
                    addAnimation(animationParams, smb.gameObject);
                    LMUtils.DeleteChunkedMesh(smb as BuildableObject);
                }
            }

            // TODO: Do we need to find orphaned PairTargets?

            setupLinks(createdLinkParents, createdLinkTargets);
            onParkStarted();
        }

        public void setupLinks(List<LMLinkParent> linkParents, List<LMLinkTarget> linkTargets) {
            LinkedMovement.Log($"LMController.setupLinks from {linkParents.Count} LinkParents and {linkTargets.Count} LinkTargets");

            foreach (var linkParent in linkParents) {
                LinkedMovement.Log($"Find targets for {linkParent.name}, id {linkParent.id}");
                var matchingTargets = getLinkTargetsById(linkParent.id, linkTargets);
                LinkedMovement.Log($"Found {matchingTargets.Count} matching targets");
                if (matchingTargets.Count > 0) {
                    var link = new LMLink(linkParent, matchingTargets);
                    LinkedMovement.Log("Adding and building Link " + link.name);
                    links.Add(link);
                    link.rebuildLink();
                }
            }
        }

        public void setupLinks(List<LMLink> newLinks) {
            LinkedMovement.Log("LMController.setupLinks from Links list, count: " + newLinks.Count);

            foreach (var link in newLinks) {
                LinkedMovement.Log($"Adding and building Link name: {link.name}, id: {link.id}");
                links.Add(link);
                link.rebuildLink();
            }
        }

        public void setupAnimations(List<LMAnimation> newAnimations) {
            LinkedMovement.Log("LMController.setupAnimations from Animations list, count: " + newAnimations.Count);

            foreach (var anim in newAnimations) {
                LinkedMovement.Log($"Adding and building Animation name: {anim.name}, id: {anim.id}");
                animations.Add(anim);
                anim.setup();
            }
        }

        public void onParkStarted() {
            LinkedMovement.Log($"LMController.onParkStarted with {animations.Count} animations");

            foreach (var animation in animations) {
                animation.setup();
            }
        }

        public void handleBuildableObjectDestroyed(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handleBuildableObjectDestroyed");
            if (buildableObject == null || buildableObject.gameObject == null) {
                LinkedMovement.Log("Missing object (BO or GO)");
                return;
            }

            var gameObject = buildableObject.gameObject;
            LinkedMovement.Log("Deleted object name: " + gameObject.name);

            var animation = findAnimationByGameObject(gameObject);
            var linkAsParent = findLinkByParentGameObject(gameObject);
            var linkAsTarget = findLinkByTargetGameObject(gameObject);

            // If LinkParent, LinkTarget, or Animation target
            // - stop associated (this will put all associated at starting values)
            if (animation != null || linkAsParent != null || linkAsTarget != null) {
                LinkedMovement.Log("Deleted object is associated with an Animation or Links");
                LMUtils.EditAssociatedAnimations(new List<GameObject>() { gameObject }, LMUtils.AssociatedAnimationEditMode.Stop, false);
            } else {
                LinkedMovement.Log("Deleted object not associated with any Animation or Links, exit");
                return;
            }

            // Create restart list
            var restartList = new HashSet<GameObject>();

            // If Animation
            // - remove from animations
            // - delete Animation (remove data)
            if (animation != null) {
                LinkedMovement.Log("deleted object has animation " + animation.name);
                animations.Remove(animation);
                animation.removeAnimation();
            }

            // If LinkParent
            // - remove from links
            // - add children to restart list
            // - delete link (unparent children, remove data)
            if (linkAsParent != null) {
                LinkedMovement.Log($"deleted object is parent for Link name: {linkAsParent.name}, id: {linkAsParent.id}");
                links.Remove(linkAsParent);
                var targetGameObjects = linkAsParent.getTargetGameObjects();
                foreach (var targetGameObject in targetGameObjects) {
                    restartList.Add(targetGameObject);
                }
                linkAsParent.removeLink();
            }

            // If LinkTarget
            // - add LinkParent to restart list
            // - unparent from LinkParent
            // - if LinkParent has no children
            // -- remove from links
            // - delete data
            if (linkAsTarget != null) {
                LinkedMovement.Log($"deleted object is target for Link name: {linkAsTarget.name}, id: {linkAsTarget.id}");
                restartList.Add(linkAsTarget.getParentGameObject());
                linkAsTarget.deleteTargetObject(gameObject);

                if (!linkAsTarget.isValid()) {
                    LinkedMovement.Log("Link no longer valid, remove");
                    links.Remove(linkAsTarget);
                    linkAsTarget.removeLink();
                } else {
                    // Link is still valid, restart parent
                    restartList.Add(linkAsTarget.getParentGameObject());
                }
            }

            // Restart list
            if (restartList.Count > 0) {
                LinkedMovement.Log($"Try to restart animations on {restartList.Count} objects");
                var restartListGameObjects = new List<GameObject>();
                foreach (var hashObject in restartList) {
                    restartListGameObjects.Add(hashObject);
                }
                LMUtils.EditAssociatedAnimations(restartListGameObjects, LMUtils.AssociatedAnimationEditMode.Start, false);
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
            
            var animation = new LMAnimation(animationParams, target, true);
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

        public LMLink findLinkByParentGameObject(GameObject gameObject) {
            LinkedMovement.Log("LMController.findLinkByParentGameObject");
            foreach (var link in links) {
                if (link.getParentGameObject() == gameObject) {
                    LinkedMovement.Log("Found Link from parent");
                    return link;
                }
            }
            LinkedMovement.Log("No Link found from parent");
            return null;
        }

        public LMLink findLinkByTargetGameObject(GameObject gameObject) {
            LinkedMovement.Log("LMController.findLinkByTargetGameObject");
            foreach (var link in links) {
                var targets = link.getTargetGameObjects();
                if (targets.Contains(gameObject)) {
                    LinkedMovement.Log("Found Link from target");
                    return link;
                }
            }
            LinkedMovement.Log("No Link found from target");
            return null;
        }

        public void addLink(LMLink link) {
            LinkedMovement.Log("LMController.addLink from LMLink");

            if (links.Contains(link)) {
                LinkedMovement.Log("Link already in controller list");
                return;
            }

            links.Add(link);
        }

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
        }

        public void commitEdit() {
            LinkedMovement.Log("LMController.commitEdit");
            
            if (currentAnimation != null) {
                LMUtils.EditAssociatedAnimations(new List<GameObject>() { currentAnimation.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Restart, true);
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
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { currentAnimation.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Restart, true);

            currentAnimation.buildSequence();
        }

        private List<LMLinkTarget> getLinkTargetsById(string id, List<LMLinkTarget> targets) {
            LinkedMovement.Log("LMController.getLinkTargetsById: " + id);
            var matchingTargets = new List<LMLinkTarget>();

            foreach (var target in targets) {
                if (target.id == id) matchingTargets.Add(target);
            }

            LinkedMovement.Log($"Got {matchingTargets.Count} targets");
            return matchingTargets;
        }

    }
}
