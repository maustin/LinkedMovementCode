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
        private static Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle> HighlightHandles;

        public static void LogBuildableObjectComponents(BuildableObject bo) {
            LinkedMovement.Log("LogBuildableObjectComponents:");
            Component[] components = bo.GetComponents<Component>();
            foreach (var component in components) {
                LinkedMovement.Log(component.GetType().ToString());
            }
            LinkedMovement.Log("End list");
        }

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
                PairTarget pairTarget = LMUtils.GetPairTargetFromSerializedMonoBehaviour(so);
                if (pairTarget != null) {
                    if (pairTarget.pairId == pairBase.pairId) {
                        targets.Add(so);
                    }
                }
            }
            LinkedMovement.Log($"Found {targets.Count.ToString()} targets");
            return targets;
        }

        public static void TryToBuildPairingFromBuiltObjects(List<BuildableObject> builtObjectInstances) {
            foreach (var buildableObject in builtObjectInstances) {
                TryToBuildPairingFromBuildableObject(buildableObject, builtObjectInstances);
            }
        }

        private static void TryToBuildPairingFromBuildableObject(BuildableObject possibleOriginBO, List<BuildableObject> builtObjectInstances) {
            PairBase pairBase = LMUtils.GetPairBaseFromSerializedMonoBehaviour(possibleOriginBO);
            if (pairBase == null) return;

            LinkedMovement.Log("TryToBuildPairingFromBuildableObject");
            BuildableObject originObject = possibleOriginBO;

            List<BuildableObject> targets = new List<BuildableObject>();
            List<PairTarget> pairTargets = new List<PairTarget>();

            foreach (var bo in builtObjectInstances) {
                var pairTarget = LMUtils.GetPairTargetFromSerializedMonoBehaviour(bo);
                if (pairTarget != null && pairTarget.pairId == pairBase.pairId) {
                    targets.Add(bo);
                    pairTargets.Add(pairTarget);
                }
            }

            if (targets.Count > 0) {
                LinkedMovement.Log("Create Pairing " + pairBase.pairName);
                pairBase.animParams.setStartingValues(originObject.transform); //, LMUtils.IsGeneratedOrigin(originObject));
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
            targetObject.SetParent(baseTransform);
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

        public static void RestartAssociatedAnimations(GameObject gameObject) {
            LinkedMovement.Log("LMUtils.RestartAssociatedAnimations for " + gameObject.name);

            var pairing = LinkedMovement.GetController().findPairingByBaseGameObject(gameObject);
            if (pairing == null) {
                LinkedMovement.Log("Failed to find existing Pairing");
                return;
            }
            LinkedMovement.Log("Found pairing name: " + pairing.pairingName + ", id: " + pairing.pairingId);

            var pairBase = pairing.pairBase;
            if (pairBase.sequence.isAlive) {
                LinkedMovement.Log("Reset progress!");
                pairBase.sequence.progress = 0f;
            }

            var animationParams = pairBase.animParams;
            LMUtils.ResetTransformLocals(gameObject.transform, animationParams.startingLocalPosition, animationParams.startingLocalRotation, animationParams.startingLocalScale);

            // Check up
            if (gameObject.transform.parent != null && gameObject.transform.parent.gameObject != null) {
                RestartAssociatedAnimations(gameObject.transform.parent.gameObject);
            }
        }

        public static void ResetTransformLocals(Transform transform, Vector3 localPosition, Vector3 localRotation, Vector3 localScale) {
            LinkedMovement.Log("LMUtils.ResetTransformLocals");

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

        private static void BuildAnimationStep(Transform transform, Sequence sequence, LMAnimationParams animationParams, LMAnimationStep animationStep, ref Vector3 lastRotationTarget) {
            LinkedMovement.Log("BuildAnimationStep");

            sequence.ChainDelay(animationStep.startDelay);

            Ease ease = ParseStringToEase(animationStep.ease);
            bool hasPositionChange = !animationStep.targetPosition.Equals(Vector3.zero);
            bool hasRotationChange = !animationStep.targetRotation.Equals(Vector3.zero);
            bool hasScaleChange = !animationStep.targetScale.Equals(Vector3.zero);

            if (hasPositionChange || hasRotationChange || hasScaleChange) {
                var subSequence = Sequence.Create();
                if (hasPositionChange) {
                    sequence.Group(Tween.LocalPositionAdditive(transform, animationStep.targetPosition, animationStep.duration, ease));
                }
                if (hasRotationChange) {
                    var newRotationTarget = lastRotationTarget + animationStep.targetRotation;
                    sequence.Group(Tween.LocalEulerAngles(transform, lastRotationTarget, newRotationTarget, animationStep.duration, ease));
                    lastRotationTarget = newRotationTarget;
                }
                if (hasScaleChange) {
                    var targetScale = new Vector3(animationStep.targetScale.x * animationParams.startingLocalScale.x, animationStep.targetScale.y * animationParams.startingLocalScale.y, animationStep.targetScale.z * animationParams.startingLocalScale.z);
                    sequence.Group(Tween.ScaleAdditive(transform, targetScale, animationStep.duration, ease));
                }
            }

            sequence.ChainDelay(animationStep.endDelay);

        }

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence");

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

            var lastRotationTarget = animationParams.startingLocalRotation;
            foreach (var animationStep in animationParams.animationSteps) {
                BuildAnimationStep(transform, sequence, animationParams, animationStep, ref lastRotationTarget);
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

    }
}
