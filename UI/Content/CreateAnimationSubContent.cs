using DG.Tweening;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private BuildableObject originBO;
        private LMAnimationParams animationParams;
        private Sequence sequence;

        public CreateAnimationSubContent() {
            controller = LinkedMovement.GetController();
        }

        public void DoGUI() {
            originBO = controller.originObject;
            animationParams = controller.animationParams;

            using (Scope.Vertical()) {
                GUILayout.Label("Animate", RGUIStyle.popupTitle);

                Space(10f);

                using (Scope.Horizontal()) {
                    GUILayout.Label("Is Triggerable");
                    var newIsTriggerable = RGUI.Field(animationParams.isTriggerable);
                    if (newIsTriggerable != animationParams.isTriggerable) {
                        LinkedMovement.Log("SET isTriggerable");
                        animationParams.isTriggerable = newIsTriggerable;
                        rebuildSequence();
                    }
                }

                using (Scope.GuiEnabled(false)) {
                    GUILayout.Label("Origin Position: " + animationParams.startingPosition.ToString());
                }
                using (Scope.Horizontal()) {
                    GUILayout.Label("Target Position Offset");
                    var newOriginPosition = RGUI.Field(animationParams.targetPosition);
                    if (!animationParams.targetPosition.Equals(newOriginPosition)) {
                        LinkedMovement.Log("SET target position");
                        animationParams.targetPosition = newOriginPosition;
                        rebuildSequence();
                    }
                }

                using (Scope.GuiEnabled(false)) {
                    GUILayout.Label("Origin Rotation: " + animationParams.startingRotation.ToString());
                }
                using (Scope.Horizontal()) {
                    GUILayout.Label("Target Rotation Offset");
                    var newOriginRotation = RGUI.Field(animationParams.targetRotation);
                    if (!animationParams.targetRotation.Equals(newOriginRotation)) {
                        LinkedMovement.Log("SET target rotation");
                        animationParams.targetRotation = newOriginRotation;
                        rebuildSequence();
                    }
                }

                // TO Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Animate TO Duration");
                    var newToDuration = RGUI.Field(animationParams.toDuration);
                    if (!animationParams.toDuration.Equals(newToDuration)) {
                        LinkedMovement.Log("SET to duration");
                        animationParams.toDuration = newToDuration;
                        rebuildSequence();
                    }
                }

                // TO Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Animate TO Easing");
                }

                HorizontalLine.DrawHorizontalLine(Color.grey);

                // FROM Delay
                using (Scope.Horizontal()) {
                    GUILayout.Label("Pause time at target");
                    var newPauseAtTargetDuration = RGUI.Field(animationParams.fromDelay);
                    if (!animationParams.fromDelay.Equals(newPauseAtTargetDuration)) {
                        LinkedMovement.Log("SET from delay");
                        animationParams.fromDelay = newPauseAtTargetDuration;
                        rebuildSequence();
                    }
                }

                // FROM Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return FROM Duration");
                    var newReturnFromDuration = RGUI.Field(animationParams.fromDuration);
                    if (!animationParams.fromDuration.Equals(newReturnFromDuration)) {
                        LinkedMovement.Log("SET from duration");
                        animationParams.fromDuration = newReturnFromDuration;
                        rebuildSequence();
                    }
                }

                // FROM Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return FROM Easing");
                }

                // Restart delay
                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(!animationParams.isTriggerable)) {
                        GUILayout.Label("Restart animation delay");
                        var newRestartDelay = RGUI.Field(animationParams.restartDelay);
                        if (!animationParams.restartDelay.Equals(newRestartDelay)) {
                            LinkedMovement.Log("SET restart delay");
                            animationParams.restartDelay = newRestartDelay;
                            rebuildSequence();
                        }
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void killSequence() {
            LinkedMovement.Log("killSequence");
            if (sequence == null) {
                LinkedMovement.Log("No sequence to kill");
                return;
            }

            sequence.Kill();
            sequence = null;

            originBO.transform.position = animationParams.startingPosition;
            originBO.transform.rotation = Quaternion.Euler(animationParams.startingRotation);
        }

        private void rebuildSequence(bool isSaving = false) {
            LinkedMovement.Log("rebuildSequence");
            killSequence();

            if (originBO == null) {
                LinkedMovement.Log("NO ORIGIN BO!");
                return;
            }

            //LinkedMovement.Log(originBO.transform.rotation.ToString());
            //LinkedMovement.Log("start: " + animationParams.startingRotation.ToString());
            //LinkedMovement.Log("target: " + animationParams.targetRotation.ToString());

            //var thing = originBO.transform.DORotate(animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration, RotateMode.FastBeyond360);
            //DOTween.To(() => myQuaternion, x => myQuaternion = x, new Vector3(0, 180, 0), 1).SetOptions(true);
            //var thing = DOTween.To(() => originBO.transform.rotation, value => originBO.transform.rotation = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration).From(false);
            //return;

            sequence = DOTween.Sequence();
            var toPositionTween = DOTween.To(() => originBO.transform.position, value => originBO.transform.position = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration);//.SetEase(Ease)

            var toRotationTween = DOTween.To(() => originBO.transform.rotation, value => originBO.transform.rotation = value, animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration).SetOptions(false);
            //var toRotationTween = originBO.transform.DORotate(animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration, RotateMode.FastBeyond360);

            //var toRotationTween = DOTween.To(() => originBO.transform.eulerAngles, value => originBO.transform.rotation.eulerAngles = value, animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration);
            //var toRotationTween = DOTween.To(() => originBO.transform)
            //var toPositionTween = originBO.transform.DOMove(animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration);
            //var toRotationTween = originBO.transform.DORotate(animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration, RotateMode.FastBeyond360);

            //toPositionTween.SetEase()
            // delay

            var fromPositionTween = DOTween.To(() => originBO.transform.position, value => originBO.transform.position = value, animationParams.startingPosition, animationParams.fromDuration);

            // Rotation set as "From" to support values >= 360
            var fromRotationTween = DOTween.To(() => originBO.transform.rotation, value => originBO.transform.rotation = value, animationParams.startingRotation + animationParams.targetRotation, animationParams.fromDuration).From().SetOptions(false);
            //var fromRotationTween = originBO.transform.DORotate(animationParams.startingRotation, animationParams.fromDuration, RotateMode.FastBeyond360);
            //var fromPositionTween = originBO.transform.DOMove(animationParams.startingPosition, animationParams.fromDuration);
            //var fromRotationTween = originBO.transform.DORotate(animationParams.startingRotation, animationParams.fromDuration, RotateMode.FastBeyond360);
            //fromPositionTween.SetEase()
            // delay

            //return;

            //sequence.Append(toRotationTween);
            sequence.Append(toPositionTween);
            //sequence.Insert(0, toRotationTween);
            sequence.Join(toRotationTween);

            sequence.AppendInterval(animationParams.fromDelay);

            //sequence.Append(fromRotationTween);
            sequence.Append(fromPositionTween);
            //sequence.Insert(animationParams.toDuration, fromRotationTween);
            sequence.Join(fromRotationTween);

            var restartDelay = animationParams.isTriggerable ? 0 : animationParams.restartDelay;
            sequence.AppendInterval(restartDelay);

            sequence.SetLoops(-1);
            //if (isSaving && animationParams.isTriggerable) {
            //    sequence.SetLoops(0);
            //    sequence.Pause();
            //} else {
            //    sequence.SetLoops(-1);
            //}
        }
    }
}
