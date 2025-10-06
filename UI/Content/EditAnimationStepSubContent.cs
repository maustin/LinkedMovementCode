using LinkedMovement.Animation;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class EditAnimationStepSubContent : IDoGUI {
        private LinkedMovementController controller;
        private LMAnimationParams animationParams;
        private LMAnimationStep animationStep;
        private int stepIndex;

        public EditAnimationStepSubContent(LMAnimationParams animationParams, LMAnimationStep animationStep, int stepIndex) {
            controller = LinkedMovement.GetController();
            this.animationParams = animationParams;
            this.animationStep = animationStep;
            this.stepIndex = stepIndex;
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    if (Button($"{(animationStep.uiIsOpen ? "▼" : "►")} {stepIndex.ToString()} : {(animationStep.name == "" ? "Step" : animationStep.name)} ", RGUIStyle.flatButtonLeft)) {
                        animationStep.uiIsOpen = !animationStep.uiIsOpen;
                    }
                    if (Button("↑", Width(25f))) {
                        LinkedMovement.Log("Move AnimationStep UP");
                        var didChange = animationParams.moveAnimationStepUp(animationStep);
                        if (didChange)
                            controller.rebuildSampleSequence();
                    }
                    if (Button("↓", Width(25f))) {
                        LinkedMovement.Log("Move AnimationStep DOWN");
                        var didChange = animationParams.moveAnimationStepDown(animationStep);
                        if (didChange)
                            controller.rebuildSampleSequence();
                    }
                    if (Button("✕", Width(25f))) {
                        LinkedMovement.Log("Delete AnimationStep");
                        animationParams.deleteAnimationStep(animationStep);
                        controller.rebuildSampleSequence();
                    }
                }

                if (animationStep.uiIsOpen) {
                    using (new GUILayout.VerticalScope(RGUIStyle.animationStep)) {
                        doOpenGUI();
                    }
                }

            }

            Space(5f);
        }

        private void doOpenGUI() {
            using (Scope.Horizontal()) {
                Label("Step name");
                animationStep.name = RGUI.Field(animationStep.name);
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Start delay");
                var newStartDelay = RGUI.Field(animationStep.startDelay);
                if (animationStep.startDelay != newStartDelay) {
                    animationStep.startDelay = newStartDelay;
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Duration");
                var newDuration = RGUI.Field(animationStep.duration);
                if (animationStep.duration != newDuration) {
                    animationStep.duration = newDuration;
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Ease");
                var newEase = RGUI.SelectionPopup(animationStep.ease, LMEase.Names);
                if (animationStep.ease != newEase) {
                    animationStep.ease = newEase;
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Position change");
                var newPosition = RGUI.Field(animationStep.targetPosition);
                if (!animationStep.targetPosition.Equals(newPosition)) {
                    animationStep.targetPosition = newPosition;
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Rotation change");
                var newRotation = RGUI.Field(animationStep.targetRotation);
                if (!animationStep.targetRotation.Equals(newRotation)) {
                    animationStep.targetRotation = newRotation;
                    //LinkedMovement.Log("SET rotation: " + newRotation.ToString());
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("Scale change");
                var newScale = RGUI.Field(animationStep.targetScale);
                if (!animationStep.targetScale.Equals(newScale)) {
                    animationStep.targetScale = newScale;
                    controller.rebuildSampleSequence();
                }
            }

            using (Scope.Horizontal()) {
                Space(5f);
                GUILayout.Label("End delay");
                var newEndDelay = RGUI.Field(animationStep.endDelay);
                if (animationStep.endDelay != newEndDelay) {
                    animationStep.endDelay = newEndDelay;
                    controller.rebuildSampleSequence();
                }
            }
        }
    }
}
