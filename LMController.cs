using LinkedMovement.Animation;
using LinkedMovement.Links;
using LinkedMovement.UI;
using LinkedMovement.Utils;
using Parkitect.UI;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LMController : MonoBehaviour {

        // TODO: 11-10
        //
        // Check # Tweens being created matches expected

        // Triggers
        //
        // non fatal exception
        // Appears to happen when selecting a triggered animation in the Effects Controller
        // This happens with OLD system as well. Possibly ignore.
        // NullReferenceException: Object reference not set to an instance of an object
        //   at AnimationTriggerEffectEditorPanel.initialize()[0x00011] in <eefe76887ca042e485a07fadc6c705a6>:0 

        // Once saw issue with animations, when at end of sequence, having 1 frame of child object mispositioned
        // Haven't seen since first occurrence.

        public WindowManager windowManager;

        public LMAnimation currentAnimation { get; private set; }
        public LMLink currentLink { get; private set; }
        
        private List<LMAnimation> animations = new List<LMAnimation>();
        private List<LMLink> links = new List<LMLink>();

        private HashSet<BuildableObject> buildableObjectsToUpdate = new HashSet<BuildableObject>();

        private bool mouseToolActive = false;
        private HashSet<BuildableObject> animationHelperObjects = new HashSet<BuildableObject>();

        private LMAnimation queuedAnimationToRemove;
        private LMLink queuedLinkToRemove;

        // For lack of better place for this
        private ColorPickerWindow colorPickerWindow;
        
        private void Awake() {
            LinkedMovement.Log("LMController Awake");

            windowManager = new WindowManager();
        }

        private void OnDisable() {
            LinkedMovement.Log("LMController OnDisable");
        }

        private void OnDestroy() {
            LinkedMovement.Log("LMController OnDestroy");

            if (windowManager != null) {
                windowManager.destroy();
                windowManager = null;
            }

            foreach (var animation in animations) {
                animation.stopSequenceImmediate();
            }
            animations.Clear();
            links.Clear();
            animationHelperObjects.Clear();
            LinkedMovement.ClearLMController();
        }

        private void Update() {
            if (queuedAnimationToRemove != null) {
                doRemoveAnimation(queuedAnimationToRemove);
            }
            if (queuedLinkToRemove != null) {
                doRemoveLink(queuedLinkToRemove);
            }

            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked() || GameController.Instance.isQuittingGame) {
                return;
            }

            if (InputManager.getKeyUp("LM_toggleGUI") && !windowManager.uiPresent()) {
                LinkedMovement.Log("Toggle GUI");
                windowManager.createWindow(WindowManager.WindowType.ModeDeterminationNew, null);
            }

            // If there is no mouse tool active & not in builder mode (Deco, Blueprints), we don't need to update mouse colliders
            var mouseTool = GameController.Instance.getActiveMouseTool();
            if (mouseTool == null && !GameController.Instance.hasActiveBuilderWindow()) {
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

        private void OnGUI() {
            if (OptionsMenu.instance != null) return;

            float uiScale = Settings.Instance.uiScale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1f));
            windowManager.DoGUI();
            GUI.matrix = Matrix4x4.identity;
        }

        public List<LMAnimation> getAnimations() {
            return animations;
        }

        public List<LMLink> getLinks() {
            return links;
        }

        public void setMouseToolActive(bool active) {
            mouseToolActive = active;
            updateAnimationHelperVisibility();
        }

        public void addAnimationHelper(BuildableObject buildableObject) {
            LinkedMovement.Log("Add animationHelper " + buildableObject.name);
            animationHelperObjects.Add(buildableObject);
            var renderer = buildableObject.gameObject.GetComponent<Renderer>();
            if (renderer != null) {
                renderer.enabled = shouldShowAnimationHelperObjects();
            }
        }

        public void removeAnimationHelper(BuildableObject buildableObject) {
            LinkedMovement.Log("Remove animationHelper " + buildableObject.name);
            animationHelperObjects.Remove(buildableObject);
        }

        private bool shouldShowAnimationHelperObjects() {
            return mouseToolActive || currentAnimation != null || currentLink != null;
        }

        private void updateAnimationHelperVisibility() {
            var show = shouldShowAnimationHelperObjects();

            foreach (var animationHelperObject in animationHelperObjects) {
                var renderer = animationHelperObject.gameObject.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.enabled = show;
                }
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

        public void handleBuildableObjectDestruct(BuildableObject buildableObject) {
            LinkedMovement.Log("LMController.handleBuildableObjectDestruct");
            if (buildableObject == null || buildableObject.gameObject == null) {
                LinkedMovement.Log("Missing object (BO or GO)");
                return;
            }
            if (buildableObject.isPreview) {
                LinkedMovement.Log("Object is preview, skip");
                return;
            }

            var gameObject = buildableObject.gameObject;
            LinkedMovement.Log("Destruct object: " + gameObject.name);

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
                animation.destroyAnimation();
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
                linkAsParent.destroyLink();
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
                    linkAsTarget.destroyLink();
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
            closeColorPickerWindow();

            if (currentAnimation != null) {
                currentAnimation.discardChanges();
                currentAnimation = null;
            }
            if (currentLink != null) {
                currentLink.discardChanges();
                currentLink = null;
            }

            updateAnimationHelperVisibility();
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
                LinkedMovement.Log("Edit existing Animation");
                currentAnimation = animation;
            } else {
                // TODO: Set new animation name
                LinkedMovement.Log("Edit new Animation");
                var animationParams = new LMAnimationParams();
                currentAnimation = new LMAnimation(animationParams);
            }

            currentAnimation.IsEditing = true;

            if (animation != null) {
                currentAnimation.buildSequence();
            }

            updateAnimationHelperVisibility();
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

            updateAnimationHelperVisibility();
        }

        public void queueAnimationToRemove(LMAnimation animation) {
            LinkedMovement.Log($"LMController.queueAnimationToRemove name: {animation.name}, id: {animation.id}");
            queuedAnimationToRemove = animation;
        }

        private void doRemoveAnimation(LMAnimation animation) {
            LinkedMovement.Log($"LMController.doRemoveAnimation name: {animation.name}, id: {animation.id}");

            var animationGameObject = animation.targetGameObject;
            var goList = new List<GameObject>() { animationGameObject };
            LMUtils.EditAssociatedAnimations(goList, LMUtils.AssociatedAnimationEditMode.Stop, false);

            animations.Remove(animation);
            animation.destroyAnimation();

            LMUtils.EditAssociatedAnimations(goList, LMUtils.AssociatedAnimationEditMode.Start, false);
            queuedAnimationToRemove = null;
        }

        public void queueLinkToRemove(LMLink link) {
            LinkedMovement.Log($"LMController.queueLinkToRemove name: {link.name}, id: {link.id}");
            queuedLinkToRemove = link;
        }

        private void doRemoveLink(LMLink link) {
            LinkedMovement.Log($"LMController.doRemoveList name: {link.name}, id: {link.id}");

            var linkParentGameObject = link.getParentGameObject();
            var linkTargetGameObjects = link.getTargetGameObjects();
            var parentGameObjectList = new List<GameObject>() { linkParentGameObject };
            var allGameObjectList = new List<GameObject>() { linkParentGameObject };
            allGameObjectList.AddRange(linkTargetGameObjects);

            LMUtils.EditAssociatedAnimations(parentGameObjectList, LMUtils.AssociatedAnimationEditMode.Stop, false);

            links.Remove(link);
            link.destroyLink();

            LMUtils.EditAssociatedAnimations(allGameObjectList, LMUtils.AssociatedAnimationEditMode.Start, false);

            queuedLinkToRemove = null;
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

        public bool colorPickerWindowIsOpen() {
            return colorPickerWindow != null;
        }

        public ColorPickerWindow getColorPickerWindow() {
            return colorPickerWindow;
        }

        public void closeColorPickerWindow() {
            if (colorPickerWindow != null) {
                ColorPickerWindow.closeOpenInstance();
                colorPickerWindow = null;
            }
        }

        public ColorPickerWindow launchColorPickerWindow(Color[] colors, int selectedIndex) {
            colorPickerWindow = ColorPickerWindow.launch(colors, selectedIndex);
            colorPickerWindow.onClose += () => {
                colorPickerWindow = null;
            };
            return colorPickerWindow;
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
