// ATTRIB: HideScenery (very partial)
using LinkedMovement.AltUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovementController : MonoBehaviour {
        // TODO: Move this
        static public void AttachTargetToBase(Transform baseObject, Transform targetObject) {
            LinkedMovement.Log("Find attach parent between " + baseObject.name + " and " + targetObject.name);
            var baseTransform = baseObject;
            bool foundPlatform = false;

            var baseChildrenCount = baseTransform.childCount;
            for (var i = 0; i < baseChildrenCount; i++) {
                var child = baseTransform.GetChild(i);
                var childName = child.gameObject.name;
                if (childName.Contains("[Platform]")) {
                    foundPlatform = true;
                    baseTransform = child;
                    //LinkedMovement.Log("Using Platform");
                    break;
                }
            }
            // TODO: Not sure about this case
            if (!foundPlatform && baseChildrenCount > 0) {
                // Take child at 0
                baseTransform = baseTransform.GetChild(0);
                //LinkedMovement.Log("Using child 0");
            }

            targetObject.SetParent(baseTransform);
        }

        private BaseWindow mainWindow;

        private SelectionHandler selectionHandler;
        private bool selectionHandlerEnabled {
            get => selectionHandler.enabled;
            set => selectionHandler.enabled = value;
        }

        // TODO: Make better
        private bool isSettingBase = false;
        private bool isSettingTarget = false;

        public BuildableObject baseObject { get; private set; }
        public List<BuildableObject> targetObjects { get; private set; }

        public List<PairBase> pairBases = new List<PairBase>();
        public List<PairTarget> pairTargets = new List<PairTarget>();
        private List<Pairing> pairings = new List<Pairing>();

        private void Awake() {
            LinkedMovement.Log("LinkedMovementController Awake");
            targetObjects = new List<BuildableObject>();
            mainWindow = new MainWindow(this);
            selectionHandler = gameObject.AddComponent<SelectionHandler>();
            selectionHandler.controller = this;
            selectionHandler.enabled = false;
        }

        private void OnDisable() {
            disableSelectionHandler();
        }

        private void OnDestroy() {
            LinkedMovement.Controller = null;
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
            baseObject = null;
            targetObjects.Clear();
            pairBases.Clear();
            pairTargets.Clear();
            pairings.Clear();
        }

        private void Update() {
            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked()) {
                return;
            }

            if (InputManager.getKeyDown("LM_toggleGUI")) {
                LinkedMovement.Log("Toggle GUI");
                mainWindow.toggleWindowOpen();
            }
        }

        private void OnGUI() {
            if (mainWindow.isOpen) {
                mainWindow.drawWindow();
            }
        }

        public void addPairing(Pairing pairing) {
            pairings.Add(pairing);
        }

        public void addPairBase(PairBase pairBase) {
            pairBases.Add(pairBase);
        }

        public void addPairTarget(PairTarget pairTarget) {
            pairTargets.Add(pairTarget);
        }

        public void pickBaseObject() {
            LinkedMovement.Log("pickBaseObject");

            var newMode = Selection.Mode.Individual;
            var options = selectionHandler.Options;
            if (options.Mode == newMode) {
                LinkedMovement.Log("Set base select none");
                isSettingBase = false;
                options.Mode = Selection.Mode.None;
                disableSelectionHandler();
            } else {
                LinkedMovement.Log("Set base select individual");
                isSettingBase = true;
                isSettingTarget = false;
                options.Mode = newMode;
                enableSelectionHandler();
            }
        }

        public void pickTargetObject(int selectionMode) {
            LinkedMovement.Log("pickTargetObject");
            // TODO: Box select

            var newMode = Selection.Mode.Individual;
            var options = selectionHandler.Options;
            if (options.Mode == newMode) {
                LinkedMovement.Log("Set target select none");
                isSettingTarget = false;
                options.Mode = Selection.Mode.None;
                disableSelectionHandler();
            }
            else {
                LinkedMovement.Log("Set target select individual");
                isSettingBase = false;
                isSettingTarget = true;
                options.Mode = newMode;
                enableSelectionHandler();
            }
        }

        public void setSelectedBuildableObject(BuildableObject bo) {
            if (bo == null)
                return;
            
            if (isSettingBase) {
                baseObject = bo;
                isSettingBase = false;
            }
            else if (isSettingTarget) {
                targetObjects.Add(bo);
                isSettingTarget = false;
            } else {
                LinkedMovement.Log("setSelectedBuildableObject while NOT SELECTING");
                return;
            }

            LinkedMovement.Log("Selected BO position:");
            LinkedMovement.Log(bo.gameObject.transform.position.ToString());

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.None;
            disableSelectionHandler();
            clearSelection();
        }

        public void clearBaseObject() {
            baseObject = null;
        }

        public void clearTargetObject(BuildableObject target) {
            targetObjects.Remove(target);
        }

        public void clearTargetObjects() {
            targetObjects.Clear();
        }

        public void joinObjects() {
            LinkedMovement.Log("JOIN!");

            List<GameObject> targetGOs = new List<GameObject>();
            foreach (var bo in targetObjects) {
                targetGOs.Add(bo.gameObject);
            }

            var pairing = new Pairing(baseObject.gameObject, targetGOs);
            pairings.Add(pairing);

            baseObject.addCustomData(pairing.getPairBase());

            // Hate we iterate twice
            foreach (var bo in targetObjects) {
                // TODO: Offsets
                bo.addCustomData(pairing.getPairTarget());
            }

            baseObject = null;
            //targetObject = null;
            targetObjects.Clear();

            clearSelection();
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
    }
}
