// ATTRIB: TransformAnarchy
using LinkedMovement.Animation;
using LinkedMovement.Links;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    // TODO: This needs to be split up
    static class LMUtils {
        public enum AssociatedAnimationEditMode {
            Stop,
            Start,
            Restart,
        }

        private static HashSet<GameObject> AssociatedGameObjects;

        public static void LogComponents(GameObject go) {
            LMLogger.Debug("LMUtils.LogComponents for GameObject: " + go.name);
            Component[] components = go.GetComponents<Component>();
            foreach (var component in components) {
                LMLogger.Debug($"Component type: {component.GetType().Name}, name: {component.name}");
            }

            if (go.transform.parent != null) {
                LMLogger.Debug("Has parent");
                LogComponents(go.transform.parent.gameObject);
            }
        }

        public static void LogComponents(BuildableObject bo) {
            LMLogger.Debug("LMUtils.LogComponents for BuildableObject: " + bo.getName());
            Component[] components = bo.gameObject.GetComponents<Component>();
            foreach (var component in components) {
                LMLogger.Debug($"Component type: {component.GetType().Name}, name: {component.name}");
            }
        }

        public static string GetNewId() {
            return Guid.NewGuid().ToString();
        }

        public static void DeleteChunkedMesh(BuildableObject bo) {
            if (bo == null) return;

            LMLogger.Debug("LMUtils.DeleteChunkedMesh for " + bo.name);
            var chunkedMesh = bo.GetComponent<ChunkedMesh>();
            if (chunkedMesh != null) {
                UnityEngine.Object.Destroy(chunkedMesh);
                return;
            }

            // Object doesn't have ChunkedMesh. Check if it has LODGroup.
            // If so, the ChunkedMesh components will be in 2nd-level children.
            var lodGroup = bo.GetComponent<LODGroup>();
            if (lodGroup == null) return;

            // Object has LODGroup, check children
            foreach (Transform child in bo.transform) {
                var childGO = child.gameObject;
                var childGOName = childGO.name;
                if (childGOName.Contains("LOD") && child.childCount > 0) {
                    // Child is an LOD object
                    var childChild = child.GetChild(0);
                    if (childChild == null) return;
                    // LOD object has a child (this is what should have the ChunkedMesh)
                    var childChildGO = childChild.gameObject;
                    chunkedMesh = childChildGO.GetComponent<ChunkedMesh>();
                    if (chunkedMesh != null) {
                        UnityEngine.Object.Destroy(chunkedMesh);
                    }
                }
            }
        }

        // TODO: Eliminate this method (prefer delete ChunkedMesh instead)
        // Built-in objects have a ChunkedMesh component. This component can prevent visual updates
        // while we're modifying animations that affect their GameObject.
        // Disable when creating and modifying. Re-enable when finished.
        public static void SetChunkedMeshEnalbedIfPresent(BuildableObject bo, bool enalbed) {
            LMLogger.Debug($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} set to {enalbed.ToString()}");
            //LogComponents(bo);
            
            var chunker = bo.GetComponent<ChunkedMesh>();
            if (chunker != null) {
                //LinkedMovement.Log($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} set to {enalbed.ToString()}");
                LMLogger.Debug("Has ChunkedMesh");
                chunker.enabled = enalbed;
            } else {
                //LinkedMovement.Log($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} does NOT have ChunkedMesh component");
                LMLogger.Debug("NO ChunkedMesh");
            }
        }

        public static bool IsGeneratedOrigin(BuildableObject bo) {
            return bo != null && bo.getName().Contains(LinkedMovement.HELPER_OBJECT_NAME);
        }

        public static void BuildLinksAndAnimationsFromBlueprint(List<BuildableObject> builtObjectInstances, Vector3 forward) {
            LMLogger.Debug("LMUtils.BuildLinksAndAnimationsFromBlueprint");

            var createdLinkParents = new List<LMLinkParent>();
            var createdLinkTargets = new List<LMLinkTarget>();
            var createdAnimations = new List<LMAnimation>();
            foreach (var buildableObject in builtObjectInstances) {
                TryToBuildLinkParentFromBlueprintObject(buildableObject, createdLinkParents);
                TryToBuildLinkTargetFromBlueprintObject(buildableObject, createdLinkTargets);
                TryToBuildAnimationFromBlueprintObject(buildableObject, forward, createdAnimations);

                DeleteChunkedMesh(buildableObject);
            }

            // Generate new Link Ids
            var newLinkIds = new Dictionary<string, string>();
            foreach (var linkParent in createdLinkParents) {
                var newId = GetNewId();
                newLinkIds.Add(linkParent.id, newId);
                linkParent.id = newId;
            }
            foreach (var linkTarget in createdLinkTargets) {
                var newId = newLinkIds[linkTarget.id];
                linkTarget.id = newId;
            }
            // TODO: Delete LMLinkParent and LMLinkTarget data from orphaned

            LMLogger.Debug($"Try to build {createdLinkParents.Count} links");
            LinkedMovement.GetLMController().setupLinks(createdLinkParents, createdLinkTargets);

            LMLogger.Debug($"Built {createdAnimations.Count} animations from blueprint, starting");

            foreach (var animation in createdAnimations) {
                animation.setup();
            }
        }

        private static void TryToBuildLinkParentFromBlueprintObject(BuildableObject buildableObject, List<LMLinkParent> createdLinkParents) {
            LMLinkParent linkParent = GetLinkParentFromSerializedMonoBehaviour(buildableObject);
            if (linkParent != null) {
                linkParent.setTarget(buildableObject.gameObject);
                createdLinkParents.Add(linkParent);
            }
        }

        private static void TryToBuildLinkTargetFromBlueprintObject(BuildableObject buildableObject, List<LMLinkTarget> createdLinkTargets) {
            LMLinkTarget linkTarget = GetLinkTargetFromSerializedMonoBehaviour(buildableObject);
            if (linkTarget != null) {
                linkTarget.setTarget(buildableObject.gameObject);
                createdLinkTargets.Add(linkTarget);
            }
        }

        private static void TryToBuildAnimationFromBlueprintObject(BuildableObject buildableObject, Vector3 forward, List<LMAnimation> createdAnimations) {
            LMAnimationParams animationParams = GetAnimationParamsFromSerializedMonoBehaviour(buildableObject);
            if (animationParams == null) {
                return;
            }

            LMLogger.Debug("LMUtils.TryToBuildAnimationFromBlueprintObject for " + buildableObject.getName());

            LMAnimation animation = new LMAnimation(animationParams, buildableObject.gameObject, true);
            animation.generateNewId();
            animationParams.forward = Quaternion.LookRotation(forward);

            LinkedMovement.GetLMController().addAnimation(animation);
            createdAnimations.Add(animation);
        }

        public static void LogDetails(BuildableObject bo) {
            LMLogger.Debug($"BO name: {bo.name}, BO getName: {bo.getName()}, GO name: {bo.gameObject.name}");
            LMLogger.Debug($"POS: {bo.transform.position.ToString()}, lPOS: {bo.transform.localPosition.ToString()}");
            LMLogger.Debug($"ROT: {bo.transform.eulerAngles.ToString()}, lRot: {bo.transform.localEulerAngles.ToString()}");
        }

        public static void SetTargetParent(Transform parentTransform, Transform targetObject) {
            LMLogger.Debug("LMUtils.SetTargetParent");
            var oldParent = targetObject.parent;
            var oldParentName = (oldParent != null) ? oldParent.name : "null";
            LMLogger.Debug("OLD PARENT: " + oldParentName);
            if (parentTransform == null) {
                LMLogger.Debug($"parentTransform is null, clearing target {targetObject.name} parent");
                targetObject.SetParent(null);
                return;
            }

            var baseTransform = parentTransform;
            if (targetObject.IsChildOf(baseTransform)) {
                LMLogger.Debug("ALREADY A CHILD!");
            } else {
                LMLogger.Debug("Making child");
                targetObject.SetParent(baseTransform);
                LogDetails(GetBuildableObjectFromGameObject(targetObject.gameObject));
            }
        }

        public static void UpdateMouseColliders(BuildableObject bo) {
            if (bo.mouseColliders != null) {
                foreach (MouseCollider mouseCollider in bo.mouseColliders)
                    mouseCollider.updatePosition();
            }
        }

        public static string GetGameObjectBuildableName(GameObject go) {
            var buildableObject = GetBuildableObjectFromGameObject(go);
            if (buildableObject != null)
                return buildableObject.getName();
            return go.name;
        }

        public static BuildableObject GetBuildableObjectFromGameObject(GameObject go) {
            var buildableObject = go.GetComponent<BuildableObject>();
            return buildableObject;
        }

        public static BuildableObject GetBuildableObjectFromBoxSelectGameObject(GameObject gameObject) {
            LMLogger.Debug("LMUtils.GetBuildableObjectFromBoxSelectGameObject for: " + gameObject.name);
            var buildableObject = GetBuildableObjectFromGameObject(gameObject);
            if (buildableObject != null) {
                LMLogger.Debug("Has BuildableObject, return it");
                return buildableObject;
            }

            // Here we'll check if the gameObject is part of an LODGroup.
            // In this case we'll need to traverse up two levels.
            // The first parent is the LOD object with just a transform.
            // The second parent will have the LODGroup component.
            // TODO: I don't like blindly reaching up two levels. Is there a better way?

            var lodParentGameObject = gameObject.transform?.parent?.parent?.gameObject;
            if (lodParentGameObject == null) {
                LMLogger.Debug("No LOD parent, return null");
                return null;
            }

            //LogComponents(lodParentGameObject);

            var lodGroup = lodParentGameObject.GetComponent<LODGroup>();
            if (lodGroup == null) {
                LMLogger.Debug("No LODGroup, return null");
                return null;
            }

            buildableObject = lodParentGameObject.GetComponent<BuildableObject>();
            if (buildableObject != null) {
                LMLogger.Debug("LOD parent HAS BuildableObject");
            } else {
                LMLogger.Debug("LOD parent HAS NO BuildableObject");
            }
            return buildableObject;
        }

        public static LMAnimationParams GetAnimationParamsFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            LMAnimationParams animationParams;
            smb.tryGetCustomData(out animationParams);
            return animationParams;
        }

        public static LMLinkParent GetLinkParentFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            LMLinkParent linkParent;
            smb.tryGetCustomData(out linkParent);
            return linkParent;
        }

        public static LMLinkTarget GetLinkTargetFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            LMLinkTarget linkTarget;
            smb.tryGetCustomData(out linkTarget);
            return linkTarget;
        }

        public static List<BuildableObject> GetBuildableObjectsFromGameObjects(List<GameObject> gameObjects) {
            List<BuildableObject> buildableObjects = new List<BuildableObject>();
            foreach (GameObject go in gameObjects) {
                buildableObjects.Add(GetBuildableObjectFromGameObject(go));
            }
            return buildableObjects;
        }

        private static void PrepAssociatedGameObjects() {
            LMLogger.Debug("LMUtils.PrepAssociatedGameObjects");
            if (AssociatedGameObjects != null) {
                // SHOULD NEVER GET HERE
                //throw new Exception("LMUtils.PrepAssociatedGameObjects ALREADY RUNNING!");
                // For now, just handle gracefully
                LMLogger.Error("LMUtils AssociatedGameObjects was not reset!");
                CleanupAssociateGameObjects();
            }
            AssociatedGameObjects = new HashSet<GameObject>();
        }

        private static void CleanupAssociateGameObjects() {
            LMLogger.Debug("LMUtils.CleanupAssociatedGameObjects");
            AssociatedGameObjects.Clear();
            AssociatedGameObjects = null;
        }

        // TODO: "editMode" and "isEditing" names are ambiguous here
        // Only Start mode uses isEditing
        public static void EditAssociatedAnimations(List<GameObject> gameObjects, AssociatedAnimationEditMode editMode, bool isEditing) {
            LMLogger.Debug($"LMUtils.EditAssociatedAnimations mode {editMode.ToString()} with {gameObjects.Count} gameObjects, isEditing: {isEditing.ToString()}");
            PrepAssociatedGameObjects();
            foreach (GameObject go in gameObjects) {
                if (go == null) continue;
                EditAssociatedAnimation(go, editMode, isEditing);
            }
            CleanupAssociateGameObjects();
        }

        private static void EditAssociatedAnimation(GameObject gameObject, AssociatedAnimationEditMode editMode, bool isEditing) {
            LMLogger.Debug("LMUtils.EditAssociatedAnimation for " + gameObject.name);

            var gameObjectHasBeenVisited = AssociatedGameObjects.Contains(gameObject);
            if (gameObjectHasBeenVisited) {
                LMLogger.Debug("Already visited GameObject " + gameObject.name);
                return;
            }

            AssociatedGameObjects.Add(gameObject);

            switch (editMode) {
                case AssociatedAnimationEditMode.Stop: {
                        StopAssociatedAnimation(gameObject);
                        break;
                    }
                case AssociatedAnimationEditMode.Start: {
                        StartAssociatedAnimation(gameObject, isEditing);
                        break;
                    }
                case AssociatedAnimationEditMode.Restart: {
                        RestartAssociatedAnimation(gameObject);
                        break;
                    }
            }

            // TODO: Paths have unexpected parent/child relationships. Need to exclude or we reach into all tiles in the park.

            LMLogger.Debug("Check children for " + gameObject.name);
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                var childTransform = gameObject.transform.GetChild(i);
                if (childTransform != null) {
                    var childGO = childTransform.gameObject;
                    if (childGO != null) {
                        LMLogger.Debug($"Try edit associated child for {gameObject.name}, index {i.ToString()}, name {childGO.name}");
                        EditAssociatedAnimation(childGO, editMode, isEditing);
                    }
                }
            }

            LMLogger.Debug("Check parent for " + gameObject.name);
            if (gameObject.transform.parent != null && gameObject.transform.parent.gameObject != null) {
                var parentGO = gameObject.transform.parent.gameObject;
                LMLogger.Debug($"Try edit associated parent for {gameObject.name}, parent: {parentGO.name}");
                EditAssociatedAnimation(parentGO, editMode, isEditing);
            } else {
                LMLogger.Debug("No parent");
            }
        }

        private static void StopAssociatedAnimation(GameObject gameObject) {
            LMLogger.Debug("LMUtils.StopAssociatedAnimation for " + gameObject.name);
            
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LMLogger.Debug("Found Animation");
                animation.stopSequence();
            } else {
                LMLogger.Debug("No Animation found");
            }
        }

        private static void StartAssociatedAnimation(GameObject gameObject, bool isEditing) {
            LMLogger.Debug("LMUtils.StartAssociatedAnimation for " + gameObject.name);

            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LMLogger.Debug("Found Animation");
                animation.getAnimationParams().setStartingValues(gameObject.transform);

                animation.buildSequence(isEditing);
            } else {
                LMLogger.Debug("No Animation found");
            }
        }

        private static void RestartAssociatedAnimation(GameObject gameObject) {
            LMLogger.Debug("LMUtils.RestartAssociatedAnimation for " + gameObject.name);

            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LMLogger.Debug("Found Animation");
                if (animation.sequence.isAlive) {
                    LMLogger.Debug("Reset sequence progress");
                    animation.sequence.progress = 0f;
                } else {
                    LMLogger.Debug("Sequence not alive");
                }
            } else {
                LMLogger.Debug("No Animation found");
            }
        }

        public static void ResetTransformLocals(Transform transform, Vector3 localPosition, Vector3 localRotation, Vector3 localScale) {
            LMLogger.Debug("LMUtils.ResetTransformLocals for: " + transform.gameObject.name);
            
            LMLogger.Debug($"FROM pos: {transform.position.ToString()}, lPos: {transform.localPosition.ToString()}, rot: {transform.eulerAngles.ToString()}, lRot: {transform.localEulerAngles.ToString()}, scale: {transform.localScale.ToString()}");
            LMLogger.Debug($"TO lPos: {localPosition.ToString()}, lRot: {localRotation.ToString()}, scale: {localScale.ToString()}");

            transform.localPosition = localPosition;
            transform.localEulerAngles = localRotation;
            transform.localScale = localScale;

            LMLogger.Debug($"DONE pos: {transform.position.ToString()}, lPos: {transform.localPosition.ToString()}, rot: {transform.eulerAngles.ToString()}, lRot: {transform.localEulerAngles.ToString()}, scale: {transform.localScale.ToString()}");
        }

        private static Ease ParseStringToEase(string ease) {
            Ease parsedEase;

            if (Enum.TryParse(ease, out parsedEase)) {
                // OK
            } else {
                // Couldn't parse, use default
                LMLogger.Debug($"ParseStringToEase couldn't parse '{ease}', using default");
                parsedEase = Ease.InOutQuad;
            }

            return parsedEase;
        }

        private static void GetParentRot(Transform transform, ref Vector3 parentRot) {
            var parent = transform.parent;
            if (parent != null) {
                parentRot += parent.localEulerAngles;
                GetParentRot(parent, ref parentRot);
            }
        }

        private static Vector3 GetCumulativeParentLocalRotation(Transform startingTransform, Vector3 cumulativeValue) {
            LMLogger.Debug("GetCumulativeParentLocalRotation value: " + cumulativeValue.ToString());
            var parentTransform = startingTransform.parent;
            if (parentTransform == null) {
                return cumulativeValue;
            }

            LMLogger.Debug("Adding: " + parentTransform.localEulerAngles.ToString());
            cumulativeValue += parentTransform.localEulerAngles;

            return GetCumulativeParentLocalRotation(parentTransform, cumulativeValue);
        }

        private static bool StepHasColorChange(LMAnimationStep animationStep, List<Color> lastTargetColors) {
            var targetColors = animationStep.targetColors;
            if (targetColors == null) return false;

            for (var i = 0; i < targetColors.Count; i++) {
                var targetColor = targetColors[i];
                var lastColor = lastTargetColors[i];
                if (targetColor != lastColor) return true;
            }
            return false;
        }

        private static void BuildAnimationStep(Transform transform, Sequence sequence, LMAnimationParams animationParams, LMAnimationStep animationStep, ref Vector3 lastLocalRotationTarget, ref List<Color> lastTargetColors) {
            LMLogger.Debug($"LMUtils.BuildAnimationStep {animationStep.name} for sequence {animationParams.name}");
            LMLogger.Debug(animationStep.ToString());

            Ease ease = ParseStringToEase(animationStep.ease);
            bool hasPositionChange = !animationStep.targetPosition.Equals(Vector3.zero);
            bool hasRotationChange = !animationStep.targetRotation.Equals(Vector3.zero);
            bool hasScaleChange = !animationStep.targetScale.Equals(Vector3.zero);
            bool hasColorChange = StepHasColorChange(animationStep, lastTargetColors);

            if (hasPositionChange || hasRotationChange || hasScaleChange || hasColorChange) {
                LMLogger.Debug("Has change");
                if (hasPositionChange) {
                    Vector3 positionTarget = animationStep.targetPosition;
                    LMLogger.Debug("Position target: " + positionTarget.ToString());
                    
                    Vector3 forwardEuler = animationParams.forward.eulerAngles;
                    LMLogger.Debug("FORWARD euler: " + forwardEuler.ToString());
                    
                    Vector3 parentLocalRotationOffset = GetCumulativeParentLocalRotation(transform, Vector3.zero);
                    LMLogger.Debug("parenLocalRotationOffset: " + parentLocalRotationOffset.ToString());
                    
                    Vector3 combinedOffset = forwardEuler - parentLocalRotationOffset;
                    LMLogger.Debug("combinedOffset: " + combinedOffset.ToString());
                    
                    Vector3 newPositionTarget = Quaternion.Euler(combinedOffset) * positionTarget;

                    positionTarget = newPositionTarget;
                    LMLogger.Debug("Final positionTarget: " + positionTarget.ToString());

                    sequence.Group(Tween.LocalPositionAdditive(transform, positionTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                    
                    ////    sequence.Group(Tween.LocalPositionAdditive(transform, rotatedPositionTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay)
                    ////    .OnUpdate(target: transform, (target, tween) => {
                    ////        LinkedMovement.Log($"os1 Update pos: {target.position.ToString()}, lPos: {target.localPosition.ToString()}, rot: {target.eulerAngles.ToString()}, lRot: {target.localEulerAngles.ToString()}");
                    ////    })
                    ////    );
                }
                if (hasRotationChange) {
                    // Can't be additive because rotation on multiple axes will cause object to reset to incorrect rotation
                    var newRotationTarget = lastLocalRotationTarget + animationStep.targetRotation;
                    LMLogger.Debug($"Rotate target: {animationStep.targetRotation.ToString()}, newTarget: {newRotationTarget.ToString()}");
                    sequence.Group(Tween.LocalEulerAngles(transform, lastLocalRotationTarget, newRotationTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                    lastLocalRotationTarget = newRotationTarget;
                }
                if (hasScaleChange) {
                    // TODO: Does this also need last update fix?
                    var newScaleTarget = new Vector3(animationStep.targetScale.x * animationParams.startingLocalScale.x, animationStep.targetScale.y * animationParams.startingLocalScale.y, animationStep.targetScale.z * animationParams.startingLocalScale.z);
                    sequence.Group(Tween.ScaleAdditive(transform, newScaleTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                }
                // TODO: Split color Tween creation out to its own function so the update lamba has less overhead
                if (hasColorChange) {
                    var customColorsComponent = transform.gameObject.GetComponent<CustomColors>();
                    var targetColors = animationStep.targetColors;
                    for (int i = 0; i < targetColors.Count; i++) {
                        var colorIndex = i;
                        var startingColor = lastTargetColors[colorIndex];
                        var targetColor = targetColors[colorIndex];

                        // Only animation changed colors
                        if (targetColor != startingColor) {
                            sequence.Group(
                            Tween.Custom(startingColor, targetColor, animationStep.duration, newValue => {
                                customColorsComponent.setColor(newValue, colorIndex);
                            }, ease, default, default, animationStep.startDelay, animationStep.endDelay)
                        );
                        }
                    }
                    lastTargetColors = animationStep.targetColors;
                }
                // Add ChainDelay to "close" the current Sequence Group
                sequence.ChainDelay(0f);
            } else {
                LMLogger.Debug("No change");
                sequence.ChainDelay(animationStep.startDelay);
                sequence.ChainDelay(animationStep.duration);
                sequence.ChainDelay(animationStep.endDelay);
            }

        }

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LMLogger.Debug("LMUtils.BuildAnimationSequence name: " + animationParams.name);

            int loops = -1;
            float startingDelay = 0f;

            bool isTriggered = false;
            if (!isEditing) {
                if (animationParams.isTriggerable) {
                    // Only play once for triggers
                    loops = 0;
                    isTriggered = true;
                } else {
                    if (animationParams.initialStartDelayMin > 0f || animationParams.initialStartDelayMax > 0f) {
                        startingDelay = UnityEngine.Random.Range(animationParams.initialStartDelayMin, animationParams.initialStartDelayMax);
                    }
                }
            }

            Sequence sequence = Sequence.Create(cycles: loops, cycleMode: CycleMode.Restart);

            var lastLocalRotationTarget = transform.localEulerAngles;
            LMLogger.Debug("transform.localEulerAngels: " + lastLocalRotationTarget.ToString());

            var lastTargetColors = animationParams.startingCustomColors;

            foreach (var animationStep in animationParams.animationSteps) {
                BuildAnimationStep(transform, sequence, animationParams, animationStep, ref lastLocalRotationTarget, ref lastTargetColors);
            }

            if (!isEditing) {
                if (startingDelay > 0f) {
                    sequence.isPaused = true;
                    Tween.Delay(startingDelay, () => sequence.isPaused = false);
                }
            }

            if (isTriggered) {
                sequence.ResetBeforeComplete();
            }

            return sequence;
        }
        
        public static void AddObjectHighlight(BuildableObject buildableObject, HighlightType highlightType) {
            if (buildableObject == null) return;
            LMLogger.Debug($"LMUtils.AddObjectHighlight to: {buildableObject.getName()}, type: {highlightType.ToString()}");
            var highlightComponent = buildableObject.gameObject.GetComponent<LMHighlightComponent>();
            if (highlightComponent == null) {
                LMLogger.Debug("LMHighlightComponent doesn't exist, creating");
                highlightComponent = buildableObject.gameObject.AddComponent<LMHighlightComponent>();
            }
            highlightComponent.addHighlightFlag(highlightType);
        }

        public static void RemoveObjectHighlight(BuildableObject buildableObject, HighlightType highlightType) {
            if (buildableObject == null) return;
            LMLogger.Debug($"LMUtils.RemoveObjectHighlight from: {buildableObject.getName()}, type: {highlightType.ToString()}");
            var highlightComponent = buildableObject.gameObject.GetComponent<LMHighlightComponent>();
            if (highlightComponent == null) {
                LMLogger.Debug("LMHighlightComponent doesn't exist");
            } else {
                highlightComponent.removeHighlightFlag(highlightType);
                if (highlightComponent.hasNoHighlights()) {
                    LMLogger.Debug("Destroy LMHighlightComponent from object");
                    GameObject.Destroy(highlightComponent);
                }
            }
        }

        public static float GetSequenceDuration(LMAnimationParams animationParams) {
            var duration = 0f;
            foreach (var step in animationParams.animationSteps) {
                duration += step.startDelay;
                duration += step.duration;
                duration += step.endDelay;
            }
            return duration;
        }

        public static void UpdateGameMouseMode(bool mouseToolActive) {
            // Mouse tool changes can happen before the park has fully loaded. Skip updates in this case.
            if (!LinkedMovement.HasLMController()) return;

            LMLogger.Debug("LMUtils.UpdateGameMouseMode: " + mouseToolActive);
            LinkedMovement.GetLMController().setMouseToolActive(mouseToolActive);
        }

        public static Color[] GetCustomColors(GameObject gameObject) {
            var customColorsComponent = gameObject.GetComponent<CustomColors>();
            if (customColorsComponent != null) {
                return customColorsComponent.getColors();
            }
            return null;
        }

    }
}
