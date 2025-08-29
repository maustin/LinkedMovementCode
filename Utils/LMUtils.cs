// ATTRIB: TransformAnarchy
using PrimeTween;
using System;
using System.Collections.Generic;
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
            //return bo != null && bo.getName() == "LMOriginBase";
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
                        //LinkedMovement.Log("Same pairId!");
                        targets.Add(so);
                    }
                }
            }
            LinkedMovement.Log($"Found {targets.Count.ToString()} targets");
            return targets;
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

            // Check up
            if (gameObject.transform.parent != null && gameObject.transform.parent.gameObject != null) {
                RestartAssociatedAnimations(gameObject.transform.parent.gameObject);
            }
        }

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence");
            //LinkedMovement.Log(animationParams.ToString());

            // TODO: Need to prevent adding multiple pairings on the same objects
            // E.g. an object can only be the base of a single Pairing

            // Parse easings
            Ease toEase;
            Ease fromEase;

            if (Enum.TryParse(animationParams.toEase, out toEase)) {
                LinkedMovement.Log($"Sucessfully parsed toEase {animationParams.toEase}");
            } else {
                LinkedMovement.Log($"Failed to parse toEase {animationParams.toEase}");
                toEase = Ease.InOutQuad;
            }

            if (Enum.TryParse(animationParams.fromEase, out fromEase)) {
                LinkedMovement.Log($"Sucessfully parsed fromEase {animationParams.fromEase}");
            } else {
                LinkedMovement.Log($"Failed to parse fromEase {animationParams.fromEase}");
                fromEase = Ease.InOutQuad;
            }

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

            //Vector3 rotationOffset = animationParams.startingLocalRotation - animationParams.originalLocalRotation;
            //Vector3 rotatedPositionTarget = Quaternion.Euler(rotationOffset) * animationParams.targetPosition;

            // TODO: Thinking triggerable can have restartDelay as a cool-down period
            //var restartDelay = animationParams.isTriggerable ? 0 : animationParams.restartDelay;

            var toPositionTween = Tween.LocalPositionAdditive(transform, animationParams.targetPosition, animationParams.toDuration, toEase);
            //var toPositionTween = Tween.LocalPosition(transform, animationParams.startingLocalPosition + animationParams.targetPosition, animationParams.toDuration, toEase);
            //var toPositionTween = Tween.Position(transform, animationParams.startingPosition + rotatedPositionTarget, animationParams.toDuration, toEase);
            //var toRotationTween = Tween.LocalEulerAngles(transform, animationParams.startingRotation, animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration, toEase);
            var toRotationTween = Tween.LocalRotationAdditive(transform, animationParams.targetRotation, animationParams.toDuration, toEase);

            var fromPositionTween = Tween.LocalPositionAdditive(transform, -animationParams.targetPosition, animationParams.fromDuration, fromEase);
            //var fromPositionTween = Tween.LocalPosition(transform, animationParams.startingLocalPosition, animationParams.fromDuration, fromEase);
            //var fromPositionTween = Tween.Position(transform, animationParams.startingPosition, animationParams.fromDuration, fromEase);
            //var fromRotationTween = Tween.LocalEulerAngles(transform, animationParams.startingRotation + animationParams.targetRotation, animationParams.startingRotation, animationParams.fromDuration, fromEase);
            var fromRotationTween = Tween.LocalRotationAdditive(transform, -animationParams.targetRotation, animationParams.fromDuration, fromEase);

            Sequence sequence = Sequence.Create(cycles: loops, cycleMode: CycleMode.Restart)
                .Chain(Sequence.Create()
                    //.Group(toRotationTween)
                    .Group(toPositionTween)
                    .Group(toRotationTween)
                    )
                .ChainDelay(animationParams.fromDelay)
                .Chain(Sequence.Create()
                    //.Group(fromRotationTween)
                    .Group(fromPositionTween)
                    .Group(fromRotationTween)
                    )
                .ChainDelay(animationParams.restartDelay)
                //.ChainDelay(1f);
                ;

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
    }
}
