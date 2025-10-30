using LinkedMovement.Animation;
using LinkedMovement.UI.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class CreateAnimationStepSubContentNew : IDoGUI {
        private LMController controller;
        private LMAnimationParams animationParams;
        private LMAnimationStep animationStep;
        private int stepIndex;

        // TODO: Cleanup these on UI closed
        private List<Texture2D> customTextures;
        private int numCustomColors = 0;
        private Color[] colors;

        private void startColorPicking() {
            var colorPicker = controller.launchColorPickerWindow(colors, 0);
            colorPicker.OnColorsChanged += newColors => {
                var didChangeColors = false;
                for (int i = 0; i < numCustomColors; i++) {
                    if (colors[i] != newColors[i]) {
                        colors[i] = newColors[i];
                        didChangeColors = true;
                    }
                }
                
                if (didChangeColors) {
                    animationStep.targetColors = new List<Color>(colors);
                    controller.currentAnimationUpdated();
                }
            };
        }

        private void clearColors() {
            colors = animationParams.startingCustomColors.ToArray();
            //animationStep.targetColors = null;
            animationStep.targetColors = new List<Color>(colors);
            controller.currentAnimationUpdated();
        }

        public CreateAnimationStepSubContentNew(LMAnimationParams animationParams, LMAnimationStep animationStep, int stepIndex) {
            controller = LinkedMovement.GetLMController();
            this.animationParams = animationParams;
            this.animationStep = animationStep;
            this.stepIndex = stepIndex;

            var staringCustomColors = animationStep.targetColors;// animationStep.targetColors ?? animationParams.startingCustomColors;
            if (staringCustomColors != null) {
                this.colors = staringCustomColors.ToArray();
                numCustomColors = this.colors.Length;
                customTextures = new List<Texture2D>();
                for (int i = 0; i < numCustomColors; i++) {
                    customTextures.Add(new Texture2D(1, 1));
                }
            }
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    if (Button($"{(animationStep.uiIsOpen ? "▼" : "►")} {(stepIndex + 1).ToString()} : {(animationStep.name == "" ? "Step" : animationStep.name)} ", RGUIStyle.flatButtonLeft)) {
                        animationStep.uiIsOpen = !animationStep.uiIsOpen;
                    }
                    
                    if (Button("↑", RGUIStyle.flatButtonNew, Width(26f))) {
                        LinkedMovement.Log("Move AnimationStep UP");
                        var didChange = animationParams.moveAnimationStepUp(animationStep);
                        if (didChange)
                            controller.currentAnimationUpdated();
                    }
                    if (Button("↓", RGUIStyle.flatButtonNew, Width(26f))) {
                        LinkedMovement.Log("Move AnimationStep DOWN");
                        var didChange = animationParams.moveAnimationStepDown(animationStep);
                        if (didChange)
                            controller.currentAnimationUpdated();
                    }
                    Label("|", RGUIStyle.dimText, Width(3f));
                    if (Button("+Dup", RGUIStyle.flatButtonNew, Width(42f))) {
                        LinkedMovement.Log("Add duplicate step");
                        animationParams.addDuplicateAnimationStep(animationStep);
                        controller.currentAnimationUpdated();
                    }
                    if (Button("+Inv", RGUIStyle.flatButtonNew, Width(40f))) {
                        LinkedMovement.Log("Add inverse step");
                        animationParams.addInverseAnimationStep(animationStep);
                        controller.currentAnimationUpdated();
                    }
                    Label("|", RGUIStyle.dimText, Width(3f));
                    if (Button("✕", RGUIStyle.flatButtonNew, Width(25f))) {
                        LinkedMovement.Log("Delete AnimationStep");
                        animationParams.deleteAnimationStep(animationStep);
                        controller.currentAnimationUpdated();
                    }
                }

                // THIS IS 100% A HACK AND AT THIS POINT I DON'T CARE
                //if (Settings.Instance.uiScale > 1.13f) {
                //    Space(-13f);
                //}

                if (animationStep.uiIsOpen) {
                    using (new GUILayout.VerticalScope(RGUIStyle.animationStep)) {
                        renderStepDetails();
                    }
                }
            }
        }

        private void renderStepDetails() {
            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_STEP_NAME);
                Label("Step name", RGUIStyle.popupTextNew);
                animationStep.name = RGUI.Field(animationStep.name);
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_START_DELAY);
                Label("Start delay", RGUIStyle.popupTextNew);
                var newStartDelay = RGUI.Field(animationStep.startDelay);
                if (animationStep.startDelay != newStartDelay) {
                    animationStep.startDelay = newStartDelay;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_DURATION);
                Label("Duration", RGUIStyle.popupTextNew);
                var newDuration = RGUI.Field(animationStep.duration);
                if (animationStep.duration != newDuration) {
                    animationStep.duration = newDuration;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_EASE);
                Label("Ease", RGUIStyle.popupTextNew);
                var newEase = RGUI.SelectionPopup(animationStep.ease, LMEase.Names);
                if (animationStep.ease != newEase) {
                    animationStep.ease = newEase;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_CHANGE_POSITION);
                Label("Position", RGUIStyle.popupTextNew);
                FlexibleSpace();
                var newPosition = RGUI.Field(animationStep.targetPosition);
                if (!animationStep.targetPosition.Equals(newPosition)) {
                    animationStep.targetPosition = newPosition;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_CHANGE_ROTATION);
                Label("Rotation", RGUIStyle.popupTextNew);
                FlexibleSpace();
                var newRotation = RGUI.Field(animationStep.targetRotation);
                if (!animationStep.targetRotation.Equals(newRotation)) {
                    animationStep.targetRotation = newRotation;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_CHANGE_SCALE);
                Label("Scale", RGUIStyle.popupTextNew);
                FlexibleSpace();
                var newScale = RGUI.Field(animationStep.targetScale);
                if (!animationStep.targetScale.Equals(newScale)) {
                    animationStep.targetScale = newScale;
                    controller.currentAnimationUpdated();
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.TODO);
                Label("Color", RGUIStyle.popupTextNew);
                FlexibleSpace();
                if (colors != null) {
                    for (int i = 0; i < colors.Length; i++) {
                        var color = colors[i];
                        var customTexture = customTextures[i];
                        Label("     ", LMStyles.GetColoredBackgroundLabelStyle(customTexture, color));
                        Label(" ");
                    }

                    using (Scope.GuiEnabled(!controller.colorPickerWindowIsOpen())) {
                        if (Button("Set")) {
                            startColorPicking();
                        }
                        if (Button("Clear")) {
                            clearColors();
                        }
                    }
                }
            }

            using (Scope.Horizontal()) {
                InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_END_DELAY);
                Label("End delay", RGUIStyle.popupTextNew);
                var newEndDelay = RGUI.Field(animationStep.endDelay);
                if (animationStep.endDelay != newEndDelay) {
                    animationStep.endDelay = newEndDelay;
                    controller.currentAnimationUpdated();
                }
            }
        }
    }
}
