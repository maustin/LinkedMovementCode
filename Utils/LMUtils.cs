// ATTRIB: TransformAnarchy
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    // TODO: Lots of UI stuff. Split or move to UI.Utils?
    static class LMUtils {
        private static Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle> HighlightHandles;

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

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence");
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

            Sequence sequence = DOTween.Sequence();

            var toPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration).SetEase(toEase);
            var toRotationTween = DOTween.To(() => transform.rotation, value => transform.rotation = value, animationParams.targetRotation, animationParams.toDuration).SetOptions(false).SetEase(toEase);

            var fromPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition, animationParams.fromDuration).SetEase(fromEase);
            var fromRotationTween = DOTween.To(() => transform.rotation, value => transform.rotation = value, -animationParams.targetRotation, animationParams.fromDuration).SetOptions(false).SetRelative(true).SetEase(fromEase);

            sequence.Append(toPositionTween);
            sequence.Join(toRotationTween);

            sequence.AppendInterval(animationParams.fromDelay);

            sequence.Append(fromPositionTween);
            sequence.Join(fromRotationTween);

            var restartDelay = animationParams.isTriggerable ? 0 : animationParams.restartDelay;
            sequence.AppendInterval(restartDelay);

            // TODO: Ability to set loops for triggered?
            if (isEditing || !animationParams.isTriggerable) {
                sequence.SetLoops(-1);
            } else {
                sequence.SetLoops(0);
                sequence.Pause();
            }

            if (!isEditing && !animationParams.isTriggerable) {
                sequence.Pause();
                var initialDelay = UnityEngine.Random.Range(animationParams.initialStartDelayMin, animationParams.initialStartDelayMax);
                LinkedMovement.Log($"Initial delay for {animationParams.name} is {initialDelay}");
                DOVirtual.DelayedCall(initialDelay, () => {
                    LinkedMovement.Log("Run initial delayed sequence " + animationParams.name);
                    sequence.Play();
                }, false);
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
