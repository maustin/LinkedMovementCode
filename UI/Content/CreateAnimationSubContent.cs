using DG.Tweening;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private BuildableObject originBO;
        private LMAnimationParams animationParams;
        private Sequence sequence;
        private string[] easeNames;

        public CreateAnimationSubContent() {
            controller = LinkedMovement.GetController();
            easeNames = Enum.GetNames(typeof(LMEase));
        }

        public void DoGUI() {
            originBO = controller.originObject;
            animationParams = controller.animationParams;

            using (Scope.Vertical()) {
                GUILayout.Label("Animate", RGUIStyle.popupTitle);

                Space(5f);

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
                    var newToEasing = RGUI.SelectionPopup(animationParams.toEase, easeNames);
                    if (animationParams.toEase != newToEasing) {
                        LinkedMovement.Log("SET to ease");
                        animationParams.toEase = newToEasing;
                        rebuildSequence();
                    }
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
                    var newFromEasing = RGUI.SelectionPopup(animationParams.fromEase, easeNames);
                    if (animationParams.fromEase != newFromEasing) {
                        LinkedMovement.Log("SET from ease");
                        animationParams.fromEase = newFromEasing;
                        rebuildSequence();
                    }
                }

                HorizontalLine.DrawHorizontalLine(Color.grey);

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

            sequence = DOTween.Sequence();

            var toPositionTween = DOTween.To(() => originBO.transform.position, value => originBO.transform.position = value, animationParams.startingPosition + animationParams.targetPosition, animationParams.toDuration).SetEase(toEase);
            var toRotationTween = DOTween.To(() => originBO.transform.rotation, value => originBO.transform.rotation = value, animationParams.startingRotation + animationParams.targetRotation, animationParams.toDuration).SetOptions(false).SetEase(toEase);
            
            var fromPositionTween = DOTween.To(() => originBO.transform.position, value => originBO.transform.position = value, animationParams.startingPosition, animationParams.fromDuration).SetEase(fromEase);
            // Rotation set as "From" to support values >= 360
            var fromRotationTween = DOTween.To(() => originBO.transform.rotation, value => originBO.transform.rotation = value, animationParams.startingRotation + animationParams.targetRotation, animationParams.fromDuration).From().SetOptions(false).SetEase(fromEase);
            
            sequence.Append(toPositionTween);
            sequence.Join(toRotationTween);

            sequence.AppendInterval(animationParams.fromDelay);

            sequence.Append(fromPositionTween);
            sequence.Join(fromRotationTween);

            var restartDelay = animationParams.isTriggerable ? 0 : animationParams.restartDelay;
            sequence.AppendInterval(restartDelay);

            // TODO: Ability to set loops for triggered?
            sequence.SetLoops(-1);
        }
    }
}
