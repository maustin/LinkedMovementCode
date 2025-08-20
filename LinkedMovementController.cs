using DG.Tweening;
using LinkedMovement.UI;
using LinkedMovement.Utils;
using Parkitect.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovementController : MonoBehaviour {
        // TODO: Move enum out
        public enum CreationSteps {
            Select,
            Assemble,
            Animate,
            Finish,
        }

        private enum PickingMode {
            Origin,
            Target,
        }
        private PickingMode pickingMode;

        // TODO: !!! This needs to be split into a couple different classes

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
        
        public string animatronicName = string.Empty;

        public Sequence sampleSequence;

        public List<BuildableObject> animatedBuildableObjects { get; private set; }
        public void addAnimatedBuildableObject(BuildableObject bo) {
            animatedBuildableObjects.Add(bo);
        }
        public void removeAnimatedBuildableObject(BuildableObject bo) {
            animatedBuildableObjects.Remove(bo);
        }

        private List<BuildableObject> queuedRemovalTargets = new List<BuildableObject>();
        
        private List<Pairing> pairings = new List<Pairing>();

        private void Awake() {
            LinkedMovement.Log("LinkedMovementController Awake");
            targetObjects = new List<BuildableObject>();
            
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
            targetObjects.Clear();
            pairings.Clear();
            if (windowManager != null) {
                windowManager.destroy();
                windowManager = null;
            }
        }

        private void Update() {
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
            // TODO: What is dis? (Think is handler when game menu is open)
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

            // TODO: Should be elsewhere (state machine!)
            if (animatronicName == string.Empty) {
                // TODO: Get #?
                animatronicName = "New Animatronic";
            }

            // TODO: Really need state machine to cover below
            if (creationStep == CreationSteps.Assemble && newStep == CreationSteps.Animate) {
                enterAnimateState();
            }

            if (creationStep == CreationSteps.Animate) {
                exitAnimateState();
            }

            if (newStep == CreationSteps.Finish) {
                finishAnimatronic();
            } else {
                // finishAnimatronic resets the state so we don't want to change it back
                // Again, really need state machine
                creationStep = newStep;
            }
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

            removeOrigin();

            var originPosition = LMUtils.FindBuildObjectsCenterPosition(targetObjects);
            originObject = ScriptableSingleton<AssetManager>.Instance.instantiatePrefab<Deco>("98f0269770ff44247b38607fdb2cf837", originPosition, Quaternion.identity);
            if (originObject == null) {
                throw new Exception("FAILED TO CREATE ORIGIN OBJECT");
            }
            LMUtils.AddObjectHighlight(originObject, Color.red);
        }

        public void removeOrigin() {
            LinkedMovement.Log("Controller.removeOrigin");
            if (originObject != null) {
                LMUtils.RemoveObjectHighlight(originObject);
                LinkedMovement.Log("Destroy existing origin");
                originObject.Kill();
                originObject = null;
            }
        }

        public void pickingOriginObject() {
            LinkedMovement.Log("Controller.pickOriginObject");
            pickingMode = PickingMode.Origin;
        }

        public void pickingTargetObject(Selection.Mode newMode) {
            LinkedMovement.Log("pickTargetObject");
            pickingMode = PickingMode.Target;
            
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

        public void handleAddObjectSelection(BuildableObject bo) {
            LinkedMovement.Log("Controller.handleObjectSelection");
            if (pickingMode == PickingMode.Origin)
                addOriginBuildableObject(bo);
            else if (pickingMode == PickingMode.Target)
                addTargetBuildableObject(bo);
            else
                throw new Exception("UNKNOWN PICKING MODE");
        }

        public void handleRemoveObjectSelection(BuildableObject bo) {
            LinkedMovement.Log("Controller.handleRemoveObjectSelection");
            if (pickingMode == PickingMode.Target)
                removeTargetBuildableObject(bo);
        }

        private void addOriginBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("Controller.addOriginBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }

            LMUtils.AddObjectHighlight(bo, Color.red);
            originObject = bo;
        }

        private void addTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("Controller.addTargetBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }
            if (targetObjects.Contains(bo)) {
                LinkedMovement.Log("BuildableObject already added");
                return;
            }

            targetObjects.Add(bo);

            LMUtils.AddObjectHighlight(bo, Color.yellow);
            
            LinkedMovement.Log("Selected BO position:");
            LinkedMovement.Log("World: " + bo.gameObject.transform.position.ToString());
            LinkedMovement.Log("Local: " + bo.gameObject.transform.localPosition.ToString());
        }

        public void queueRemoveTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("queueRemoveTargetBuildableObject");
            queuedRemovalTargets.Add(bo);
        }

        private void removeTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("removeTargetBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }

            LMUtils.RemoveObjectHighlight(bo);
            
            targetObjects.Remove(bo);
        }

        public void clearTargetObjects() {
            LinkedMovement.Log("Controller.clearTargetObjects");
            foreach (var target in targetObjects) {
                if (target != null && target.transform != null)
                    target.transform.parent = null;
            }
            targetObjects.Clear();
        }

        public void clearAllSelections() {
            LinkedMovement.Log("Controller.clearAllSelections");

            setCreationStep(CreationSteps.Select);

            animatronicName = string.Empty;
            animationParams = null;

            killSampleSequence();
            clearSelection();
            clearTargetObjects();

            removeOrigin();

            queuedRemovalTargets.Clear();
            LMUtils.ResetObjectHighlights();
        }

        public void joinObjects() {
            LinkedMovement.Log("Controller.joinObjects");

            // "Officially" create the origin object
            originObject.Initialize();

            List<GameObject> targetGOs = new List<GameObject>();
            foreach (var bo in targetObjects) {
                targetGOs.Add(bo.gameObject);
            }
            LinkedMovement.Log("Join # single targets: " + targetGOs.Count);
            targetObjects.Clear();

            // TODO: This is duplicating data
            animationParams.name = animatronicName;

            var pairing = new Pairing(originObject.gameObject, targetGOs, null, animatronicName);
            // TODO: Eliminate origin offsets
            pairing.setCustomData(false, default, default, animationParams);
            pairing.connect();

            originObject = null;

            clearAllSelections();
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

        public void killSampleSequence() {
            LinkedMovement.Log("Controller.killSampleSequence");
            if (sampleSequence == null) {
                LinkedMovement.Log("No sequence to kill");
                return;
            }

            sampleSequence.Kill();
            sampleSequence = null;

            if (originObject != null && originObject.transform != null && animationParams != null) {
                originObject.transform.position = animationParams.startingPosition;
                originObject.transform.rotation = Quaternion.Euler(animationParams.startingRotation);
            }
        }

        public void rebuildSampleSequence() {
            LinkedMovement.Log("rebuildSequence");
            killSampleSequence();

            if (originObject == null) {
                LinkedMovement.Log("NO ORIGIN BO!");
                return;
            }

            sampleSequence = LMUtils.BuildAnimationSequence(originObject.transform, animationParams, true);
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
            if (selectionHandler != null)
                selectionHandler.DeselectAll();
        }

        private void enterAnimateState() {
            LinkedMovement.Log("Enter Animate State");

            if (animationParams == null) {
                animationParams = new LMAnimationParams(originObject.transform.position, originObject.transform.rotation.eulerAngles);
            }
            
            // set targets parent
            foreach (var targetBO in targetObjects) {
                targetBO.transform.SetParent(originObject.transform);
            }

            rebuildSampleSequence();
        }

        private void exitAnimateState() {
            LinkedMovement.Log("Exit Animate State");
            killSampleSequence();
            foreach (var targetBO in targetObjects) {
                targetBO.transform.SetParent(null);
            }
        }

        private void finishAnimatronic() {
            LinkedMovement.Log("Finish Animatronic");
            joinObjects();
        }

    }
}
