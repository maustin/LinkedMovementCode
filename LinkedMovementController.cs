using LinkedMovement.UI;
using LinkedMovement.Utils;
using Parkitect.UI;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovementController : MonoBehaviour {
        public enum CreationSteps {
            Select,
            Assemble,
            Animate,
            Finish,
        }
        private CreationSteps creationStep = CreationSteps.Select;

        public enum PickingMode {
            Origin,
            Target,
        }
        public PickingMode pickingMode;

        // TODO: !!! This needs to be split into a couple different classes

        // TODO: 9-13
        // Bug: Create an animation
        // Create new animation
        // Select original origin as a target
        // Select new origin object
        // Enter Animate panel
        // Observe target moves incorrectly

        private SelectionHandler selectionHandler;
        private bool selectionHandlerEnabled {
            get => selectionHandler.enabled;
            set => selectionHandler.enabled = value;
        }

        public WindowManager windowManager;

        // TODO: I don't like having 'animationParams', 'targetObjects', and 'originObject' just hanging out
        // as public props on the controller. Need some access controls and better naming.
        public LMAnimationParams animationParams;
        public List<BuildableObject> targetObjects { get; private set; }

        public BuildableObject originObject { get; private set; }

        // TODO: Does this need to be a getter/setter?
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
        
        // TODO: Love to eliminate this. Currently only used when creating new animatronic as there isn't
        // an "animationParams" to carry the name until the "Animate" step.
        public string animatronicName = string.Empty;

        public Sequence sampleSequence;

        private Pairing targetPairing;

        private Pairing pendingPairingForDeletion;

        private List<BuildableObject> queuedRemovalTargets = new List<BuildableObject>();
        
        private List<Pairing> pairings = new List<Pairing>();

        private void Awake() {
            LinkedMovement.Log("LinkedMovementController Awake");
            targetObjects = new List<BuildableObject>();
            
            windowManager = new WindowManager();
            selectionHandler = gameObject.AddComponent<SelectionHandler>();
            selectionHandler.controller = this;
            selectionHandler.enabled = false;
        }

        private void OnDisable() {
            LinkedMovement.Log("LinkedMovementController OnDisable");
            disableSelectionHandler();
            // TODO: Do all Sequences need to be tracked here so they can be killed?
        }

        private void OnDestroy() {
            LinkedMovement.Log("LinkedMovementController OnDestroy");
            resetController();
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
            pairings.Clear();
            if (windowManager != null) {
                windowManager.destroy();
                windowManager = null;
            }
            LinkedMovement.ClearController();
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
            // TODO: What is dis?
            if (mouseTool == null) return;

            foreach (var pairing in pairings) {
                pairing.frameUpdate();
            }
        }

        private void OnGUI() {
            if (OptionsMenu.instance != null) return;

            float uiScale = Settings.Instance.uiScale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1f));
            windowManager.DoGUI();
            GUI.matrix = Matrix4x4.identity;
        }

        public CreationSteps getCreationStep() { return creationStep; }

        public void setCreationStep(CreationSteps newStep) {
            LinkedMovement.Log("Controller.setCreationStep " + newStep.ToString());
            if (newStep == creationStep) {
                LinkedMovement.Log("Already in creation step " + newStep.ToString());
                return;
            }

            disableSelectionHandler();

            // TODO: Prolly need state machine here
            if (animatronicName == string.Empty) {
                // TODO: Get #?
                animatronicName = "New Animatronic";
            }

            if (creationStep == CreationSteps.Assemble && newStep == CreationSteps.Animate) {
                enterAnimateState();
            }

            if (creationStep == CreationSteps.Animate && newStep != CreationSteps.Finish) {
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

        public void setTargetPairing(Pairing pairing) {
            LinkedMovement.Log("Controller.setTargetPairing " + pairing.pairingName);
            targetPairing = pairing;
            
            pairing.disconnect();

            originObject = LMUtils.GetBuildableObjectFromGameObject(targetPairing.baseGO);
            targetObjects = LMUtils.GetBuildableObjectsFromGameObjects(targetPairing.targetGOs);

            LMUtils.AddObjectHighlight(originObject, Color.red);
            foreach (var target in targetObjects) {
                LMUtils.AddObjectHighlight(target, Color.yellow);
            }

            animationParams = LMAnimationParams.Duplicate(targetPairing.pairBase.animParams);
            
            rebuildSampleSequence();
        }

        public void clearTargetPairing() {
            LMUtils.ResetObjectHighlights();
            targetPairing = null;
        }

        public void discardChanges() {
            LinkedMovement.Log("Controller.discardChanges");
            LMUtils.ResetObjectHighlights();
            killSampleSequence();
            restartAssociatedAnimations(false);

            if (targetPairing != null) {
                // Reset parents for new targets
                var originalTargets = LMUtils.GetBuildableObjectsFromGameObjects(targetPairing.targetGOs);
                LMUtils.ResetUnusedTargets(targetObjects, originalTargets);

                LinkedMovement.Log("Reconnect targetPairing");
                targetPairing.connect();
                targetPairing = null;
                originObject = null;

                targetObjects.Clear();
            }

            resetController();
        }

        // TODO
        public void confirmDeletePairing(Pairing pairing) {
            pendingPairingForDeletion = pairing;
            // TODO: Window
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

        public Pairing findPairingByBaseGameObject(GameObject gameObject) {
            foreach (var pairing in pairings) {
                if (pairing.baseGO == gameObject) {
                    return pairing;
                }
            }
            LinkedMovement.Log("No pairings found");
            return null;
        }

        public void addPairing(Pairing pairing) {
            LinkedMovement.Log("Controller.addPairing " + pairing.pairingId);
            if (pairings.Contains(pairing)) {
                LinkedMovement.Log("Pairing already present");
                return;
            }
            pairings.Add(pairing);
        }

        public bool removePairing(Pairing pairing) {
            LinkedMovement.Log("Controller.removePairing " + pairing.pairingId);
            return pairings.Remove(pairing);
        }

        public bool hasPairing(Pairing pairing) {
            var hasPairing = pairings.Contains(pairing);
            LinkedMovement.Log("Controller.hasPairing? " + hasPairing + ", id: " + pairing.pairingId);
            return hasPairing;
        }

        public void generateOrigin() {
            LinkedMovement.Log("Controller.generateOrigin");
            disableSelectionHandler();

            removeOrigin();

            restartAssociatedAnimations(true);

            var originPosition = LMUtils.FindBuildObjectsCenterPosition(targetObjects);
            originObject = ScriptableSingleton<AssetManager>.Instance.instantiatePrefab<Deco>("98f0269770ff44247b38607fdb2cf837", originPosition, Quaternion.identity);
            // Use built-in prefab for base. Otherwise trying to use mod object will fail in non-mod (e.g. scenario) env.
            //originObject = ScriptableSingleton<AssetManager>.Instance.instantiatePrefab<Deco>(Prefabs.ScenicCube, originPosition, Quaternion.identity);
            if (originObject == null) {
                throw new Exception("FAILED TO CREATE ORIGIN OBJECT");
            }
            originObject.setDisplayName("LMOriginBase");
            originObject.setCanBeDestroyedByPlayer(false);
            // TODO: Configurable generated origin color?
            originObject.GetComponent<CustomColors>().setColor(new Color(1f, 0f, 1f), 0);
            //originObject.GetComponent<CustomSize>().setValue(0.1f);
            Destroy(originObject.GetComponent<ChunkedMesh>());

            LMUtils.AddObjectHighlight(originObject, Color.red);
        }

        public void removeOrigin() {
            LinkedMovement.Log("Controller.removeOrigin");
            if (originObject != null) {
                LMUtils.RemoveObjectHighlight(originObject);
                // Only re-enable chunker if the origin is not already a PairTarget
                if (LMUtils.GetPairTargetFromSerializedMonoBehaviour(originObject) == null)
                    LMUtils.SetChunkedMeshEnalbedIfPresent(originObject, true);

                LinkedMovement.Log("Destroy existing origin: " + originObject.getName());
                // Only destroy the origin if it was generated
                if (LMUtils.IsGeneratedOrigin(originObject))
                    originObject.Kill();
                originObject = null;
            }
        }

        public void pickingOriginObject() {
            LinkedMovement.Log("Controller.pickOriginObject");
            pickingMode = PickingMode.Origin;

            selectionHandler.Options.Mode = Selection.Mode.Individual;
            enableSelectionHandler();
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

            LinkedMovement.Log("Object position: " + bo.transform.position.ToString());
            LinkedMovement.Log("Object local position: " + bo.transform.localPosition.ToString());

            // We have to disable the ChunkedMesh component so tweens update the visual location
            LMUtils.SetChunkedMeshEnalbedIfPresent(bo, false);

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
            disableSelectionHandler();
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

            // TODO: STATE MACHINE! Don't like this.
            if (targetPairing != null) {
                LinkedMovement.Log("Has target Pairing, reset sequences and attach");
                killSampleSequence();
                rebuildSampleSequence();
                LMUtils.AttachTargetToBase(originObject.transform, bo.transform);
            }
        }

        public void queueRemoveTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("Controller.queueRemoveTargetBuildableObject");
            queuedRemovalTargets.Add(bo);
        }

        private void removeTargetBuildableObject(BuildableObject bo) {
            LinkedMovement.Log("Controller.removeTargetBuildableObject");
            if (bo == null) {
                LinkedMovement.Log("null BuildableObject");
                return;
            }

            LMUtils.SetChunkedMeshEnalbedIfPresent(bo, true);
            LMUtils.RemoveObjectHighlight(bo);

            killSampleSequence();
            // TODO: STATE MACHINE! Don't like this.
            if (targetPairing != null) {
                rebuildSampleSequence();
            }

            bo.transform.parent = null;
            
            targetObjects.Remove(bo);
        }

        public void clearTargetObjects() {
            LinkedMovement.Log("Controller.clearTargetObjects");
            foreach (var target in targetObjects) {
                LMUtils.SetChunkedMeshEnalbedIfPresent(target, true);
                if (target != null && target.transform != null)
                    target.transform.parent = null;
            }
            targetObjects.Clear();
        }

        public void resetController() {
            LinkedMovement.Log("Controller.resetController");
            disableSelectionHandler();

            setCreationStep(CreationSteps.Select);

            targetPairing = null;
            animatronicName = string.Empty;
            animationParams = null;

            killSampleSequence();
            clearSelection();
            clearTargetObjects();

            removeOrigin();

            queuedRemovalTargets.Clear();
            LMUtils.ResetObjectHighlights();

            clearTargetPairing();
        }

        // Only use when creating a new Pairing
        private void joinObjects() {
            LinkedMovement.Log("Controller.joinObjects");

            if (LMUtils.IsGeneratedOrigin(originObject)) {
                // "Officially" create the generated origin object
                originObject.Initialize();
            }
            // TODO: Change to stop/start ?
            restartAssociatedAnimations(false);

            List<GameObject> targetGOs = new List<GameObject>();
            foreach (var bo in targetObjects) {
                targetGOs.Add(bo.gameObject);
            }
            LinkedMovement.Log($"Join {targetGOs.Count} targets");
            targetObjects.Clear();

            // TODO: This is duplicating data
            animationParams.name = animatronicName;

            var pairing = new Pairing(originObject.gameObject, targetGOs, null, animatronicName);

            // TODO: Eliminate origin offsets
            pairing.setCustomData(false, default, default, animationParams);

            originObject = null;

            resetController();

            pairing.connect();
        }

        // Only use when saving changes to an existing Pairing
        public void saveChanges() {
            LinkedMovement.Log("Controller.saveChanges");
            LMUtils.ResetObjectHighlights();
            killSampleSequence();
            restartAssociatedAnimations(false);

            targetPairing.updatePairing(animationParams, targetObjects);
            targetPairing.connect();
            targetPairing = null;

            originObject = null;
            
            targetObjects.Clear();

            resetController();
        }

        public void tryToDeletePairing(Pairing pairing) {
            LinkedMovement.Log("Controller.tryToDeletePairing " + pairing.getPairingName());

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

            if (sampleSequence.isAlive) {
                LinkedMovement.Log("Sequence is alive, stop!");
                sampleSequence.Stop();
            }
            
            if (originObject == null || animationParams == null) {
                return;
            }

            // TODO: Does this need to reset targets as well?
            LMUtils.ResetTransformLocals(originObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);
        }

        public void rebuildSampleSequence() {
            LinkedMovement.Log("Controller.rebuildSampleSequence");
            killSampleSequence();

            if (originObject == null) {
                LinkedMovement.Log("NO ORIGIN BO!");
                return;
            }

            restartAssociatedAnimations(true);

            sampleSequence = LMUtils.BuildAnimationSequence(originObject.transform, animationParams, true);
        }

        private List<GameObject> getAssociatedGameObjects() {
            var associated = new List<GameObject>();
            if (originObject != null && originObject.gameObject != null) {
                //LMUtils.RestartAssociatedAnimation(originObject.gameObject);
                associated.Add(originObject.gameObject);
            }
            if (targetObjects != null && targetObjects.Count > 0) {
                foreach (var targetObject in targetObjects) {
                    if (targetObject.gameObject != null) {
                        associated.Add(targetObject.gameObject);
                    }
                }
            }
            LinkedMovement.Log($"Controller.getAssociatedGameObjects got {associated.Count} associated objects");
            return associated;
        }

        private void stopAssociatedAnimations(bool isEditing) {
            LinkedMovement.Log("Controller.stopAssociatedAnimations");
            var stopList = getAssociatedGameObjects();

            if (stopList.Count > 0) {
                LinkedMovement.Log($"Trying to stop {stopList.Count} objects");
                LMUtils.EditAssociatedAnimations(stopList, LMUtils.AssociatedAnimationEditMode.Stop, isEditing);
            }
        }

        private void startAssociatedAnimations(bool isEditing) {
            LinkedMovement.Log("Controller.startAssociatedAnimations");
            var startList = getAssociatedGameObjects();

            if (startList.Count > 0) {
                LinkedMovement.Log($"Trying to start {startList.Count} objects");
                LMUtils.EditAssociatedAnimations(startList, LMUtils.AssociatedAnimationEditMode.Start, isEditing);
            }
        }

        private void restartAssociatedAnimations(bool isEditing) {
            LinkedMovement.Log("Controller.restartAssociatedAnimations");
            var restartList = getAssociatedGameObjects();

            if (restartList.Count > 0) {
                LinkedMovement.Log($"Trying to restart {restartList.Count} objects");
                LMUtils.EditAssociatedAnimations(restartList, LMUtils.AssociatedAnimationEditMode.Restart, isEditing);
            }
        }

        private void enableSelectionHandler() {
            if (!selectionHandlerEnabled)
                selectionHandlerEnabled = true;
        }

        public void disableSelectionHandler() {
            if (selectionHandlerEnabled)
                selectionHandlerEnabled = false;
        }

        private void clearSelection() {
            if (selectionHandler != null)
                selectionHandler.DeselectAll();
        }

        private void enterAnimateState() {
            LinkedMovement.Log("Controller.enterAnimateState START");

            if (animationParams == null) {
                animationParams = new LMAnimationParams();
            }

            // Restart parent sequences so target is attached when parent is at starting location
            //restartAssociatedAnimations(true);
            stopAssociatedAnimations(true);

            animationParams.setOriginalValues(originObject.transform);

            animationParams.setStartingValues(originObject.transform);

            LinkedMovement.Log("Attach targets");
            // set targets parent
            foreach (var targetBO in targetObjects) {
                LMUtils.AttachTargetToBase(originObject.transform, targetBO.transform);
            }

            startAssociatedAnimations(true);

            rebuildSampleSequence();

            LinkedMovement.Log("Controller.enterAnimateState FINISH");
        }

        private void exitAnimateState() {
            LinkedMovement.Log("Exit Animate State");
            killSampleSequence();
            restartAssociatedAnimations(true);
            foreach (var targetBO in targetObjects) {
                targetBO.transform.SetParent(null);
            }
        }

        private void finishAnimatronic() {
            LinkedMovement.Log("Finish Animatronic");
            killSampleSequence();
            joinObjects();
        }

    }
}
