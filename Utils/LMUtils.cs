// ATTRIB: TransformAnarchy
using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    // TODO: Lots of UI stuff. Consider splitting UI stuff out to, like, UI.Utils
    static class LMUtils {
        private static Dictionary<BuildableObject, HighlightOverlayController.HighlightHandle> HighlightHandles;

        public static bool IsGeneratedOrigin(BuildableObject bo) {
            return bo != null && bo.getName() == "LMOriginBase";
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

            foreach (var targetGO in pairing.targetGOs) {
                RestartAssociatedAnimations(targetGO);
            }
        }

        public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
            LinkedMovement.Log("LMUtils.BuildAnimationSequence");
            LinkedMovement.Log(animationParams.ToString());

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

            Tween toPositionTween;
            Tween toRotationTween;
            Tween fromPositionTween;
            Tween fromRotationTween;

            toPositionTween = Tween.LocalPositionAdditive(transform, animationParams.targetPosition, animationParams.toDuration, toEase);
            toRotationTween = Tween.LocalRotationAdditive(transform, animationParams.targetRotation, animationParams.toDuration, toEase);

            fromPositionTween = Tween.LocalPositionAdditive(transform, -animationParams.targetPosition, animationParams.fromDuration, fromEase);
            fromRotationTween = Tween.LocalRotationAdditive(transform, -animationParams.targetRotation, animationParams.fromDuration, fromEase);

            Sequence sequence = Sequence.Create(cycles: -1, cycleMode: CycleMode.Restart)
                .ChainDelay(0)
                .Chain(Sequence.Create()
                    .Group(toPositionTween)
                    .Group(toRotationTween))
                .ChainDelay(animationParams.fromDelay)
                .Chain(Sequence.Create()
                    .Group(fromPositionTween)
                    .Group(fromRotationTween))
                .ChainDelay(animationParams.restartDelay)
                ;

            return sequence;
        }
        //public static Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams, bool isEditing = false) {
        //    LinkedMovement.Log("LMUtils.BuildAnimationSequence");
            
        //    // Parse easings
        //    Ease toEase;
        //    Ease fromEase;

        //    if (Enum.TryParse(animationParams.toEase, out toEase)) {
        //        LinkedMovement.Log($"Sucessfully parsed toEase {animationParams.toEase}");
        //    } else {
        //        LinkedMovement.Log($"Failed to parse toEase {animationParams.toEase}");
        //        toEase = Ease.InOutQuad;
        //    }

        //    if (Enum.TryParse(animationParams.fromEase, out fromEase)) {
        //        LinkedMovement.Log($"Sucessfully parsed fromEase {animationParams.fromEase}");
        //    } else {
        //        LinkedMovement.Log($"Failed to parse fromEase {animationParams.fromEase}");
        //        fromEase = Ease.InOutQuad;
        //    }

        //    var restartDelay = animationParams.isTriggerable ? 0 : animationParams.restartDelay;

        //    //LinkedMovement.Log("!!! start anim pos: " + animationParams.startingPosition.ToString());
        //    //LinkedMovement.Log("!!! targt anim pos: " + animationParams.targetPosition.ToString());
        //    //LinkedMovement.Log("!!! start pos: " + transform.position.ToString());
        //    //LinkedMovement.Log("!!! start lps: " + transform.localPosition.ToString());

        //    var hasParent = transform.parent != null;
        //    Tweener toPositionTween;
        //    Tweener fromPositionTween;

        //    Sequence sequence = DOTween.Sequence();

        //    //if (sequences.Contains(sequence)) {
        //    //    LinkedMovement.Log("!!!! HAS SEQUENCE!");
        //    //} else {
        //    //    LinkedMovement.Log("!!!! doesn't have sequence");
        //    //    sequences.Add(sequence);
        //    //}

        //    //transform.DOLocalMove(new Vector3(0, 0, -2), 1);

        //    if (hasParent) {
        //        LinkedMovement.Log("!!! has parent");

        //        //toPositionTween = DOTween.To(() => transform);

        //        //toPositionTween = transform.DOLocalMove(new Vector3(0, 1, 0), 1);//.SetRelative(true);
        //        //fromPositionTween = transform.DOLocalMove(new Vector3(0, 0, 0), 1);//.SetRelative(true);

        //        //toPositionTween = transform.DOLocalMove(animationParams.targetPosition, 1).SetRelative(true);
        //        //fromPositionTween = transform.DOLocalMove(-animationParams.targetPosition, 1).SetRelative(true);

        //        toPositionTween = DOTweenShims.DOLocalMove(transform, new Vector3(0, 0, -1), 1);

        //        //toPositionTween = DOTweenShims.DOLocalMove(transform, animationParams.targetPosition, 1).SetRelative(true);
        //        //fromPositionTween = DOTweenShims.DOLocalMove(transform, -animationParams.targetPosition, 1).SetRelative(true);

        //        //sequence.Insert(0, toPositionTween);
        //        //sequence.Insert(1, fromPositionTween);
        //    } else {
        //        LinkedMovement.Log("!!! NO parent");
        //        toPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration).SetEase(toEase);
        //        //fromPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition, animationParams.fromDuration).SetEase(fromEase);

        //        //sequence.Append(toPositionTween);
        //        //sequence.Append(fromPositionTween);
        //    }

        //    //var toPositionTween = DOTweenShims.DOLocalMove(transform, animationParams.targetPosition, 1);//.SetRelative(true);
        //    //var fromPositionTween = DOTweenShims.DOLocalMove(transform, animationParams.startingPosition, 1);//.SetRelative(true);

        //    sequence.Append(toPositionTween);
        //    //sequence.AppendInterval(1);
        //    //sequence.Append(fromPositionTween);
        //    //sequence.AppendInterval(1);

        //    //Sequence sequence = DOTween.Sequence()
        //    //    .Append(transform.DOLocalMove(new Vector3(0, 0, -1), 1)).SetRelative(true)
        //    //    .AppendInterval(1)
        //    //    .Append(transform.DOLocalMove(new Vector3(0, 0, 1), 1)).SetRelative(true)
        //    //    .AppendInterval(1);

        //    //Sequence sequence = DOTween.Sequence();

        //    //var toPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration).SetEase(toEase);
        //    //var toRotationTween = DOTween.To(() => transform.rotation, value => transform.rotation = value, animationParams.targetRotation, animationParams.toDuration).SetOptions(false).SetRelative(true).SetEase(toEase);

        //    //var fromPositionTween = DOTween.To(() => transform.position, value => transform.position = value, animationParams.startingPosition, animationParams.fromDuration).SetEase(fromEase);
        //    //var fromRotationTween = DOTween.To(() => transform.rotation, value => transform.rotation = value, -animationParams.targetRotation, animationParams.fromDuration).SetOptions(false).SetRelative(true).SetEase(fromEase);

        //    //sequence.Append(toPositionTween);
        //    //sequence.Join(toRotationTween);

        //    //sequence.AppendInterval(animationParams.fromDelay);

        //    //sequence.Append(fromPositionTween);
        //    //sequence.Join(fromRotationTween);

        //    //sequence.AppendInterval(restartDelay);


        //    // TODO: Ability to set loops for triggered?
        //    if (isEditing || !animationParams.isTriggerable) {
        //        sequence.SetLoops(-1);
        //    } else {
        //        sequence.SetLoops(0);
        //        sequence.Pause();
        //    }

        //    if (!isEditing && !animationParams.isTriggerable) {
        //        LinkedMovement.Log("Not editing and not triggerable");
        //        sequence.Pause();
        //        var initialDelay = UnityEngine.Random.Range(animationParams.initialStartDelayMin, animationParams.initialStartDelayMax);
        //        LinkedMovement.Log($"Initial delay for {animationParams.name} is {initialDelay}");
        //        DOVirtual.DelayedCall(initialDelay, () => {
        //            LinkedMovement.Log("Run initial delayed sequence " + animationParams.name);
        //            sequence.Play();
        //        }, false);
        //    }

        //    sequence.SetUpdate(UpdateType.Late, false);

        //    return sequence;
        //}

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
