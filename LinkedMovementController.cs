// ATTRIB: HideScenery (very partial)
using LinkedMovement.AltUI;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    class LinkedMovementController : MonoBehaviour {
        // TODO: Move this
        static private void AttachTargetToBase(Transform baseObject, Transform targetObject) {
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
                    LinkedMovement.Log("Using Platform");
                    break;
                }
            }
            if (!foundPlatform && baseChildrenCount > 0) {
                // Take child at 0
                baseTransform = baseTransform.GetChild(0);
                LinkedMovement.Log("Using child 0");
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

        public BuildableObject baseObject;
        public BuildableObject targetObject;

        private List<Pairing> pairings = new List<Pairing>();

        private void Awake() {
            LinkedMovement.Log("LinkedMovementController Awake");
            mainWindow = new MainWindow(this);
            selectionHandler = gameObject.AddComponent<SelectionHandler>();
            selectionHandler.controller = this;
            selectionHandler.enabled = false;
        }

        private void Start() {
            // TODO: Setup pairings
        }

        private void OnDisable() {
            disableSelectionHandler();
        }

        private void OnDestroy() {
            if (selectionHandler != null) {
                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
        }

        private void Update() {
            if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked()) {
                return;
            }

            //foreach (Pairing pairing in pairings) {
            //    pairing.Update();
            //}

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

        public void pickTargetObject() {
            LinkedMovement.Log("pickTargetObject");

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
                targetObject = bo;
                isSettingTarget = false;
            } else {
                LinkedMovement.Log("setSelectedBuildableObject while NOT SELECTING");
                return;
            }

            LinkedMovement.Log("Selected BO position:");
            LinkedMovement.Log(bo.gameObject.transform.position.ToString());
            //LinkedMovement.Log(bo.gameObject.transform.parent.name);

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.None;
            disableSelectionHandler();
            clearSelection();
        }

        public void clearBaseObject() {
            baseObject = null;
        }

        public void clearTargetObject() {
            targetObject = null;
        }

        public void joinObjects() {
            LinkedMovement.Log("JOIN!");

            // If ChunkedMesh, it's a built-in object and we need to handle it
            var targetChunkedMesh = targetObject.GetComponent<ChunkedMesh>();

            if (targetChunkedMesh != null) {
                LinkedMovement.Log("Target is built-in deco object, enable movement");
                targetChunkedMesh.enabled = false;
                var targetMeshRenderer = targetObject.GetComponent<MeshRenderer>();
                targetMeshRenderer.enabled = true;
            }

            targetObject.transform.position = baseObject.transform.position;

            AttachTargetToBase(baseObject.transform, targetObject.transform);

            pairings.Add(new Pairing(baseObject.gameObject, targetObject.gameObject));

            baseObject = null;
            targetObject = null;

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

    class Pairing {
        public GameObject baseGO;
        public GameObject targetGO;

        public Pairing(GameObject baseGO, GameObject targetGO) {
            this.baseGO = baseGO;
            this.targetGO = targetGO;

            //var targetComponents = targetGO.GetComponents<Component>();
            //LinkedMovement.Log("Target has " + targetComponents.Length.ToString() + " components");
            //foreach (var component in targetComponents) {
            //    LinkedMovement.Log("Component: " + component.name + ", iID: " + component.GetInstanceID());
            //    var t = component.GetType();
            //    LinkedMovement.Log("Type: " + t.Name);
            //    LinkedMovement.Log("---");
            //}

            //LinkedMovement.Log("Target GO has " + targetGO.transform.childCount.ToString() + " children");
            //foreach (Transform child in targetGO.transform) {
            //    LinkedMovement.Log("Child transform GO: " + child.gameObject.name);
            //}
        }

        //public void Update() {
        //    var baseTransform = baseGO.transform;
        //    bool foundPlatform = false;

        //    var baseChildrenCount = baseTransform.childCount;
        //    for (var i = 0; i < baseChildrenCount; i++) {
        //        var child = baseTransform.GetChild(i);
        //        var childName = child.gameObject.name;
        //        if (childName.Contains("[Platform]")) {
        //            foundPlatform = true;
        //            baseTransform = child;
        //            //LinkedMovement.Log("Using Platform");
        //            break;
        //        }
        //    }
        //    if (!foundPlatform && baseChildrenCount > 0) {
        //        // Take child at 0
        //        baseTransform = baseTransform.GetChild(0);
        //        //LinkedMovement.Log("Using child 0");
        //    }
            
        //    //targetGO.transform.SetPositionAndRotation(baseTransform.position, baseTransform.rotation);

        //    //var targetRotationEuler = baseTransform.eulerAngles + new Vector3(-180f, 0f, 0f);

        //    //var targetRotationEuler = baseTransform.eulerAngles + new Vector3(0f, 0f, 0f);
        //    var targetRotationEuler = baseTransform.eulerAngles + new Vector3(90f, 0f, 0f);
        //    targetGO.transform.eulerAngles = targetRotationEuler;

        //    targetGO.transform.position = baseTransform.position;
        //}
    }
}
