// ATTRIB: TransformAnarchy
using LinkedMovement.Animation;
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
        private static HashSet<GameObject> associatedGameObjects;

        public static void LogBuildableObjectComponents(BuildableObject bo) {
            LinkedMovement.Log("LogBuildableObjectComponents:");
            Component[] components = bo.GetComponents<Component>();
            foreach (var component in components) {
                LinkedMovement.Log(component.GetType().ToString());
            }
            LinkedMovement.Log("End list");
        }

        // Built-in objects have a ChunkedMesh component. This component can prevent visual updates
        // while we're modifying animations that affect their GameObject.
        // Disable when creating and modifying. Re-enable when finished.
        public static void SetChunkedMeshEnalbedIfPresent(BuildableObject bo, bool enalbed) {
            var chunker = bo.GetComponent<ChunkedMesh>();
            if (chunker != null) {
                LinkedMovement.Log($"ChunkedMesh for {bo.getName()} set to {enalbed.ToString()}");
                chunker.enabled = enalbed;
            }
        }

        public static bool IsGeneratedOrigin(BuildableObject bo) {
            return bo != null && bo.getName().Contains("LMOriginBase");
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
        public static void TryToBuildPairingFromBuiltObjects(List<BuildableObject> builtObjectInstances) {
            foreach (var buildableObject in builtObjectInstances) {
                TryToBuildPairingFromBuildableObject(buildableObject, builtObjectInstances);
            }
        }

        private static void TryToBuildPairingFromBuildableObject(BuildableObject possibleOriginBO, List<BuildableObject> builtObjectInstances) {
            PairBase pairBase = GetPairBaseFromSerializedMonoBehaviour(possibleOriginBO);
            if (pairBase == null) return;

            LinkedMovement.Log("TryToBuildPairingFromBuildableObject");
            BuildableObject originObject = possibleOriginBO;

            List<BuildableObject> targets = new List<BuildableObject>();
            List<PairTarget> pairTargets = new List<PairTarget>();

            foreach (var bo in builtObjectInstances) {
                var pairTarget = GetPairTargetFromSerializedMonoBehaviour(bo);
                if (pairTarget != null && pairTarget.pairId == pairBase.pairId) {
                    targets.Add(bo);
                    pairTargets.Add(pairTarget);
                }
            }

            if (targets.Count > 0) {
                LinkedMovement.Log("Create Pairing " + pairBase.pairName);
                pairBase.animParams.setStartingValues(originObject.transform);
                pairBase.animParams.calculateRotationOffset();
                // create new pairing ID so we don't collide with existing pairings
                var newPairingId = Guid.NewGuid().ToString();
                pairBase.pairId = newPairingId;
                foreach (var pairTarget in pairTargets) {
                    pairTarget.pairId = newPairingId;
                }
                var targetGameObjects = targets.Select(t => t.gameObject).ToList();

                var pairing = new Pairing(originObject.gameObject, targetGameObjects, newPairingId, pairBase.pairName);
                pairing.connect();
            }
        }

        public static void AttachTargetToBase(Transform baseObject, Transform targetObject) {
            LinkedMovement.Log("LMUtils.AttachTargetToBase, parent: " + baseObject.name + ", target: " + targetObject.name);
            var baseTransform = baseObject;
            if (targetObject.IsChildOf(baseTransform)) {
                LinkedMovement.Log("ALREADY A CHILD!");
            } else {
                LinkedMovement.Log("Making child");
                LinkedMovement.Log($"Pre Make Child position: {targetObject.position.ToString()}, localPosition: {targetObject.localPosition.ToString()} localRotation: {targetObject.localEulerAngles.ToString()}");
                targetObject.SetParent(baseTransform);
                LinkedMovement.Log($"Post Make Child position: {targetObject.position.ToString()}, localPosition: {targetObject.localPosition.ToString()} localRotation: {targetObject.localEulerAngles.ToString()}");
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
            LinkedMovement.Log("LMUtils.PreAssociatedGameObjects");
            if (associatedGameObjects != null) {
                throw new Exception("LMUtils.PreAssociatedGameObjects ALREADY RUNNING!");
            }
            associatedGameObjects = new HashSet<GameObject>();
        }

        private static void CleanupAssociateGameObjects() {
            LinkedMovement.Log("LMUtils.CleanupAssociatedGameObjects");
            associatedGameObjects.Clear();
            associatedGameObjects = null;
        }

        public static void EditAssociatedAnimations(List<GameObject> gameObjects, AssociatedAnimationEditMode editMode, bool isEditing) {
            LinkedMovement.Log($"LMUtils.EditAssociatedAnimations mode {editMode.ToString()} with {gameObjects.Count} gameObjects, isEditing: {isEditing.ToString()}");
            PrepAssociatedGameObjects();
            foreach (GameObject go in gameObjects) {
                EditAssociatedAnimation(go, editMode, isEditing);
            }
            CleanupAssociateGameObjects();
        }

        private static void EditAssociatedAnimation(GameObject gameObject, AssociatedAnimationEditMode editMode, bool isEditing) {
            LinkedMovement.Log("LMUtils.EditAssociatedAnimation for " + gameObject.name);
            var gameObjectHasBeenVisited = associatedGameObjects.Contains(gameObject);
            if (gameObjectHasBeenVisited) {
                LinkedMovement.Log("Already visited GameObject " + gameObject.name);
                return;
            }

            associatedGameObjects.Add(gameObject);

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
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");
                if (pairing.pairBase.sequence.isAlive) {
                    LinkedMovement.Log("Stop sequence");
                    pairing.pairBase.sequence.progress = 0f;
                    pairing.pairBase.sequence.Stop();
                    //pairing.pairBase.animParams.calculateRotationOffset();
                } else {
                    LinkedMovement.Log("Sequence not alive");
                }
            } else {
                LinkedMovement.Log("No Pairing exists");
            }
        }

        private static void StartAssociatedAnimation(GameObject gameObject, bool isEditing) {
            LinkedMovement.Log("LMUtils.StartAssociatedAnimation for " + gameObject.name);
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");
                pairing.pairBase.animParams.setStartingValues(gameObject.transform);
                pairing.pairBase.animParams.calculateRotationOffset();
                pairing.pairBase.sequence = LMUtils.BuildAnimationSequence(pairing.baseGO.transform, pairing.pairBase.animParams);
            } else {
                LinkedMovement.Log("No Pairing exists");
            }
        }

        private static void RestartAssociatedAnimation(GameObject gameObject) {
            LinkedMovement.Log("LMUtils.RestartAssociatedAnimation for " + gameObject.name);
            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing != null) {
                LinkedMovement.Log($"Found pairing name: {pairing.pairingName}, id: {pairing.pairingId}");
                if (pairing.pairBase.sequence.isAlive) {
                    LinkedMovement.Log("Reset sequence progress!");
                    pairing.pairBase.sequence.progress = 0f;
                    //pairing.pairBase.sequence.Stop();
                    //pairing.pairBase.animParams.calculateRotationOffset();
                    //pairing.pairBase.sequence = LMUtils.BuildAnimationSequence(pairing.baseGO.transform, pairing.pairBase.animParams);
                } else {
                    LinkedMovement.Log("Sequence not alive");
                }
            } else {
                LinkedMovement.Log("No Pairing exists");
            }
        }

        public static void ResetTransformLocals(Transform transform, Vector3 localPosition, Vector3 localRotation, Vector3 localScale) {
            LinkedMovement.Log("LMUtils.ResetTransformLocals");
            LinkedMovement.Log($"FROM position: {transform.localPosition.ToString()}, rotation: {transform.localEulerAngles.ToString()}, scale: {transform.localScale.ToString()}");
            LinkedMovement.Log($"TO position: {localPosition.ToString()}, rotation: {localRotation.ToString()}, scale: {localScale.ToString()}");

            transform.localPosition = localPosition;
            transform.localEulerAngles = localRotation;
            transform.localScale = localScale;
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
                    //var newLocalPositionTarget = lastLocalPositionTarget + animationStep.targetPosition;
                    //sequence.Group(Tween.LocalPosition(transform, lastLocalPositionTarget, newLocalPositionTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                    //lastLocalPositionTarget = newLocalPositionTarget;
                    sequence.Group(Tween.LocalPositionAdditive(transform, animationStep.targetPosition, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay)
                        //.OnUpdate(target: transform, (target, tween) => {
                        //    LinkedMovement.Log($"Update pos: {target.position.ToString()}, lPos: {target.localPosition.ToString()}, rot: {target.eulerAngles.ToString()}, lRot: {target.localEulerAngles.ToString()}");
                        //})
                    );
                }
                if (hasRotationChange) {
                    var newRotationTarget = lastLocalRotationTarget + animationStep.targetRotation;
                    //LinkedMovement.Log("Rotation change: " + newRotationTarget.ToString());
                    sequence.Group(Tween.LocalEulerAngles(transform, lastLocalRotationTarget, newRotationTarget, animationStep.duration, ease, default, default, animationStep.startDelay, animationStep.endDelay));
                    lastLocalRotationTarget = newRotationTarget;
                }
                if (hasScaleChange) {
                    // TODO: Does this also need last update fix?
                    var newScaleTarget = new Vector3(animationStep.targetScale.x * animationParams.startingLocalScale.x, animationStep.targetScale.y * animationParams.startingLocalScale.y, animationStep.targetScale.z * animationParams.startingLocalScale.z);
                    //LinkedMovement.Log("Scale change: " + newScaleTarget.ToString());
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

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence name: " + animationParams.name);

            // TODO: Need to prevent adding multiple pairings on the same objects
            // E.g. an object can only be the base of a single Pairing

            int loops = -1;
            float startingDelay = 0f;

            if (!isEditing) {
                if (animationParams.isTriggerable) {
                    // Only play once for triggers
                    loops = 0;
                } else {
                    if (animationParams.initialStartDelayMin > 0f || animationParams.initialStartDelayMax > 0f) {
                        startingDelay = UnityEngine.Random.Range(animationParams.initialStartDelayMin, animationParams.initialStartDelayMax);
                    }
                }
            }

            Sequence sequence = Sequence.Create(cycles: loops, cycleMode: CycleMode.Restart);

            //var lastLocalPositionTarget = animationParams.startingLocalPosition;
            var lastLocalRotationTarget = animationParams.startingLocalRotation;
            foreach (var animationStep in animationParams.animationSteps) {
                BuildAnimationStep(transform, sequence, animationParams, animationStep, ref lastLocalRotationTarget);
            }

            if (startingDelay > 0f) {
                sequence.isPaused = true;
                Tween.Delay(startingDelay, () => sequence.isPaused = false);
            }

            return sequence;
        }
        
        public static Vector3 FindBuildObjectsCenterPosition(List<BuildableObject> objects) {
            var startingPos = objects[0].transform.position;
            var minX = startingPos.x;
            var maxX = startingPos.x;
            var minY = startingPos.y;
            var maxY = startingPos.y;
            var minZ = startingPos.z;
            var maxZ = startingPos.z;

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

    }
}
