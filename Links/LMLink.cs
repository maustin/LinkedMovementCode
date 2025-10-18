using LinkedMovement.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLink {
        private string _name = string.Empty;
        public string name {
            get {
                if (linkParent != null) {
                    return linkParent.name;
                }
                return _name;
            }
            set {
                if (linkParent != null) {
                    linkParent.name = value;
                }
                _name = value;
            }
        }

        private string _id = string.Empty;
        public string id {
            get {
                if (linkParent != null) {
                    return linkParent.id;
                }
                return _id;
            }
            set {
                if (linkParent != null) {
                    linkParent.id = value;
                }
                _id = value;
            }
        }

        private LMLinkParent linkParent;
        private List<LMLinkTarget> linkTargets;

        private GameObject tempParentGameObject;
        private BuildableObject tempParentBuildableObject;

        private List<GameObject> tempTargetGameObjects;
        private List<BuildableObject> tempTargetBuildableObjects;

        private SelectionHandler selectionHandler;

        private bool _isEditing;
        public bool IsEditing {
            get => _isEditing;
            set {
                _isEditing = value;

                if (_isEditing) {
                    // create temps
                    if (linkParent != null) {
                        tempParentGameObject = linkParent.targetGameObject;
                        tempParentBuildableObject = linkParent.targetBuildableObject;
                    }

                    tempTargetGameObjects = new List<GameObject>();
                    tempTargetBuildableObjects = new List<BuildableObject>();
                    foreach (var target in linkTargets) {
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
            LinkedMovement.Log("LMLink base constructor");
            linkTargets = new List<LMLinkTarget>();
            _name = "New Link";
            _id = Guid.NewGuid().ToString();
        }

        public LMLink(LMLinkParent linkParent, List<LMLinkTarget> linkTargets) {
            LinkedMovement.Log("LMLink constructor with params");

            this.linkParent = linkParent;
            this.linkTargets = linkTargets;

            _name = linkParent.name;
            _id = linkParent.id;
        }

        public void initializeWith(BuildableObject parentBuildableObject, List<BuildableObject> targetBuildableObjects) {
            LinkedMovement.Log("LMLink.initializeWith");

            removeCustomData();

            var newParent = new LMLinkParent(_name, _id, parentBuildableObject);
            linkParent = newParent;

            var newTargets = new List<LMLinkTarget>();
            foreach (var target in targetBuildableObjects) {
                var newTarget = new LMLinkTarget(_id, target);
                newTargets.Add(newTarget);
            }
            linkTargets = newTargets;

            setCustomData();
        }

        public GameObject getParentGameObject() {
            if (tempParentGameObject != null) return tempParentGameObject;
            if (linkParent != null) {
                return linkParent.targetGameObject;
            }
            return null;
        }

        public BuildableObject getParentBuildableObject() {
            if (tempParentBuildableObject != null) return tempParentBuildableObject;
            if (linkParent != null) {
                return linkParent.targetBuildableObject;
            }
            return null;
        }

        public List<GameObject> getTargetGameObjects() {
            if (tempTargetGameObjects != null) {
                return tempTargetGameObjects;
            }
            var targetGOs = new List<GameObject>();
            foreach (var target in linkTargets) {
                targetGOs.Add(target.targetGameObject);
            }
            return targetGOs;
        }

        public List<BuildableObject> getTargetBuildableObjects() {
            if (tempTargetBuildableObjects != null) {
                return tempTargetBuildableObjects;
            }
            var targetBOs = new List<BuildableObject>();
            foreach (var target in linkTargets) {
                targetBOs.Add(target.targetBuildableObject);
            }
            return targetBOs;
        }

        public bool hasParent() {
            return linkParent != null || tempParentGameObject != null;
        }

        public bool isValid() {
            // This should only be checked in edit mode
            //return hasParent() && linkTargets != null && linkTargets.Count > 0;
            return tempParentGameObject != null && tempTargetGameObjects != null && tempTargetGameObjects.Count > 0;
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
                // Think this shouldn't be a case. Setting parent would only occur during link creation.
            }
        }

        public void removeParentObject() {
            LinkedMovement.Log("LMLink.removeParentObject");

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject);
                
                tempParentGameObject = null;
                tempParentBuildableObject = null;

                // TODO: If any targets present, reset their parent to null
                // Also, stop associated animations
            }
        }

        public void addTargetObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.addTargetObject");

            // TODO: Possibly cache target original parent
            // Actually think this should be fine. If changes are discarded, reset temp targets parent to null
            // then rebuild original linkage.
            tempTargetBuildableObjects.Add(buildableObject);
            tempTargetGameObjects.Add(buildableObject.gameObject);

            LMUtils.SetChunkedMeshEnalbedIfPresent(buildableObject, false);
            LMUtils.AddObjectHighlight(buildableObject, Color.yellow);

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject, buildableObject.gameObject }, LMUtils.AssociatedAnimationEditMode.Stop, true);
            LMUtils.SetTargetParent(tempParentGameObject.transform, buildableObject.transform);
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Start, true);
        }

        public void removeTargetObject(BuildableObject buildableObject, bool skipRemoval = false) {
            LinkedMovement.Log("LMLink.removeTargetObject");

            if (!tempTargetBuildableObjects.Contains(buildableObject)) {
                // Not a target
                return;
            }

            LMUtils.RemoveObjectHighlight(buildableObject);

            if (!skipRemoval) {
                tempTargetBuildableObjects.Remove(buildableObject);
                tempTargetGameObjects.Remove(buildableObject.gameObject);
            }

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Stop, true);
            LMUtils.SetTargetParent(null, buildableObject.transform);
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject, buildableObject.gameObject }, LMUtils.AssociatedAnimationEditMode.Start, true);
        }

        public void discardChanges() {
            LinkedMovement.Log("LMLink.discardChanges");

            stopPicking();

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject);
            }

            foreach (var buildableObject in tempTargetBuildableObjects) {
                LMUtils.RemoveObjectHighlight(buildableObject);
                removeTargetObject(buildableObject, true);
            }

            IsEditing = false;
        }

        public void saveChanges() {
            LinkedMovement.Log("LMLink.saveChanges");

            stopPicking();

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject);
            }

            foreach (var buildableObject in tempTargetBuildableObjects) {
                LMUtils.RemoveObjectHighlight(buildableObject);
                removeTargetObject(buildableObject, true);
            }

            initializeWith(tempParentBuildableObject, tempTargetBuildableObjects);

            IsEditing = false;

            LinkedMovement.GetLMController().addLink(this);

            rebuildLink();
        }

        public void removeLink() {
            LinkedMovement.Log("LMLink.removeLink");

            // TODO
        }

        public void rebuildLink(bool skipRestartAnimations = false) {
            LinkedMovement.Log("LMLink.rebuildLink");

            var parentTransform = linkParent.targetGameObject.transform;
            foreach (var targetLink in linkTargets) {
                LMUtils.SetTargetParent(parentTransform, targetLink.targetGameObject.transform);
            }

            if (!skipRestartAnimations) {
                //LMUtils.EditAssociatedAnimations(new List<GameObject>() { linkParent.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Restart, false);
                LMUtils.EditAssociatedAnimations(new List<GameObject>() { linkParent.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Stop, false);
                LMUtils.EditAssociatedAnimations(new List<GameObject>() { linkParent.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Start, false);
            }
        }

        private void handlePickerAddObjectAsParent(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsParent");

            // TODO: Validate
            // Object not this parent and doesn't have LMLinkParent

            setParentObject(buildableObject);
            stopPicking();
        }

        private void handlePickerAddObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsTarget");

            // TODO: Validate
            // Object not already target and doesn't have LMLinkTarget

            addTargetObject(buildableObject);
        }

        private void handlePickerRemoveObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerRemoveObjectAsTarget");

            // TODO: Validate - maybe not, already validating in removeTargetObject

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

            linkParent.targetBuildableObject.addCustomData(linkParent);

            foreach (var linkTarget in linkTargets) {
                linkTarget.targetBuildableObject.addCustomData(linkTarget);
            }
        }

        private void removeCustomData() {
            LinkedMovement.Log("LMLink.removeCustomData");

            if (linkParent != null && linkParent.targetBuildableObject != null) {
                linkParent.targetBuildableObject.removeCustomData<LMLinkParent>();
            }
            
            if (linkTargets != null && linkTargets.Count > 0) {
                foreach (var target in linkTargets) {
                    if (target.targetBuildableObject != null) {
                        target.targetBuildableObject.removeCustomData<LMLinkTarget>();
                    }
                }
            }
        }
    }
}
