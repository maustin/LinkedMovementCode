using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLink {
        private string _name = string.Empty;
        private string _tempName = null;
        public string name {
            get {
                if (IsEditing) {
                    return _tempName;
                } else {
                    if (linkParent != null) {
                        return linkParent.name;
                    }
                    return _name;
                }
            }
            set {
                if (IsEditing) {
                    _tempName = value;
                } else {
                    if (linkParent != null) {
                        linkParent.name = value;
                    }
                    _name = value;
                }
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
                        LMUtils.AddObjectHighlight(tempParentBuildableObject, HighlightType.LinkParent);
                    }

                    tempTargetGameObjects = new List<GameObject>();
                    tempTargetBuildableObjects = new List<BuildableObject>();
                    foreach (var linkTarget in linkTargets) {
                        tempTargetGameObjects.Add(linkTarget.targetGameObject);
                        tempTargetBuildableObjects.Add(linkTarget.targetBuildableObject);
                        LMUtils.AddObjectHighlight(linkTarget.targetBuildableObject, HighlightType.LinkTarget);
                    }
                    _tempName = _name;
                } else {
                    // clear temps
                    if (tempParentBuildableObject != null && linkParent == null) {
                        LMUtils.RemoveObjectHighlight(tempParentBuildableObject, HighlightType.LinkParent);
                    }
                    if (linkParent != null && linkParent.targetBuildableObject != null && !ForceShowHighlight) {
                        LMUtils.RemoveObjectHighlight(linkParent.targetBuildableObject, HighlightType.LinkParent);
                    }

                    // Here linkTargets are the final target selections
                    var linkTargetBuildableObjects = new HashSet<BuildableObject>();
                    foreach (var linkTarget in linkTargets) {
                        linkTargetBuildableObjects.Add(linkTarget.targetBuildableObject);
                    }

                    // Objects in tempTargets that are not in linkTargets need to clear their highlights
                    foreach (var tempTargetBuildableObject in tempTargetBuildableObjects) {
                        if (!linkTargetBuildableObjects.Contains(tempTargetBuildableObject)) {
                            LMUtils.RemoveObjectHighlight(tempTargetBuildableObject, HighlightType.LinkTarget);
                        }
                    }

                    // Reset highlights on linkTargets
                    foreach (var linkTargetBuildableObject in linkTargetBuildableObjects) {
                        if (ForceShowHighlight) {
                            LMUtils.AddObjectHighlight(linkTargetBuildableObject, HighlightType.LinkTarget);
                        } else {
                            LMUtils.RemoveObjectHighlight(linkTargetBuildableObject, HighlightType.LinkTarget);
                        }
                    }

                    tempParentGameObject = null;
                    tempParentBuildableObject = null;
                    tempTargetGameObjects = null;
                    tempTargetBuildableObjects = null;
                    _tempName = null;
                }
            }
        }

        private bool _forceShowHighlight;
        public bool ForceShowHighlight {
            get => _forceShowHighlight;
            set {
                _forceShowHighlight = value;

                if (_forceShowHighlight) {
                    LMUtils.AddObjectHighlight(linkParent.targetBuildableObject, HighlightType.LinkParent);
                    foreach (var linkTarget in linkTargets) {
                        LMUtils.AddObjectHighlight(linkTarget.targetBuildableObject, HighlightType.LinkTarget);
                    }
                } else {
                    LMUtils.RemoveObjectHighlight(linkParent.targetBuildableObject, HighlightType.LinkParent);
                    foreach (var linkTarget in linkTargets) {
                        LMUtils.RemoveObjectHighlight(linkTarget.targetBuildableObject, HighlightType.LinkTarget);
                    }
                }
            }
        }

        public LMLink() {
            LinkedMovement.Log("LMLink base constructor");
            linkTargets = new List<LMLinkTarget>();
            _name = "New Link";
            _id = LMUtils.GetNewId();
        }

        public LMLink(LMLinkParent linkParent, List<LMLinkTarget> linkTargets) {
            LinkedMovement.Log("LMLink constructor with LinkParent and LinkTargets");

            this.linkParent = linkParent;
            this.linkTargets = linkTargets;

            _name = linkParent.name;
            _id = linkParent.id;
        }

        public LMLink(string name, string id, BuildableObject parentBuildableObject, List<BuildableObject> targetBuildableObjects) {
            LinkedMovement.Log("LMLink constructor with id, name, and buildable objects");
            _name = name;
            _id = id;

            initializeWith(parentBuildableObject, targetBuildableObjects);
        }

        private void initializeWith(BuildableObject parentBuildableObject, List<BuildableObject> targetBuildableObjects) {
            LinkedMovement.Log("LMLink.initializeWith");

            removeCustomData();

            if (linkParent == null || linkParent.targetBuildableObject != parentBuildableObject) {
                var newParent = new LMLinkParent(_name, _id, parentBuildableObject);
                linkParent = newParent;
            } // else linkParent already setup

            // Just recreate all LMLinkTargets
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
            if (IsEditing) {
                return tempParentGameObject != null && tempTargetGameObjects != null && tempTargetGameObjects.Count > 0;
            }
            else {
                return linkParent != null && linkTargets != null && linkTargets.Count > 0;
            }
        }

        public void addObjectsToUpdateMouseColliders(HashSet<BuildableObject> buildableObjectsToUpdate) {
            var parentBuildableObject = getParentBuildableObject();
            if (parentBuildableObject != null) {
                buildableObjectsToUpdate.Add(parentBuildableObject);
            }
            var targetBuildableObjects = getTargetBuildableObjects();
            foreach (var buildableObject in targetBuildableObjects) {
                buildableObjectsToUpdate.Add(buildableObject);
            }
        }

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

            if (IsEditing) {
                tempParentBuildableObject = buildableObject;
                tempParentGameObject = buildableObject.gameObject;

                LMUtils.DeleteChunkedMesh(buildableObject);
                LMUtils.AddObjectHighlight(buildableObject, HighlightType.LinkParent);
            } else {
                // TODO: ?
                // Think this shouldn't be a case. Setting parent would only occur during link creation.
            }
        }

        public void removeParentObject() {
            LinkedMovement.Log("LMLink.removeParentObject");

            if (tempParentBuildableObject != null) {
                LMUtils.RemoveObjectHighlight(tempParentBuildableObject, HighlightType.LinkParent);
                
                tempParentGameObject = null;
                tempParentBuildableObject = null;

                // TODO: Needed?
                // If any targets present, reset their parent to null
                // Also, stop associated animations
            }
        }

        public void addTargetObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.addTargetObject");

            tempTargetBuildableObjects.Add(buildableObject);
            tempTargetGameObjects.Add(buildableObject.gameObject);

            LMUtils.DeleteChunkedMesh(buildableObject);
            LMUtils.AddObjectHighlight(buildableObject, HighlightType.LinkTarget);

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject, buildableObject.gameObject }, LMUtils.AssociatedAnimationEditMode.Stop, true);
            LMUtils.SetTargetParent(tempParentGameObject.transform, buildableObject.transform);
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Start, true);
        }

        // Called when an object is deleted from the park
        public void deleteTargetObject(GameObject gameObject) {
            LinkedMovement.Log("LMLink.deleteTargetObject");

            LMLinkTarget foundLinkTarget = null;
            foreach (var linkTarget in linkTargets) {
                if (linkTarget.targetGameObject == gameObject) {
                    LinkedMovement.Log("Found LinkTarget");
                    foundLinkTarget = linkTarget;
                    LMUtils.SetTargetParent(null, foundLinkTarget.targetBuildableObject.transform);
                    break;
                }
            }

            if (foundLinkTarget != null) {
                linkTargets.Remove(foundLinkTarget);
            } else {
                LinkedMovement.Log("Couldn't find LinkTarget from gameObject");
            }

        }

        // Called via UI action ('X' from UI or right-click deselect)
        public void removeSingleTargetObject(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.removeSingleTargetObject");

            if (!tempTargetBuildableObjects.Contains(buildableObject)) {
                // Not a target
                return;
            }

            LMUtils.RemoveObjectHighlight(buildableObject, HighlightType.LinkTarget);

            tempTargetBuildableObjects.Remove(buildableObject);
            tempTargetGameObjects.Remove(buildableObject.gameObject);

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Stop, true);
            LMUtils.SetTargetParent(null, buildableObject.transform);
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject, buildableObject.gameObject }, LMUtils.AssociatedAnimationEditMode.Start, true);

            LMUtils.UpdateMouseColliders(buildableObject);
        }

        // Called from UI
        public void removeAllTargetObjects() {
            LinkedMovement.Log("LMLink.removeAllTargetObjects");

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Stop, true);

            foreach (var buildableObject in tempTargetBuildableObjects) {
                LMUtils.RemoveObjectHighlight(buildableObject, HighlightType.LinkTarget);
                LMUtils.SetTargetParent(null, buildableObject.transform);
                LMUtils.UpdateMouseColliders(buildableObject);
            }

            // Restart the parent with isEditing true
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { tempParentGameObject }, LMUtils.AssociatedAnimationEditMode.Start, true);

            // Removed targets will restart with isEditing false as they are no longer associated with UI
            LMUtils.EditAssociatedAnimations(tempTargetGameObjects, LMUtils.AssociatedAnimationEditMode.Start, false);

            tempTargetBuildableObjects.Clear();
            tempTargetGameObjects.Clear();
        }

        public void discardChanges() {
            LinkedMovement.Log("LMLink.discardChanges");

            stopPicking();

            // Stop temp parent and targets
            var stopList = new List<GameObject>();
            if (tempParentBuildableObject != null) {
                stopList.Add(tempParentGameObject);
            }
            if (tempTargetGameObjects.Count > 0) {
                stopList.AddRange(tempTargetGameObjects);
            }
            if (stopList.Count > 0) {
                LMUtils.EditAssociatedAnimations(stopList, LMUtils.AssociatedAnimationEditMode.Stop, false);
            }

            foreach (var buildableObject in tempTargetBuildableObjects) {
                //LMUtils.LogComponents(buildableObject);
                LMUtils.SetTargetParent(null, buildableObject.transform);
                LMUtils.UpdateMouseColliders(buildableObject);
            }

            var restartList = new List<GameObject>() { tempParentGameObject };

            // Temp targets that are *not* an original should be added to the restartList
            // This ensures any that had their own animations are rebuilt now without the parent
            if (linkTargets != null && linkTargets.Count > 0 && tempTargetGameObjects.Count > 0) {
                var originalTargetGameObjects = new List<GameObject>();
                foreach (var target in linkTargets) {
                    originalTargetGameObjects.Add(target.targetGameObject);
                }
                
                foreach (var tempTargetGameObject in tempTargetGameObjects) {
                    if (!originalTargetGameObjects.Contains(tempTargetGameObject)) {
                        restartList.Add(tempTargetGameObject);
                    }
                }
            }

            if (linkParent != null) {
                // Only rebuild the link if we were editing (e.g. LinkParent and LinkTargets present)
                rebuildLink();
            }

            IsEditing = false;

            LMUtils.EditAssociatedAnimations(restartList, LMUtils.AssociatedAnimationEditMode.Start, false);
        }

        public void saveChanges() {
            LinkedMovement.Log("LMLink.saveChanges");

            stopPicking();

            initializeWith(tempParentBuildableObject, tempTargetBuildableObjects);

            var newName = _tempName;
            IsEditing = false;
            name = newName;

            LMUtils.EditAssociatedAnimations(new List<GameObject>() { linkParent.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Stop, false);
            LMUtils.EditAssociatedAnimations(new List<GameObject>() { linkParent.targetGameObject }, LMUtils.AssociatedAnimationEditMode.Start, false);

            LinkedMovement.GetLMController().addLink(this);
        }

        public void destroyLink() {
            LinkedMovement.Log($"LMLink.destroyLink name: {name}, id: {id}");

            LMUtils.RemoveObjectHighlight(linkParent.targetBuildableObject, HighlightType.LinkParent);

            foreach (var targetLink in linkTargets) {
                LinkedMovement.Log("Unparent target: " + targetLink.targetGameObject.name);
                //LMUtils.LogComponents(targetLink.targetBuildableObject);
                LMUtils.SetTargetParent(null, targetLink.targetGameObject.transform);
                LMUtils.UpdateMouseColliders(targetLink.targetBuildableObject);

                LMUtils.RemoveObjectHighlight(targetLink.targetBuildableObject, HighlightType.LinkTarget);
            }

            // Remove data
            removeCustomData();

            linkParent = null;
            linkTargets = null;
        }

        public void rebuildLink() {
            LinkedMovement.Log("LMLink.rebuildLink");

            var parentTransform = linkParent.targetGameObject.transform;
            foreach (var targetLink in linkTargets) {
                LMUtils.SetTargetParent(parentTransform, targetLink.targetGameObject.transform);
            }
        }

        private void handlePickerAddObjectAsParent(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsParent");

            // Validate - ensure target is not already a LinkParent
            var gameObject = buildableObject.gameObject;
            var link = LinkedMovement.GetLMController().findLinkByParentGameObject(gameObject);
            if (link != null) {
                //string rejectMessage = $"Selection is already the Parent object of Link '{link.name}'";
                string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_LINK_PARENT_EXISTS, buildableObject.getName(), link.name);
                LinkedMovement.Log(rejectMessage);
                LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }

            setParentObject(buildableObject);
            stopPicking();
        }

        private void handlePickerAddObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerAddObjectAsTarget");
            var targetName = buildableObject.getName();

            // Validate
            // Ensure target is not already selected as parent
            if (getParentBuildableObject() == buildableObject) {
                string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_LINK_TARGET_IS_PARENT, targetName);
                LinkedMovement.Log(rejectMessage);
                LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }
            // Ensure Target is not already selected as target (silent fail)
            if (getTargetBuildableObjects().Contains(buildableObject)) {
                string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_LINK_TARGET_IS_TARGET, targetName);
                LinkedMovement.Log(rejectMessage);
                //LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }
            // Ensure Target is not already child of another link
            var gameObject = buildableObject.gameObject;
            var otherLink = LinkedMovement.GetLMController().findLinkByTargetGameObject(gameObject);
            if (otherLink != null) {
                string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_LINK_TARGET_EXISTS, targetName, otherLink.name);
                LinkedMovement.Log(rejectMessage);
                // TODO: This call needs much cleaner access
                LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                return;
            }
            // Ensure Target is not parent of this link (circular link)
            var parentLink = LinkedMovement.GetLMController().findLinkByParentGameObject(gameObject);
            if (parentLink != null) {
                var parentTargets = parentLink.getTargetGameObjects();
                if (parentTargets.Contains(getParentGameObject())) {
                    string rejectMessage = LMStringSystem.GetText(LMStringKey.SELECT_LINK_CIRCULAR, targetName);
                    LinkedMovement.Log(rejectMessage);
                    // TODO: This call needs much cleaner access
                    LinkedMovement.GetLMController().windowManager.createWindow(UI.WindowManager.WindowType.Error, rejectMessage);
                    return;
                }
            }

            addTargetObject(buildableObject);
        }

        private void handlePickerRemoveObjectAsTarget(BuildableObject buildableObject) {
            LinkedMovement.Log("LMLink.handlePickerRemoveObjectAsTarget");

            removeSingleTargetObject(buildableObject);
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
