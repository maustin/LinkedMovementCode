// ATTRIB: HideScenery (very partial)
//using LinkedMovement.AltUI;
using LinkedMovement.UI.InGame;
using Parkitect;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovementController : MonoBehaviour {
        // TODO: Move this to utils
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

        // TODO: Move this too to utils
        static public List<BlueprintFile> FindDecoBlueprints(IList<BlueprintFile> blueprints) {
            var list = new List<BlueprintFile>();
            foreach (var blueprint in blueprints) {
                if (blueprint.getCategoryTags().Contains("Deco")) {
                    list.Add(blueprint);
                }
            }
            return list;
        }

        private MainWindow mainWindow;

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

        private List<Pairing> pairings = new List<Pairing>();

        public BlueprintBuilder selectedBlueprintBuilder { get; private set; }

        public BlueprintFile selectedBlueprint {
            get; private set;
        }

        public Vector3 basePositionOffset { get; private set; }
        public void setBasePositionOffset(Vector3 value) {
            basePositionOffset = value;
            tryToCreateBlueprintBuilder();
        }
        public Vector3 baseRotationOffset { get; private set; }
        public void setBaseRotationOffset(Vector3 value) {
            baseRotationOffset = value;
            tryToCreateBlueprintBuilder();
        }

        public bool catchCreatedObjects { get; private set; }
        private List<GameObject> blueprintCreatedObjects = new List<GameObject>();
        public void addBlueprintCreatedObject(GameObject go) {
            LinkedMovement.Log("Adding blueprintCreatedObject");
            blueprintCreatedObjects.Add(go);
        }

        public void setSelectedBlueprint(BlueprintFile blueprint) {
            LinkedMovement.Log("setSelectedBlueprint " + blueprint.getName());
            
            selectedBlueprint = blueprint;
            tryToCreateBlueprintBuilder();
        }

        private void tryToDestroyExistingBlueprintBuilder() {
            LinkedMovement.Log("try destroy existing blueprint builder");
            if (selectedBlueprintBuilder != null) {
                GameObject.Destroy(selectedBlueprintBuilder.gameObject);
                selectedBlueprintBuilder = null;
            }
        }

        private void tryToCreateBlueprintBuilder() {
            tryToDestroyExistingBlueprintBuilder();
            if (baseObject == null || selectedBlueprint == null) {
                LinkedMovement.Log("Cannot create blueprint builder, missing component");
                return;
            }

            LinkedMovement.Log("Create new builder");
            // Create new builder
            selectedBlueprintBuilder = UnityEngine.Object.Instantiate<BlueprintBuilder>(ScriptableSingleton<AssetManager>.Instance.blueprintBuilderGO);
            selectedBlueprintBuilder.skipDeco = false; // TODO: ????
            selectedBlueprintBuilder.filePath = selectedBlueprint.path;
        }

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
            LinkedMovement.ClearController();
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
            baseObject = null;
            targetObjects.Clear();
            pairings.Clear();
        }

        private void Update() {
            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked()) {
                return;
            }

            if (InputManager.getKeyDown("LM_toggleGUI")) {
                LinkedMovement.Log("Toggle GUI");
                mainWindow.isOpen = !mainWindow.isOpen;
            }
        }

        private void OnGUI() {
            if (mainWindow.isOpen) {
                mainWindow.Show();
            }
        }

        public void addPairing(Pairing pairing) {
            pairings.Add(pairing);
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

        public void pickTargetObject(Selection.Mode newMode) {
            LinkedMovement.Log("pickTargetObject");
            
            var options = selectionHandler.Options;
            if (options.Mode == newMode) {
                LinkedMovement.Log("Set target select none");
                isSettingTarget = false;
                options.Mode = Selection.Mode.None;
                disableSelectionHandler();
            }
            else {
                LinkedMovement.Log("Set target select mode " + newMode.ToString());
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
                tryToCreateBlueprintBuilder();
            }
            else if (isSettingTarget) {
                targetObjects.Add(bo);
            } else {
                LinkedMovement.Log("setSelectedBuildableObject while NOT SELECTING");
                return;
            }

            LinkedMovement.Log("Selected BO position:");
            LinkedMovement.Log("World: " + bo.gameObject.transform.position.ToString());
            LinkedMovement.Log("Local: " + bo.gameObject.transform.localPosition.ToString());
        }

        public void endSelection() {
            LinkedMovement.Log("Controller endSelection");

            isSettingBase = false;
            isSettingTarget = false;

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.None;
            disableSelectionHandler();
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

        public void clearAllSelections() {
            clearBaseObject();
            clearTargetObjects();
            tryToDestroyExistingBlueprintBuilder();
            selectedBlueprint = null;
        }

        public void joinObjects() {
            LinkedMovement.Log("JOIN!");

            // Attempt to reset base to starting animation position
            var baseAnimator = baseObject.GetComponent<Animator>();
            if (baseAnimator != null) {
                LinkedMovement.Log("Found Animator, reset time");
                //anim_gun.Play("idle", -1, 0f);
                baseAnimator.Rebind();
                baseAnimator.Update(0f);
            }
            else {
                LinkedMovement.Log("Couldn't find Animator");
            }

            if (selectedBlueprint != null) {
                LinkedMovement.Log("Create blueprint");

                catchCreatedObjects = true;
                blueprintCreatedObjects.Clear();
                selectedBlueprintBuilder.OnBuildTriggered += (Builder.OnBuildTriggeredHandler)(() => {
                    // TODO: This might need to be a delayed call?
                    LinkedMovement.Log("OnBuildTriggered");
                    catchCreatedObjects = false;
                    
                    LinkedMovement.Log("Got # created objects: " + blueprintCreatedObjects.Count);

                    var pairing = new Pairing(baseObject.gameObject, blueprintCreatedObjects);
                    pairing.setCustomData(true, basePositionOffset, baseRotationOffset);
                    pairing.connect();

                    clearAllSelections();
                    clearSelection();
                });
                MethodInfo methodInfo = selectedBlueprintBuilder.GetType().GetMethod("buildObjects", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(selectedBlueprintBuilder, null);
                return;
            }

            List<GameObject> targetGOs = new List<GameObject>();
            foreach (var bo in targetObjects) {
                targetGOs.Add(bo.gameObject);
            }

            var pairing = new Pairing(baseObject.gameObject, targetGOs);
            // TODO: Preview single objects & use offsets
            pairing.setCustomData();
            pairing.connect();

            clearAllSelections();
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
