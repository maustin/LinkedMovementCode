using LinkedMovement.UI.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class AnimationSubContentNew : IDoGUI {
        private LMController controller;
        private LMAnimationParams animationParams;
        private Vector2 targetsScrollPosition;
        private List<AnimationStepSubContentNew> animationStepContents;
        private long lastAnimationStepsUpdate = 0;

        public AnimationSubContentNew() {
            controller = LinkedMovement.GetLMController();

            var animation = controller.currentAnimation;
            animationParams = animation.getAnimationParams();
        }

        public void DoGUI() {
            rebuildStepContent();

            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIM_IS_TRIGGERABLE);
                    Label("Is Triggerable", RGUIStyle.popupTextNew);
                    var newIsTriggerable = RGUI.Field(animationParams.isTriggerable);
                    if (newIsTriggerable != animationParams.isTriggerable) {
                        LMLogger.Debug("SET isTriggerable");
                        animationParams.isTriggerable = newIsTriggerable;
                        controller.currentAnimationUpdated();
                    }
                }

                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIM_DELAY_ON_PARK_LOAD);
                    Label("Delay animation on park load", RGUIStyle.popupTextNew);
                    var newUseInitialStartDelay = RGUI.Field(animationParams.useInitialStartDelay);
                    if (animationParams.useInitialStartDelay != newUseInitialStartDelay) {
                        LMLogger.Debug("SET use initial start delay");
                        animationParams.useInitialStartDelay = newUseInitialStartDelay;
                    }
                }

                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(animationParams.useInitialStartDelay)) {
                        InfoPopper.DoInfoPopper(LMStringKey.ANIM_DELAY_ON_PARK_LOAD_MINMAX);
                        var newInitialDelayMin = RGUI.Field(animationParams.initialStartDelayMin, "Delay time min");
                        var newInitialDelayMax = RGUI.Field(animationParams.initialStartDelayMax, "Delay time max");
                        if (!animationParams.initialStartDelayMin.Equals(newInitialDelayMin) || !animationParams.initialStartDelayMax.Equals(newInitialDelayMax)) {
                            LMLogger.Debug("SET initial delay range");
                            animationParams.initialStartDelayMin = newInitialDelayMin;
                            animationParams.initialStartDelayMax = newInitialDelayMax;
                        }
                    }
                }

                Space(5f);

                var animationLength = animationParams.getAnimationLength();
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIM_STEPS);
                    Label($"Animation Steps (Current animation length {animationLength.ToString("F2")} sec)", RGUIStyle.popupTextNew);
                }

                //var currentAnimation = controller.currentAnimation;
                //var currentAnimationProgress = currentAnimation.sequence.progress;
                //using (Scope.GuiEnabled(false)) {
                //    RGUI.Slider(currentAnimationProgress, 0, 1, "Animation progress");
                //}
                //using (Scope.Horizontal()) {
                //    Label("Animation Progress: ", RGUIStyle.popupTextNew);
                //    GUILayout.HorizontalSlider(currentAnimationProgress, 0f, 1f);
                //}

                if (Button("Add Step", RGUIStyle.roundedFlatButton)) {
                    animationParams.addNewAnimationStep();
                    controller.currentAnimationUpdated();
                }

                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(600));
                foreach (var animationStepContent in animationStepContents) {
                    animationStepContent.DoGUI();
                }
                EndScrollView();

                GUILayout.FlexibleSpace();
            }
        }

        private void rebuildStepContent() {
            if (animationParams.timeOfLastStepsUpdate > lastAnimationStepsUpdate) {
                animationStepContents = new List<AnimationStepSubContentNew>();

                var numSteps = animationParams.animationSteps.Count;
                for (int i = 0; i < numSteps; i++) {
                    animationStepContents.Add(new AnimationStepSubContentNew(animationParams, animationParams.animationSteps[i], i));
                }

                lastAnimationStepsUpdate = animationParams.timeOfLastStepsUpdate;
            }
        }
    }
}
