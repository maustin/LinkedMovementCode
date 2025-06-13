using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    static class TAUtils {
        static private HighlightOverlayController.HighlightHandle CurrentHighlightHandle;
        static private BuildableObject CurrentHighlightedObject;
        static private IEnumerator CurrentHightlightCoroutine;

        static public void AttachTargetToBase(Transform baseObject, Transform targetObject) {
            LinkedMovement.Log("Find attach parent between " + baseObject.name + " and " + targetObject.name);
            var baseTransform = baseObject;
            bool foundPlatform = false;

            var baseChildrenCount = baseTransform.childCount;
            for (var i = 0; i < baseChildrenCount; i++) {
                var child = baseTransform.GetChild(i);
                var childName = child.gameObject.name;
                if (childName.Contains("[Platform]")) {
                    foundPlatform = true;
                    baseTransform = child;
                    //LinkedMovement.Log("Using Platform");
                    break;
                }
            }
            // TODO: Not sure about this case
            if (!foundPlatform && baseChildrenCount > 0) {
                // Take child at 0
                baseTransform = baseTransform.GetChild(0);
                //LinkedMovement.Log("Using child 0");
            }

            targetObject.SetParent(baseTransform);
        }

        static public List<BlueprintFile> FindDecoBlueprints(IList<BlueprintFile> blueprints) {
            var list = new List<BlueprintFile>();
            foreach (var blueprint in blueprints) {
                if (blueprint.getCategoryTags().Contains("Deco")) {
                    list.Add(blueprint);
                }
            }
            return list;
        }

        static public void UpdateMouseColliders(BuildableObject bo) {
            if (bo.mouseColliders != null) {
                foreach (MouseCollider mouseCollider in bo.mouseColliders)
                    mouseCollider.updatePosition();
            }
        }

        static public string GetGameObjectBuildableName(GameObject go) {
            var buildableObject = GetBuildableObjectFromGameObject(go);
            if (buildableObject != null)
                return buildableObject.getName();
            return go.name;
        }

        static public BuildableObject GetBuildableObjectFromGameObject(GameObject go) {
            var buildableObject = go.GetComponent<BuildableObject>();
            return buildableObject;
        }

        static public PairBase GetPairBaseFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairBase pairBase;
            smb.tryGetCustomData(out pairBase);
            return pairBase;
        }

        static public PairTarget GetPairTargetFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairTarget pairTarget;
            smb.tryGetCustomData(out pairTarget);
            return pairTarget;
        }

        static public void HighlightBuildableObject(BuildableObject bo) {
            if (CurrentHighlightedObject != null) {
                CurrentHighlightedObject.OnKilled -= new SerializedMonoBehaviour.OnKilledHandler(OnHighlightedObjectKilled);
                if (CurrentHightlightCoroutine != null) {
                    CurrentHighlightedObject.StopCoroutine(CurrentHightlightCoroutine);
                }
            }
            CurrentHighlightHandle?.remove();

            CurrentHighlightHandle = HighlightOverlayController.Instance.add(bo.getRenderersToHighlight());
            CurrentHighlightedObject = bo;
            CurrentHighlightedObject.OnKilled += new SerializedMonoBehaviour.OnKilledHandler(OnHighlightedObjectKilled);

            CurrentHightlightCoroutine = ClearHighlightOnBuildableObject(bo);
            bo.StartCoroutine(CurrentHightlightCoroutine);
        }

        static public Sequence BuildAnimationSequence(Transform transform, LMAnimationParams animationParams) {
            LinkedMovement.Log("TAUtils.GetAnimationSequence");

            Sequence sequence = DOTween.Sequence();
            var toTween = DOTween.To(() => transform.position, x => transform.position = x, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration);
            // ease
            // delay
            var fromTween = DOTween.To(() => transform.position, x => transform.position = x, animationParams.startingPosition, animationParams.fromDuration);
            // ease
            // delay
            sequence.Append(toTween);
            sequence.Append(fromTween);

            if (animationParams.isTriggerable) {
                sequence.SetLoops(0);
                sequence.Pause();
            } else {
                sequence.SetLoops(-1);
            }

            return sequence;
        }

        //private void rebuildSequence(bool isSaving = false) {
        //    LinkedMovement.Log("rebuildSequence");
        //    killSequence();

        //    sequence = DOTween.Sequence();
        //    var toTween = DOTween.To(() => baseBO.transform.position, x => baseBO.transform.position = x, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration);
        //    //toTween.SetEase()
        //    // delay
        //    var fromTween = DOTween.To(() => baseBO.transform.position, x => baseBO.transform.position = x, animationParams.startingPosition, animationParams.fromDuration);
        //    //fromTween.SetEase()
        //    // delay
        //    sequence.Append(toTween);
        //    sequence.Append(fromTween);
        //    if (isSaving && animationParams.isTriggerable) {
        //        sequence.SetLoops(0);
        //        sequence.Pause();
        //    } else {
        //        sequence.SetLoops(-1);
        //    }
        //}

        private static void OnHighlightedObjectKilled(SerializedMonoBehaviour smb) {
            CurrentHighlightedObject.OnKilled -= new SerializedMonoBehaviour.OnKilledHandler(OnHighlightedObjectKilled);
            CurrentHighlightedObject = null;
            CurrentHighlightHandle?.remove();
            CurrentHighlightHandle = null;
        }

        private static IEnumerator ClearHighlightOnBuildableObject(BuildableObject bo) {
            yield return new WaitForSecondsRealtime(2f);
            if (bo == CurrentHighlightedObject) {
                OnHighlightedObjectKilled(bo);
                CurrentHightlightCoroutine = null;
            }
        }
    }
}
