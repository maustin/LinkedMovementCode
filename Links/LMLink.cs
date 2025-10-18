using LinkedMovement.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLink {
        private LMLinkParent parent;
        private List<LMLinkTarget> targets;

        private GameObject tempParentGameObject;
        private BuildableObject tempParentBuildableObject;

        private List<GameObject> tempTargetGameObjects;
        private List<BuildableObject> tempTargetBuildableObjects;

        private SelectionHandler selectionHandler;

        private bool _isEditing;
        public bool IsEditing {
            get => _isEditing;
            set {
                LinkedMovement.Log("LMLink.IsEditing SET to " + value.ToString());
                _isEditing = value;

                if (_isEditing) {
                    // create temps
                    if (parent != null) {
                        tempParentGameObject = parent.targetGameObject;
                        tempParentBuildableObject = parent.targetBuildableObject;
                    }

                    tempTargetGameObjects = new List<GameObject>();
                    tempTargetBuildableObjects = new List<BuildableObject>();
                    foreach (var target in targets) {
                        tempTargetGameObjects.Add(target.targetGameObject);
                        tempTargetBuildableObjects.Add(target.targetBuildableObject);
                    }
                } else {
                    // clear temps
                    tempParentGameObject = null;
                    tempParentBuildableObject = null;
                    tempTargetGameObjects = null;
                    tempTargetBuildableObjects = null;
                }
            }
        }

        private bool isNewLink = false;

        public LMLink() {
            LinkedMovement.Log("LMLink constructor");
            targets = new List<LMLinkTarget>();
        }

        public GameObject getParentGameObject() {
            if (tempParentGameObject != null) return tempParentGameObject;
            if (parent != null) {
                return parent.targetGameObject;
            }
            return null;
        }

        public BuildableObject getParentBuildableObject() {
            if (tempParentBuildableObject != null) return tempParentBuildableObject;
            if (parent != null) {
                return parent.targetBuildableObject;
            }
            return null;
        }

        public List<GameObject> getTargetGameObjects() {
            if (tempTargetGameObjects != null) {
                return tempTargetGameObjects;
            }
            var targetGOs = new List<GameObject>();
            foreach (var target in targets) {
                targetGOs.Add(target.targetGameObject);
            }
            return targetGOs;
        }

        public List<BuildableObject> getTargetBuildableObjects() {
            if (tempTargetBuildableObjects != null) {
                return tempTargetBuildableObjects;
            }
            var targetBOs = new List<BuildableObject>();
            foreach (var target in targets) {
                targetBOs.Add(target.targetBuildableObject);
            }
            return targetBOs;
        }

        public bool hasParent() {
            return parent != null || tempParentGameObject != null;
        }

        public bool isValid() {
            return hasParent() && targets != null && targets.Count > 0;
        }

        // TODO: Necessary? Just make clearSelectionHandler public?
        public void stopPicking() {
            LinkedMovement.Log("LMLink.stopPicking");

            clearSelectionHandler();
        }

        public void startPickingParent() {
            LinkedMovement.Log("LMLink.startPickingParent");

            clearSelectionHandler();

            selectionHandler = LinkedMovement.GetLMController().gameObject.AddComponent<SelectionHandler>();
            selectionHandler.OnAddBuildableObject += handlePickerAddObjectAsParent;
            // No OnRemove handler as this is not supported in parent-picking mode

            var options = selectionHandler.Options;
            options.Mode = Selection.Mode.Individual;
            selectionHandler.enabled = true;
        }

        public void startPickingTargets(Selection.Mode selectionMode) {
            LinkedMovement.Log("LMLink.startPickingTargets");

            clearSelectionHandler();

            selectionHandler = LinkedMovement.GetLMController().gameObject.AddComponent<SelectionHandler>();
            selectionHandler.OnAddBuildableObject += handlePickerAddObjectAsTarget;
            selectionHandler.OnRemoveBuildableObject += handlePickerRemoveObjectAsTarget;

            var options = selectionHandler.Options;
            options.Mode = selectionMode;
            selectionHandler.enabled = true;
        }

        public void setParentObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.setParentObject");

            // TODO: Remove parenting from targets if present

            if (IsEditing) {
                tempParentBuildableObject = buildableObject;
                tempParentGameObject = buildableObject.gameObject;
                LMUtils.AddObjectHighlight(buildableObject, Color.red);
            } else {
                // TODO: ?
            }
        }

        public void removeParentObject() {
            LinkedMovement.Log("LMLink.removeParentObject");

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject);
                // TODO: reparent

                tempParentGameObject = null;
                tempParentBuildableObject = null;
            }
        }

        public void addTargetObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.addTargetObject");

            // TODO: Possibly cache target original parent
            // Actually think this should be fine. If changes are discarded, reset temp targets parent to null
            // then rebuild original linkage.
            tempTargetBuildableObjects.Add(buildableObject);
            tempTargetGameObjects.Add(buildableObject.gameObject);
            LMUtils.AddObjectHighlight(buildableObject, Color.yellow);

            // TODO: reparent
        }

        public void removeTargetObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.removeTargetObject");

            if (!tempTargetBuildableObjects.Contains(buildableObject)) {
                // Not a target
                return;
            }

            LMUtils.RemoveObjectHighlight(buildableObject);

            // TODO: reparent

            tempTargetBuildableObjects.Remove(buildableObject);
            tempTargetGameObjects.Remove(buildableObject.gameObject);
        }

        public void discardChanges() {
            LinkedMovement.Log("LMLink.discardChanges");

            stopPicking();

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject);
            }
            foreach (var buildableObject in tempTargetBuildableObjects) {
                LMUtils.RemoveObjectHighlight(buildableObject);
            }

            IsEditing = false;
        }

        public void saveChanges() {
            LinkedMovement.Log("LMLink.saveChanges");

            // TODO
        }

        public void removeLink() {
            LinkedMovement.Log("LMLink.removeLink");

            // TODO
        }

        private void handlePickerAddObjectAsParent(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsParent");

            // TODO: Validate

            setParentObject(buildableObject);
            stopPicking();
        }

        private void handlePickerAddObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsTarget");

            // TODO: Validate

            addTargetObject(buildableObject);
        }

        private void handlePickerRemoveObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerRemoveObjectAsTarget");

            // TODO: Validate

            removeTargetObject(buildableObject);
        }

        private void clearSelectionHandler() {
            if (selectionHandler != null) {
                selectionHandler.enabled = false;
                selectionHandler.OnAddBuildableObject -= handlePickerAddObjectAsParent;
                selectionHandler.OnAddBuildableObject -= handlePickerAddObjectAsTarget;
                selectionHandler.OnRemoveBuildableObject -= handlePickerRemoveObjectAsTarget;

                GameObject.Destroy(selectionHandler);
                selectionHandler = null;
            }
        }

        private void setCustomData() {
            LinkedMovement.Log("LMLink.setCustomData");

            // TODO
        }

        private void removeCustomData() {
            LinkedMovement.Log("LMLink.removeCustomData");

            // TODO
        }
    }
}
