using DG.Tweening;
using LinkedMovement.UI;
using LinkedMovement.Utils;
using Parkitect.UI;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovementController : MonoBehaviour {
        // TODO: Move enum out
        public enum CreationSteps {
            Select,
            Assemble,
            Animate,
        }

        // TODO: Lots of this should prob be moved out to differentiate between
        // create new animation and edit existing animation

        private SelectionHandler selectionHandler;
        private bool selectionHandlerEnabled {
            get => selectionHandler.enabled;
            set => selectionHandler.enabled = value;
        }
        private CreationSteps creationStep = CreationSteps.Select;

        public WindowManager windowManager;

        // TODO: Getter setter useful here?
        public LMAnimationParams animationParams;
        public List<BuildableObject> targetObjects { get; private set; }
        //public Dictionary<BuildableObject, Vector3> targetOriginTransformPositions { get; private set; }

        //private Dictionary<BuildableObject, Vector3> animationOriginalPositions;
        //private Dictionary<BuildableObject, Quaternion> animationOriginalRotations;

        public BuildableObject originObject { get; private set; }
        public Vector3 originPosition {
            get {
                if (originObject == null) {
                    return Vector3.zero;
                } else {
                    return originObject.transform.position;
                }
            }
            set {
                if (originObject == null) {
                    throw new System.Exception("NO ORIGIN OBJECT TO SET POSITION ON!");
                } else {
                    originObject.transform.position = value;
                }
            }
        }
        //private Vector3 _originPositionOffset;
        //public Vector3 originPositionOffset {
        //    get {
        //        if (_originPositionOffset == null) {
        //            throw new System.Exception("NO ORIGIN POSITION OFFSET!");
        //        } else {
        //            return _originPositionOffset;
        //        }
        //    }
        //    set {
        //        setOriginOffsetPosition(value);
        //    }
        //}

        public string animatronicName = string.Empty;

        public List<BuildableObject> animatedBuildableObjects { get; private set; }
        public void addAnimatedBuildableObject(BuildableObject bo) {
            animatedBuildableObjects.Add(bo);
        }
        public void removeAnimatedBuildableObject(BuildableObject bo) {
            animatedBuildableObjects.Remove(bo);
        }

        private List<BuildableObject> queuedRemovalTargets = new List<BuildableObject>();
        private Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle> highlightHandles = new Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle>();

        private List<Pairing> pairings = new List<Pairing>();

        //public Vector3 basePositionOffset { get; private set; }
        //public void setBasePositionOffset(Vector3 value) {
        //    basePositionOffset = value;
        //    tryUpdateTargetsTransform();
        //}
        //public Vector3 baseRotationOffset { get; private set; }
        //public void setBaseRotationOffset(Vector3 value) {
        //    baseRotationOffset = value;
        //    tryUpdateTargetsTransform();
        //}

        //public string pairName = "";

        private void tryUpdateTargetsTransform() {
            // TODO
            //if (baseObject == null) {
            //    // Nothing to attach to!
            //    LinkedMovement.Log("LMController tryUpdateTargetsTransform no base!");
            //    return;
            //}

            //// Try to update blueprint if present
            ////LinkedMovement.Log("LMController tryUpdateTargetsTransform");
            ////tryToCreateBlueprintBuilder();

            //// Set transforms for single targets if present
            //foreach (var targetBO in targetObjects) {
            //    LinkedMovement.Log("LMController tryUpdateTargetsTransform for target " + targetBO.name);

            //    targetBO.transform.position = baseObject.transform.position + new Vector3(basePositionOffset.x, basePositionOffset.y, basePositionOffset.z);
            //    targetBO.transform.SetParent(baseObject.transform);
            //}
        }

        private void Awake() {
            LinkedMovement.Log("LinkedMovementController Awake");
            targetObjects = new List<BuildableObject>();
            //animationOriginalPositions = new Dictionary<BuildableObject, Vector3>();
            //animationOriginalRotations = new Dictionary<BuildableObject, Quaternion>();

            animatedBuildableObjects = new List<BuildableObject>();

            windowManager = new WindowManager();
            selectionHandler = gameObject.AddComponent<SelectionHandler>();
            selectionHandler.controller = this;
            selectionHandler.enabled = false;
        }

        private void OnDisable() {
            LinkedMovement.Log("LinkedMovementController OnDisable");
            disableSelectionHandler();
            DOTween.KillAll();
        }

        private void OnDestroy() {
            LinkedMovement.Log("LinkedMovementController OnDestroy");
            LinkedMovement.ClearController();
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
            clearAllSelections();
            //baseObject = null;
            targetObjects.Clear();
            pairings.Clear();
            if (windowManager != null) {
                windowManager.destroy();
                windowManager = null;
            }
        }

        private void Update() {
            //LinkedMovement.Log("Controller update");
            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked() || GameController.Instance.isQuittingGame) {
                return;
            }

            if (InputManager.getKeyUp("LM_toggleGUI") && !windowManager.uiPresent()) {
                LinkedMovement.Log("Toggle GUI");
                windowManager.createWindow(WindowManager.WindowType.ModeDetermination, null);
            }

            if (queuedRemovalTargets.Count > 0) {
                foreach (var target in queuedRemovalTargets) {
                    removeTargetBuildableObject(target);
                }
                queuedRemovalTargets.Clear();
            }

            var mouseTool = GameController.Instance.getActiveMouseTool();
            // TODO: Log? What is dis?
            if (mouseTool == null) return;

            foreach (var bo in animatedBuildableObjects) {
                LMUtils.UpdateMouseColliders(bo);
            }

            foreach (var pairing in pairings) {
                pairing.update();
            }
        }

        private void OnGUI() {
            if (OptionsMenu.instance != null) return;

            windowManager.DoGUI();
        }

        public CreationSteps getCreationStep() { return creationStep; }

        public void setCreationStep(CreationSteps newStep) {
            LinkedMovement.Log("setCreationStep " + newStep.ToString());
            if (newStep == creationStep) {
                LinkedMovement.Log("Already in creation step " + newStep.ToString());
                return;
            }

            selectionHandlerEnabled = false;

            // Select -> Assemble
            //if (creationStep == CreationSteps.Select && newStep == CreationSteps.Assemble) {
            //    // TODO
            //    // If origin exists and targets have changed, add new targets to origin
            //    // Don't think necessary as attachment will now happen when moving to Animate state
            //}

            //if (newStep == CreationSteps.Select) {
            //    //
            //} else if (newStep == CreationSteps.Assemble) {
            //    // TODO
            //} else if (newStep == CreationSteps.Animate) {
            //    //
            //}

            if (animatronicName == string.Empty) {
                // TODO: Get #?
                animatronicName = "New Animatronic";
            }

            // TODO: Lock checks to specific state changes?
            //if (animatronicName == string.Empty) {
            //    LinkedMovement.Log("DO THING!");
            //    // TODO: Get #?
            //    animatronicName = "New Animatronic";

            //    //var position = targetObjects[0].transform.position;
            //    //LinkedMovement.Log("Target pos: " + position.ToString());
            //    //Deco d2 = ScriptableSingleton<AssetManager>.Instance.instantiatePrefab<Deco>("98f0269770ff44247b38607fdb2cf837", position, Quaternion.identity);
            //    //if (d2 != null) {
            //    //    LinkedMovement.Log("Instantiated base!!!");
            //    //} else {
            //    //    LinkedMovement.Log("No instantiate base");
            //    //}
            //}

            if (creationStep == CreationSteps.Assemble && newStep == CreationSteps.Animate) {
                enterAnimateState();
            }
            if (creationStep == CreationSteps.Animate && newStep == CreationSteps.Assemble) {
                exitAnimateState();
            }

            creationStep = newStep;
        }

        public List<Pairing> getPairings() {
            return pairings;
        }

        public Pairing findPairingByID(string id) {
            foreach (var pairing in pairings) {
                if (pairing.pairingId == id) return pairing;
            }
            return null;
        }

        public void addPairing(Pairing pairing) {
            pairings.Add(pairing);
        }

        public bool removePairing(Pairing pairing) {
            return pairings.Remove(pairing);
        }

        public bool hasPairing(Pairing pairing) {
            return pairings.Contains(pairing);
        }

        public void generateOrigin() {
            LinkedMovement.Log("generateOrigin");

            if (originObject != null) {
                LinkedMovement.Log("Destroy existing origin");
                originObject.Kill();
                originObject = null;
            }

            var originPosition = findTargetsCenterPosition();
            originObject = ScriptableSingleton<AssetManager>.Instance.instantiatePrefab<Deco>("98f0269770ff44247b38607fdb2cf837", originPosition, Quaternion.identity);
            if (originObject != null) {
                LinkedMovement.Log("Instantiated origin!!!");
            } else {
                LinkedMovement.Log("FAILED to instantiate origin");
            }
        }

        public void pickTargetObject(Selection.Mode newMode) {
            LinkedMovement.Log("pickTargetObject");
            
            var options = selectionHandler.Options;
            if (options.Mode == newMode) {
                LinkedMovement.Log("Set target select none");
                options.Mode = Selection.Mode.None;
                disableSelectionHandler();
            }
            else {
                LinkedMovement.Log("Set target select mode " + newMode.ToString());
                options.Mode = newMode;
                enableSelectionHandler();
            }
        }

        public void addTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("addTargetBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }
            if (targetObjects.Contains(bo)) {
                LinkedMovement.Log("BuildableObject already added");
                return;
            }

            targetObjects.Add(bo);

            // Ensure target bo isn't already highlighted
            if (highlightHandles.ContainsKey(bo)) {
                highlightHandles[bo].remove();
                highlightHandles.Remove(bo);
            }

            // Build highlight
            List<Renderer> renderers = new List<Renderer>();
            bo.retrieveRenderersToHighlight(renderers);
            var highlightHandle = HighlightOverlayController.Instance.add(renderers, -1, Color.yellow);
            highlightHandles.Add(bo, highlightHandle);

            tryUpdateTargetsTransform();

            LinkedMovement.Log("Selected BO position:");
            LinkedMovement.Log("World: " + bo.gameObject.transform.position.ToString());
            LinkedMovement.Log("Local: " + bo.gameObject.transform.localPosition.ToString());
        }

        public void queueRemoveTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("queueRemoveTargetBuildableObject");
            queuedRemovalTargets.Add(bo);
        }

        public void removeTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("removeTargetBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }

            if (highlightHandles.ContainsKey(bo)) {
                highlightHandles[bo].remove();
                highlightHandles.Remove(bo);
            } else {
                LinkedMovement.Log("No highlight handle found");
            }

            targetObjects.Remove(bo);

            tryUpdateTargetsTransform();
        }

        //public void setSelectedBuildableObject(BuildableObject bo) {
        //    if (bo == null)
        //        return;
            
        //    if (isSettingBase) {
        //        // TODO: Add popup asking if non-animating base should add animation
        //        clearBaseObject();
        //        baseObject = bo;
        //        var baseAnimator = bo.GetComponent<Animator>();
        //        if (baseAnimator == null)
        //        {
        //            //windowManager.showInfoWindow("Selected base object has no animation.");
        //            // TODO:
        //            //windowManager.showCreateAnimationWindow();
        //            return;
        //        }

        //        //clearBaseObject();
        //        //baseObject = bo;
        //        setupBaseObject();
        //    }
        //    else if (isSettingTarget) {
        //        targetObjects.Add(bo);
        //        var p = bo.transform.position;
        //        var tarPosition = new Vector3(p.x, p.y, p.z);
        //        targetOriginTransformPositions.Add(bo, tarPosition);
        //    } else {
        //        LinkedMovement.Log("setSelectedBuildableObject while NOT SELECTING");
        //        return;
        //    }

        //    tryUpdateTargetsTransform();

        //    LinkedMovement.Log("Selected BO position:");
        //    LinkedMovement.Log("World: " + bo.gameObject.transform.position.ToString());
        //    LinkedMovement.Log("Local: " + bo.gameObject.transform.localPosition.ToString());
        //}

        //public void endSelection() {
        //    LinkedMovement.Log("Controller endSelection");

        //    var options = selectionHandler.Options;
        //    options.Mode = Selection.Mode.None;
        //    disableSelectionHandler();
        //}

        public void clearTargetObjects() {
            targetObjects.Clear();
        }

        public void clearAllSelections() {
            animatronicName = string.Empty;
            clearTargetObjects();
            queuedRemovalTargets.Clear();

            foreach (var handle in highlightHandles.Values) {
                handle.remove();
            }
            highlightHandles.Clear();
        }

        public void joinObjects() {
            LinkedMovement.Log("JOIN!");

            //if (selectedBlueprint != null) {
            //    LinkedMovement.Log("Create blueprint");

            //    catchCreatedObjects = true;
            //    blueprintCreatedObjects.Clear();
            //    selectedBlueprintBuilder.OnBuildTriggered += (Builder.OnBuildTriggeredHandler)(() => {
            //        LinkedMovement.Log("OnBuildTriggered");
            //        catchCreatedObjects = false;
                    
            //        LinkedMovement.Log("Got # created objects: " + blueprintCreatedObjects.Count);

            //        var pairing = new Pairing(baseObject.gameObject, blueprintCreatedObjects, null, pairName);
            //        pairing.setCustomData(true, basePositionOffset, baseRotationOffset, animationParams);
            //        pairing.connect();

            //        clearAllSelections();
            //        clearSelection();
            //    });
            //    MethodInfo methodInfo = selectedBlueprintBuilder.GetType().GetMethod("buildObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            //    methodInfo.Invoke(selectedBlueprintBuilder, null);
            //    return;
            //}

            List<GameObject> targetGOs = new List<GameObject>();
            foreach (var bo in targetObjects) {
                targetGOs.Add(bo.gameObject);
            }
            LinkedMovement.Log("Join # single targets: " + targetGOs.Count);

            // TODO
            //var pairing = new Pairing(baseObject.gameObject, targetGOs, null, pairName);
            //pairing.setCustomData(false, basePositionOffset, baseRotationOffset, animationParams);
            //pairing.connect();

            clearAllSelections();
            clearSelection();
        }

        public void tryToDeletePairing(Pairing pairing) {
            LinkedMovement.Log("tryToDeletePairing " + pairing.getPairingName());

            var found = hasPairing(pairing);
            if (!found) {
                LinkedMovement.Log("Pairing already removed from controller");
                return;
            }

            var hasBase = pairing.baseGO != null;
            var hasTargets = pairing.targetGOs.Count > 0;

            if (hasBase && hasTargets) {
                LinkedMovement.Log("Pairing still valid, don't delete");
                return;
            }

            // Remove the pairing now so we don't try to destroy things multiple times
            removePairing(pairing);

            if (hasBase) {
                LinkedMovement.Log("Destroy base object");
                GameObject.Destroy(pairing.baseGO);
                pairing.baseGO = null;
            }
            if (hasTargets) {
                LinkedMovement.Log("Destroy target objects");
                var cloneTargetGOs = new List<GameObject>(pairing.targetGOs);
                foreach (var targetGO in cloneTargetGOs) {
                    GameObject.Destroy(targetGO);
                }
                pairing.targetGOs.Clear();
            }
        }

        private void enableSelectionHandler() {
            if (!selectionHandlerEnabled)
                selectionHandlerEnabled = true;
        }

        private void disableSelectionHandler() {
            if (selectionHandlerEnabled)
                selectionHandlerEnabled = false;
        }

        private void clearSelection() {
            selectionHandler.DeselectAll();
        }

        private Vector3 findTargetsCenterPosition() {
            var startingPos = targetObjects[0].transform.position;
            var minX = startingPos.x;
            var maxX = startingPos.x;
            var minY = startingPos.y;
            var maxY = startingPos.y;
            var minZ = startingPos.z;
            var maxZ = startingPos.z;

            foreach (var target in targetObjects) {
                var tp = target.transform.position;
                if (tp.x < minX) minX = tp.x;
                if (tp.x > maxX) maxX = tp.x;
                if (tp.y < minY) minY = tp.y;
                if (tp.y > maxY) maxY = tp.y;
                if (tp.z < minZ) minZ = tp.z;
                if (tp.z > maxZ) maxZ = tp.z;
            }

            var midX = minX + ((maxX - minX) * 0.5f);
            var midY = minY + ((maxY - minY) * 0.5f);
            var midZ = minZ + ((maxZ - minZ) * 0.5f);

            return new Vector3(midX, midY, midZ);
        }

        private void enterAnimateState() {
            LinkedMovement.Log("Enter Animate State");

            animationParams = new LMAnimationParams(originObject.transform.position, originObject.transform.rotation.eulerAngles);
            
            // set targets parent
            foreach (var targetBO in targetObjects) {
                targetBO.transform.SetParent(originObject.transform);
            }
            //targetBO.transform.SetParent(baseObject.transform);
        }

        private void exitAnimateState() {
            LinkedMovement.Log("Exit Animate State");
            // TODO
            // Reset animation
            // clear targets parent
            foreach (var targetBO in targetObjects) {
                targetBO.transform.SetParent(null);
            }
        }

        private void finishAnimatronic() {
            // TODO
        }

        //private void resetBaseObject() {
        //    if (baseObject == null) return;

        //    LinkedMovement.Log("resetBaseObject");

        //    // There should always be an Animator on selected base objects
        //    var baseAnimator = baseObject.GetComponent<Animator>();
        //    if (baseAnimator == null) return;
        //    baseAnimator.Rebind();
        //    baseAnimator.Update(0f);

        //    if (!baseIsTriggerable) {
        //        baseAnimator.StartPlayback();
        //        baseAnimator.speed = 1f;
        //        baseIsTriggerable = false;
        //    }
        //}

        //private void setupBaseObject() {
        //    LinkedMovement.Log("setupBaseObject");

        //    var baseAnimator = baseObject.GetComponent<Animator>();
        //    baseAnimator.Rebind();
        //    baseAnimator.Update(0f);
        //    baseAnimator.StopPlayback();

        //    var modTriggerable = baseObject.GetComponent<ModAnimationTrigger>();
        //    if (modTriggerable != null) {
        //        LinkedMovement.Log("Is triggerable");
        //        baseIsTriggerable = true;
        //    } else {
        //        LinkedMovement.Log("Not triggerable, stop animation");
        //        baseAnimator.speed = 0f;
        //    }
        //}
    }
}
