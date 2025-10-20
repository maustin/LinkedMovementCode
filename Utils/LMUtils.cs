// ATTRIB: TransformAnarchy
using LinkedMovement.Animation;
using LinkedMovement.Links;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LinkedMovement.Utils {
    // TODO: Lots of UI stuff. Consider splitting UI stuff out to, like, UI.Utils
    static class LMUtils {
        public enum AssociatedAnimationEditMode {
            Stop,
            Start,
            Restart,
        }

        private static Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle> HighlightHandles;
        private static HashSet<GameObject> AssociatedGameObjects;

        public static void LogComponents(BuildableObject bo) {
            LinkedMovement.Log("LMUtils.LogComponents for " + bo.getName());
            Component[] components = bo.gameObject.GetComponents<Component>();
            foreach (var component in components) {
                LinkedMovement.Log($"Component type: {component.GetType().Name}, name: {component.name}");
            }
        }

        // Built-in objects have a ChunkedMesh component. This component can prevent visual updates
        // while we're modifying animations that affect their GameObject.
        // Disable when creating and modifying. Re-enable when finished.
        public static void SetChunkedMeshEnalbedIfPresent(BuildableObject bo, bool enalbed) {
            LinkedMovement.Log($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} set to {enalbed.ToString()}");
            //LogComponents(bo);
            
            var chunker = bo.GetComponent<ChunkedMesh>();
            if (chunker != null) {
                //LinkedMovement.Log($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} set to {enalbed.ToString()}");
                LinkedMovement.Log("Has ChunkedMesh");
                chunker.enabled = enalbed;
            } else {
                //LinkedMovement.Log($"LMUtils.SetChunkedMeshEnalbedIfPresent for {bo.getName()} does NOT have ChunkedMesh component");
                LinkedMovement.Log("NO ChunkedMesh");
            }
        }

        public static bool IsGeneratedOrigin(BuildableObject bo) {
            //return bo != null && bo.getName().Contains("LMOriginBase");
            return bo != null && bo.getName().Contains(LinkedMovement.HELPER_OBJECT_NAME);
        }

        public static List<SerializedMonoBehaviour> FindPairTargetSOs(PairBase pairBase) {
            LinkedMovement.Log("FindPairTargetSOs with pairId: " + pairBase.pairId);
            var targets = new List<SerializedMonoBehaviour>();
            var sos = GameController.Instance.getSerializedObjects();
            foreach (var so in sos) {
                PairTarget pairTarget = GetPairTargetFromSerializedMonoBehaviour(so);
                if (pairTarget != null) {
                    if (pairTarget.pairId == pairBase.pairId) {
                        targets.Add(so);
                    }
                }
            }
            LinkedMovement.Log($"Found {targets.Count.ToString()} targets");
            return targets;
        }

        // Currently only used when building animated Blueprints
        public static void BuildingBlueprintTryToBuildPairingFromBuiltObjects(List<BuildableObject> builtObjectInstances, Vector3 forward) {
            LinkedMovement.Log("LMUtils.BuildingBlueprintTryToBuildPairingFromBuiltObjects");
            var createdPairings = new List<Pairing>();
            foreach (var buildableObject in builtObjectInstances) {
                BuildingBlueprintTryToBuildPairingFromBuildableObject(buildableObject, builtObjectInstances, forward, ref createdPairings);
            }
            LinkedMovement.Log($"Built {createdPairings.Count} pairings, now creating sequences");
            var sortedPairings = SortPairings(createdPairings);
            foreach (var pairing in sortedPairings) {
                pairing.createSequence();
            }
        }

        public static void BuildLinksAndAnimationsFromBlueprint(List<BuildableObject> builtObjectInstances, Vector3 forward) {
            LinkedMovement.Log("LMUtils.BuildLinksAndAnimationsFromBlueprint");

            var createdLinkParents = new List<LMLinkParent>();
            var createdLinkTargets = new List<LMLinkTarget>();
            var createdAnimations = new List<LMAnimation>();
            foreach (var buildableObject in builtObjectInstances) {
                TryToBuildLinkParentFromBlueprintObject(buildableObject, createdLinkParents);
                TryToBuildLinkTargetFromBlueprintObject(buildableObject, createdLinkTargets);
                TryToBuildAnimationFromBlueprintObject(buildableObject, forward, createdAnimations);
            }

            // Generate new Link Ids
            var newLinkIds = new Dictionary<string, string>();
            foreach (var linkParent in createdLinkParents) {
                var newId = Guid.NewGuid().ToString();
                newLinkIds.Add(linkParent.id, newId);
                linkParent.id = newId;
            }
            foreach (var linkTarget in createdLinkTargets) {
                var newId = newLinkIds[linkTarget.id];
                linkTarget.id = newId;
            }
            // TODO: Delete LMLinkParent and LMLinkTarget data from orphaned

            LinkedMovement.Log($"Try to build {createdLinkParents.Count} links");
            LinkedMovement.GetLMController().setupLinks(createdLinkParents, createdLinkTargets);

            LinkedMovement.Log($"Built {createdAnimations.Count} animations from blueprint, starting");

            foreach (var animation in createdAnimations) {
                animation.setup();
            }
        }

        private static void BuildingBlueprintTryToBuildPairingFromBuildableObject(BuildableObject possibleOriginBO, List<BuildableObject> builtObjectInstances, Vector3 forward, ref List<Pairing> createdPairings) {
            PairBase pairBase = GetPairBaseFromSerializedMonoBehaviour(possibleOriginBO);
            if (pairBase == null) return;

            BuildableObject originObject = possibleOriginBO;
            LinkedMovement.Log("TryToBuildPairingFromBuildableObject for " + originObject.getName());

            List<BuildableObject> targets = new List<BuildableObject>();
            List<PairTarget> pairTargets = new List<PairTarget>();

            foreach (var bo in builtObjectInstances) {
                var pairTarget = GetPairTargetFromSerializedMonoBehaviour(bo);
                if (pairTarget != null && pairTarget.pairId == pairBase.pairId) {
                    targets.Add(bo);
                    pairTargets.Add(pairTarget);
                }
            }

            LinkedMovement.Log($"Found {targets.Count} targets");

            if (targets.Count > 0) {
                LinkedMovement.Log("Create Pairing " + pairBase.pairName);
                pairBase.animParams.setStartingValues(originObject.transform);
                pairBase.animParams.forward = Quaternion.LookRotation(forward);

                // create new pairing ID so we don't collide with existing pairings
                var newPairingId = Guid.NewGuid().ToString();
                pairBase.pairId = newPairingId;
                foreach (var pairTarget in pairTargets) {
                    pairTarget.pairId = newPairingId;
                }
                var targetGameObjects = targets.Select(t => t.gameObject).ToList();

                var pairing = new Pairing(originObject.gameObject, targetGameObjects, newPairingId, pairBase.pairName);
                pairing.connect(false);
                createdPairings.Add(pairing);
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

            LinkedMovement.Log("LMUtils.TryToBuildAnimationFromBlueprintObject for " + buildableObject.getName());

            LMAnimation animation = new LMAnimation(animationParams, buildableObject.gameObject, true);
            animation.generateNewId();
            animationParams.forward = Quaternion.LookRotation(forward);

            LinkedMovement.GetLMController().addAnimation(animation);
            createdAnimations.Add(animation);
        }

        public static void LogDetails(BuildableObject bo) {
            LinkedMovement.Log($"BO name: {bo.name}, BO getName: {bo.getName()}, GO name: {bo.gameObject.name}");
            LinkedMovement.Log($"POS: {bo.transform.position.ToString()}, lPOS: {bo.transform.localPosition.ToString()}");
            LinkedMovement.Log($"ROT: {bo.transform.eulerAngles.ToString()}, lRot: {bo.transform.localEulerAngles.ToString()}");
        }

        public static void SetTargetParent(Transform parentTransform, Transform targetObject) {
            LinkedMovement.Log("LMUtils.SetTargetParent");
            var oldParent = targetObject.parent;
            var oldParentName = (oldParent != null) ? oldParent.name : "null";
            LinkedMovement.Log("OLD PARENT: " + oldParentName);
            if (parentTransform == null) {
                LinkedMovement.Log($"parentTransform is null, clearing target {targetObject.name} parent");
                targetObject.SetParent(null);
                return;
            }

            var baseTransform = parentTransform;
            if (targetObject.IsChildOf(baseTransform)) {
                LinkedMovement.Log("ALREADY A CHILD!");
            } else {
                LinkedMovement.Log("Making child");
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

        public static PairBase GetPairBaseFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairBase pairBase;
            smb.tryGetCustomData(out pairBase);
            return pairBase;
        }

        public static PairTarget GetPairTargetFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairTarget pairTarget;
            smb.tryGetCustomData(out pairTarget);
            return pairTarget;
        }

        public static List<BuildableObject> GetBuildableObjectsFromGameObjects(List<GameObject> gameObjects) {
            List<BuildableObject> buildableObjects = new List<BuildableObject>();
            foreach (GameObject go in gameObjects) {
                buildableObjects.Add(GetBuildableObjectFromGameObject(go));
            }
            return buildableObjects;
        }

        private static void PrepAssociatedGameObjects() {
            LinkedMovement.Log("LMUtils.PrepAssociatedGameObjects");
            if (AssociatedGameObjects != null) {
                // SHOULD NEVER GET HERE
                //throw new Exception("LMUtils.PrepAssociatedGameObjects ALREADY RUNNING!");
                // For now, just handle gracefully
                LinkedMovement.Log("ERROR: LMUtils AssociatedGameObjects was not reset!");
                CleanupAssociateGameObjects();
            }
            AssociatedGameObjects = new HashSet<GameObject>();
        }

        private static void CleanupAssociateGameObjects() {
            LinkedMovement.Log("LMUtils.CleanupAssociatedGameObjects");
            AssociatedGameObjects.Clear();
            AssociatedGameObjects = null;
        }

        // TODO: "editMode" and "isEditing" names are ambiguous here
        // Only Start mode uses isEditing
        public static void EditAssociatedAnimations(List<GameObject> gameObjects, AssociatedAnimationEditMode editMode, bool isEditing) {
            LinkedMovement.Log($"LMUtils.EditAssociatedAnimations mode {editMode.ToString()} with {gameObjects.Count} gameObjects, isEditing: {isEditing.ToString()}");
            PrepAssociatedGameObjects();
            foreach (GameObject go in gameObjects) {
                if (go == null) continue;
                EditAssociatedAnimation(go, editMode, isEditing);
            }
            CleanupAssociateGameObjects();
        }

        private static void EditAssociatedAnimation(GameObject gameObject, AssociatedAnimationEditMode editMode, bool isEditing) {
            LinkedMovement.Log("LMUtils.EditAssociatedAnimation for " + gameObject.name);

            var gameObjectHasBeenVisited = AssociatedGameObjects.Contains(gameObject);
            if (gameObjectHasBeenVisited) {
                LinkedMovement.Log("Already visited GameObject " + gameObject.name);
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

            LinkedMovement.Log("Check children for " + gameObject.name);
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                var childTransform = gameObject.transform.GetChild(i);
                if (childTransform != null) {
                    var childGO = childTransform.gameObject;
                    if (childGO != null) {
                        LinkedMovement.Log($"Try edit associated child for {gameObject.name}, index {i.ToString()}, name {childGO.name}");
                        EditAssociatedAnimation(childGO, editMode, isEditing);
                    }
                }
            }

            LinkedMovement.Log("Check parent for " + gameObject.name);
            if (gameObject.transform.parent != null && gameObject.transform.parent.gameObject != null) {
                var parentGO = gameObject.transform.parent.gameObject;
                LinkedMovement.Log($"Try edit associated parent for {gameObject.name}, parent: {parentGO.name}");
                EditAssociatedAnimation(parentGO, editMode, isEditing);
            } else {
                LinkedMovement.Log("No parent");
            }
        }

        private static void StopAssociatedAnimation(GameObject gameObject) {
            LinkedMovement.Log("LMUtils.StopAssociatedAnimation for " + gameObject.name);
            // NEW
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LinkedMovement.Log("Found Animation");
                animation.stopSequence();
                // TODO: Think this should use LMAnimation.stopSequence()
                //if (animation.sequence.isAlive) {
                //    LinkedMovement.Log("Stop sequence");
                //    animation.sequence.progress = 0;
                //    animation.sequence.Stop();
                //}
            } else {
                LinkedMovement.Log("No Animation found");
            }

            // OLD
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");
                if (pairing.pairBase.sequence.isAlive) {
                    LinkedMovement.Log("Stop sequence");
                    pairing.pairBase.sequence.progress = 0f;
                    pairing.pairBase.sequence.Stop();
                } else {
                    LinkedMovement.Log("Sequence not alive");
                }
            } else {
                LinkedMovement.Log("No Pairing found");
            }
        }

        private static void StartAssociatedAnimation(GameObject gameObject, bool isEditing) {
            LinkedMovement.Log("LMUtils.StartAssociatedAnimation for " + gameObject.name);

            // NEW
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LinkedMovement.Log("Found Animation");
                // TODO: Is this actually needed?
                //LinkedMovement.Log("DO RECALC");
                animation.getAnimationParams().setStartingValues(gameObject.transform);

                animation.buildSequence(true);

                // TODO: Think this should use LMAnimation.buildSequence()

                // TODO: Is this actually needed?
                //LinkedMovement.Log("DO RECALC");
                //animation.getAnimationParams().setStartingValues(gameObject.transform);
            } else {
                LinkedMovement.Log("No Animation found");
            }

            // OLD
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");

                LinkedMovement.Log("DO RECALC");
                pairing.pairBase.animParams.setStartingValues(gameObject.transform);

                pairing.pairBase.sequence = LMUtils.BuildAnimationSequence(pairing.baseGO.transform, pairing.pairBase.animParams);
            } else {
                LinkedMovement.Log("No Pairing found");
            }
        }

        private static void RestartAssociatedAnimation(GameObject gameObject) {
            LinkedMovement.Log("LMUtils.RestartAssociatedAnimation for " + gameObject.name);

            // NEW
            var animation = LinkedMovement.GetLMController().findAnimationByGameObject(gameObject);
            if (animation != null) {
                LinkedMovement.Log("Found Animation");
                if (animation.sequence.isAlive) {
                    LinkedMovement.Log("Reset sequence progress");
                    animation.sequence.progress = 0f;
                } else {
                    LinkedMovement.Log("Sequence not alive");
                }
            } else {
                LinkedMovement.Log("No Animation found");
            }

            // OLD
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");
                if (pairing.pairBase.sequence.isAlive) {
                    LinkedMovement.Log("Reset sequence progress!");
                    pairing.pairBase.sequence.progress = 0f;
                } else {
                    LinkedMovement.Log("Sequence not alive");
                }
            } else {
                LinkedMovement.Log("No Pairing exists");
            }
        }

        // CONV: Reused as-is
        public static void ResetTransformLocals(Transform transform, Vector3 localPosition, Vector3 localRotation, Vector3 localScale) {
            LinkedMovement.Log("LMUtils.ResetTransformLocals for: " + transform.gameObject.name);
            
            LinkedMovement.Log($"FROM pos: {transform.position.ToString()}, lPos: {transform.localPosition.ToString()}, rot: {transform.eulerAngles.ToString()}, lRot: {transform.localEulerAngles.ToString()}, scale: {transform.localScale.ToString()}");
            LinkedMovement.Log($"TO lPos: {localPosition.ToString()}, lRot: {localRotation.ToString()}, scale: {localScale.ToString()}");

            transform.localPosition = localPosition;
            transform.localEulerAngles = localRotation;
            transform.localScale = localScale;

            LinkedMovement.Log($"DONE pos: {transform.position.ToString()}, lPos: {transform.localPosition.ToString()}, rot: {transform.eulerAngles.ToString()}, lRot: {transform.localEulerAngles.ToString()}, scale: {transform.localScale.ToString()}");
        }

        private static Ease ParseStringToEase(string ease) {
            Ease parsedEase;

            if (Enum.TryParse(ease, out parsedEase)) {
                // OK
            } else {
                // Couldn't parse, use default
                LinkedMovement.Log($"ParseStringToEase couldn't parse '{ease}', using default");
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
            LinkedMovement.Log("GetCumulativeParentLocalRotation value: " + cumulativeValue.ToString());
            var parentTransform = startingTransform.parent;
            if (parentTransform == null) {
                return cumulativeValue;
            }

            LinkedMovement.Log("Adding: " + parentTransform.localEulerAngles.ToString());
            cumulativeValue += parentTransform.localEulerAngles;

            return GetCumulativeParentLocalRotation(parentTransform, cumulativeValue);
        }

        private static void BuildAnimationStep(Transform transform, Sequence sequence, LMAnimationParams animationParams, LMAnimationStep animationStep, ref Vector3 lastLocalRotationTarget) {
            LinkedMovement.Log($"LMUtils.BuildAnimationStep {animationStep.name} for sequence {animationParams.name}");
            LinkedMovement.Log(animationStep.ToString());

            Ease ease = ParseStringToEase(animationStep.ease);
            bool hasPositionChange = !animationStep.targetPosition.Equals(Vector3.zero);
            bool hasRotationChange = !animationStep.targetRotation.Equals(Vector3.zero);
            bool hasScaleChange = !animationStep.targetScale.Equals(Vector3.zero);

            if (hasPositionChange || hasRotationChange || hasScaleChange) {
                LinkedMovement.Log("Has change");
                if (hasPositionChange) {
                    Vector3 positionTarget = animationStep.targetPosition;
                    LinkedMovement.Log("Position target: " + positionTarget.ToString());
                    
                    Vector3 forwardEuler = animationParams.forward.eulerAngles;
                    LinkedMovement.Log("FORWARD euler: " + forwardEuler.ToString());
                    
                    Vector3 parentLocalRotationOffset = GetCumulativeParentLocalRotation(transform, Vector3.zero);
                    LinkedMovement.Log("parenLocalRotationOffset: " + parentLocalRotationOffset.ToString());
                    
                    Vector3 combinedOffset = forwardEuler - parentLocalRotationOffset;
                    LinkedMovement.Log("combinedOffset: " + combinedOffset.ToString());
                    
                    Vector3 newPositionTarget = Quaternion.Euler(combinedOffset) * positionTarget;

                    positionTarget = newPositionTarget;
                    LinkedMovement.Log("Final positionTarget: " + positionTarget.ToString());

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
                    LinkedMovement.Log($"Rotate target: {animationStep.targetRotation.ToString()}, newTarget: {newRotationTarget.ToString()}");
                    sequence.Group(Tween.LocalEulerAngles(transform, lastLocalRotationTarget, newRotationTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                    lastLocalRotationTarget = newRotationTarget;
                }
                if (hasScaleChange) {
                    // TODO: Does this also need last update fix?
                    var newScaleTarget = new Vector3(animationStep.targetScale.x * animationParams.startingLocalScale.x, animationStep.targetScale.y * animationParams.startingLocalScale.y, animationStep.targetScale.z * animationParams.startingLocalScale.z);
                    sequence.Group(Tween.ScaleAdditive(transform, newScaleTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                }
                // Add ChainDelay to "close" the current Sequence Group
                sequence.ChainDelay(0f);
            } else {
                LinkedMovement.Log("No change");
                sequence.ChainDelay(animationStep.startDelay);
                sequence.ChainDelay(animationStep.duration);
                sequence.ChainDelay(animationStep.endDelay);
            }

        }

        // CONV: Reused as-is
        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence name: " + animationParams.name);

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
            LinkedMovement.Log("transform.localEulerAngels: " + lastLocalRotationTarget.ToString());
            foreach (var animationStep in animationParams.animationSteps) {
                BuildAnimationStep(transform, sequence, animationParams, animationStep, ref lastLocalRotationTarget);
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
        
        public static Vector3 FindBuildObjectsCenterPosition(List<BuildableObject> objects) {
            var firstPos = objects[0].transform.position;
            var minX = firstPos.x;
            var maxX = firstPos.x;
            var minY = firstPos.y;
            var maxY = firstPos.y;
            var minZ = firstPos.z;
            var maxZ = firstPos.z;

            foreach (var target in objects) {
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

        public static void AddObjectHighlight(BuildableObject bo, Color color) {
            EnsureHighlightHandlesReady();
            // Remove target if already present
            RemoveObjectHighlight(bo);

            List<Renderer> renderers = new List<Renderer>();
            bo.retrieveRenderersToHighlight(renderers);
            var highlightHandle = HighlightOverlayController.Instance.add(renderers, -1, color);
            HighlightHandles.Add(bo, highlightHandle);
        }

        public static void RemoveObjectHighlight(BuildableObject bo) {
            EnsureHighlightHandlesReady();
            if (HighlightHandles.ContainsKey(bo)) {
                HighlightHandles[bo].remove();
                HighlightHandles.Remove(bo);
            }
        }

        public static void ResetObjectHighlights() {
            if (HighlightHandles != null) {
                foreach (var handle in HighlightHandles.Values) {
                    handle.remove();
                }
                HighlightHandles.Clear();
            }
            HighlightHandles = null;
        }

        private static void EnsureHighlightHandlesReady() {
            if (HighlightHandles == null)
                HighlightHandles = new Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle>();
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

        public static int GetPairingDepth(Pairing pairing) {
            int depth = 0;

            CountPairingParent(pairing, ref depth);

            return depth;
        }

        private static void CountPairingParent(Pairing pairing, ref int depth) {
            if (pairing.baseGO.transform.parent == null || pairing.baseGO.transform.parent.gameObject == null) return;

            var parentPairing = LinkedMovement.GetController().findPairingByBaseGameObject(pairing.baseGO.transform.parent.gameObject);
            if (parentPairing != null) {
                depth++;
                CountPairingParent(parentPairing, ref depth);
            }
        }

        public static List<Pairing> SortPairings(List<Pairing> pairings) {
            LinkedMovement.Log("LMUtils.SortPairings");
            var sortedPairings = new List<Pairing>();
            foreach (var pairing in pairings) {
                var lowestPairing = FindLowestPairing(pairing);
                AddPairingAndTargets(lowestPairing, sortedPairings);
            }
            // Return Bottom-Up order
            sortedPairings = sortedPairings.AsEnumerable().Reverse().ToList();
            return sortedPairings;
        }

        private static Pairing FindLowestPairing(Pairing pairing) {
            LinkedMovement.Log("LMUtils.FindLowestPairing from " + pairing.pairingName);
            var baseGO = pairing.baseGO;
            var parentPairing = LinkedMovement.GetController().findPairingByTargetGameObject(baseGO);
            if (parentPairing != null) {
                return FindLowestPairing(parentPairing);
            }
            LinkedMovement.Log("Lowest pairing " + pairing.pairingName);
            return pairing;
        }

        private static void AddPairingAndTargets(Pairing pairing, List<Pairing> pairings) {
            LinkedMovement.Log("LMUtils.AddPairingAndTargets from " + pairing.pairingName);
            if (!pairings.Contains(pairing)) {
                pairings.Add(pairing);
            }
            // TODO: Skip all targets if pairing already added?
            foreach (var targetGO in pairing.targetGOs) {
                //var parentPairing = LinkedMovement.GetController().findPairingByTargetGameObject(targetGO);
                var parentPairing = LinkedMovement.GetController().findPairingByBaseGameObject(targetGO);
                if (parentPairing != null) {
                    AddPairingAndTargets(parentPairing, pairings);
                }
            }
        }

        public static void RemovePairTargetFromUnusedTargets(List<GameObject> oldTargetGOs, List<BuildableObject> newTargetObjects) {
            LinkedMovement.Log("LMUtils.RemovePairTargetFromUnusedTargets");
            foreach (var oldTargetGO in oldTargetGOs) {
                var oldTargetObject = GetBuildableObjectFromGameObject(oldTargetGO);
                if (!newTargetObjects.Contains(oldTargetObject)) {
                    LinkedMovement.Log("Try to remove PairTarget from " + oldTargetObject.name);
                    var didRemove = oldTargetObject.removeCustomData<PairTarget>();
                    LinkedMovement.Log("Did remove: " + didRemove.ToString());
                }
            }
        }

        // BuildableObjects in the old list that are not in the new list need to have their parent reset
        // and their ChunkedMesh (if present) re-enabled
        public static void ResetUnusedTargets(List<BuildableObject> oldTargetObjects, List<BuildableObject> newTargetObjects) {
            LinkedMovement.Log("LMUtils.ResetUnusedTargets");
            // TODO: Re-enable ChunkedMesh for unused targets
            foreach (var oldTargetObject in oldTargetObjects) {
                if (!newTargetObjects.Contains(oldTargetObject)) {
                    LinkedMovement.Log("Reset old target " + oldTargetObject.gameObject.name);
                    oldTargetObject.transform.SetParent(null);
                    SetChunkedMeshEnalbedIfPresent(oldTargetObject, true);
                }
            }
        }

        public static bool HitTargetIsDisqualified(BuildableObject bo) {
            var controller = LinkedMovement.GetController();
            // Check that the hit target is not already selected
            if (controller.originObject == bo || controller.targetObjects.Contains(bo)) {
                // Already selected, disqualify
                return true;
            }

            if (controller.pickingMode == LinkedMovementController.PickingMode.Origin) {
                // Picking origin/base, check hit target is not already a PairBase
                if (GetPairBaseFromSerializedMonoBehaviour(bo) != null) {
                    // Already has PairBase, disqualify
                    return true;
                }
            } else {
                // Picking targets, check hit target is not already a PairTarget
                if (GetPairTargetFromSerializedMonoBehaviour(bo) != null) {
                    // Already has PairTarget, disqualify
                    return true;
                }
            }
            return false;
        }

        public static List<GameObject> GetAssociatedGameObjects(BuildableObject originObject, List<BuildableObject> targetObjects) {
            LinkedMovement.Log("LMUtils.GetAssociatedGameObjects");
            var associated = new List<GameObject>();
            if (originObject != null && originObject.gameObject != null) {
                LinkedMovement.Log("Add associated origin " + originObject.gameObject.name);
                associated.Add(originObject.gameObject);
            }
            if (targetObjects != null && targetObjects.Count > 0) {
                foreach (var targetObject in targetObjects) {
                    if (targetObject.gameObject != null) {
                        LinkedMovement.Log("Add associated target " + targetObject.gameObject.name);
                        associated.Add(targetObject.gameObject);
                    }
                }
            }
            LinkedMovement.Log($"LMUtils.GetAssociatedGameObjects got {associated.Count} associated objects");
            return associated;
        }

        public static void UpdateGameMouseMode(bool mouseToolActive) {
            // Mouse tool changes can happen before the park has fully loaded. Skip updates in this case.
            if (!LinkedMovement.HasController()) return;

            LinkedMovement.Log("LMUtils.UpdateGameMouseMode: " + mouseToolActive);
            LinkedMovement.GetController().showGeneratedOrigins = mouseToolActive;
            UpdateGeneratedOriginRendering();
        }

        public static void UpdateGeneratedOriginRendering() {
            var shouldRender = LinkedMovement.GetController().showGeneratedOrigins;
            var pairings = LinkedMovement.GetController().getPairings();

            foreach (var pairing in pairings) {
                var baseGO = pairing.baseGO;
                var baseBO = GetBuildableObjectFromGameObject(baseGO);
                if (IsGeneratedOrigin(baseBO)) {
                    baseGO.GetComponent<Renderer>().enabled = shouldRender;
                }
            }
        }

    }
}
