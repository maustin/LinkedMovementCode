using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private LMAnimationParams animationParams;
        private string[] easeNames;

        public CreateAnimationSubContent() {
            controller = LinkedMovement.GetController();
            easeNames = Enum.GetNames(typeof(LMEase));
        }

        public void DoGUI() {
            // TODO: Info! ⓘ

            animationParams = controller.animationParams;

            using (Scope.Vertical()) {
                GUILayout.Label("Animate", RGUIStyle.popupTitle);

                using (Scope.Horizontal()) {
                    // TODO
                    //InfoPopper.DoInfoPopper();
                    GUILayout.Label("Is Triggerable");
                    var newIsTriggerable = RGUI.Field(animationParams.isTriggerable);
                    if (newIsTriggerable != animationParams.isTriggerable) {
                        LinkedMovement.Log("SET isTriggerable");
                        animationParams.isTriggerable = newIsTriggerable;
                        controller.rebuildSampleSequence();
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
                        controller.rebuildSampleSequence();
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
                        controller.rebuildSampleSequence();
                    }
                }

                // TO Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Animate TO Duration");
                    var newToDuration = RGUI.Field(animationParams.toDuration);
                    if (!animationParams.toDuration.Equals(newToDuration)) {
                        LinkedMovement.Log("SET to duration");
                        animationParams.toDuration = newToDuration;
                        controller.rebuildSampleSequence();
                    }
                }

                // TO Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Animate TO Easing");
                    var newToEasing = RGUI.SelectionPopup(animationParams.toEase, easeNames);
                    if (animationParams.toEase != newToEasing) {
                        LinkedMovement.Log("SET to ease");
                        animationParams.toEase = newToEasing;
                        controller.rebuildSampleSequence();
                    }
                }

                Space(3f);
                HorizontalLine.DrawHorizontalLine(Color.grey);
                Space(5f);

                // FROM Delay
                using (Scope.Horizontal()) {
                    GUILayout.Label("Pause time at target");
                    var newPauseAtTargetDuration = RGUI.Field(animationParams.fromDelay);
                    if (!animationParams.fromDelay.Equals(newPauseAtTargetDuration)) {
                        LinkedMovement.Log("SET from delay");
                        animationParams.fromDelay = newPauseAtTargetDuration;
                        controller.rebuildSampleSequence();
                    }
                }

                // FROM Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return FROM Duration");
                    var newReturnFromDuration = RGUI.Field(animationParams.fromDuration);
                    if (!animationParams.fromDuration.Equals(newReturnFromDuration)) {
                        LinkedMovement.Log("SET from duration");
                        animationParams.fromDuration = newReturnFromDuration;
                        controller.rebuildSampleSequence();
                    }
                }

                // FROM Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return FROM Easing");
                    var newFromEasing = RGUI.SelectionPopup(animationParams.fromEase, easeNames);
                    if (animationParams.fromEase != newFromEasing) {
                        LinkedMovement.Log("SET from ease");
                        animationParams.fromEase = newFromEasing;
                        controller.rebuildSampleSequence();
                    }
                }

                Space(3f);
                HorizontalLine.DrawHorizontalLine(Color.grey);
                Space(5f);

                // Restart delay
                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(!animationParams.isTriggerable)) {
                        GUILayout.Label("Restart animation delay");
                        var newRestartDelay = RGUI.Field(animationParams.restartDelay);
                        if (!animationParams.restartDelay.Equals(newRestartDelay)) {
                            LinkedMovement.Log("SET restart delay");
                            animationParams.restartDelay = newRestartDelay;
                            controller.rebuildSampleSequence();
                        }
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }
    }
}
