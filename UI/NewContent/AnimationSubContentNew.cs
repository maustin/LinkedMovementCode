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
            // TODO: Info i

            rebuildStepContent();

            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_IS_TRIGGERABLE);
                    Label("Is Triggerable", RGUIStyle.popupTextNew);
                    var newIsTriggerable = RGUI.Field(animationParams.isTriggerable);
                    if (newIsTriggerable != animationParams.isTriggerable) {
                        LinkedMovement.Log("SET isTriggerable");
                        animationParams.isTriggerable = newIsTriggerable;
                        controller.currentAnimationUpdated();
                    }
                }

                using (Scope.Horizontal()) {
                    // TODO: Info
                    Label("Delay animation on park load", RGUIStyle.popupTextNew);
                    var newUseInitialStartDelay = RGUI.Field(animationParams.useInitialStartDelay);
                    if (animationParams.useInitialStartDelay != newUseInitialStartDelay) {
                        LinkedMovement.Log("SET use initial start delay");
                        animationParams.useInitialStartDelay = newUseInitialStartDelay;
                    }
                }

                using (Scope.Horizontal()) {
                    var newInitialDelayMin = RGUI.Field(animationParams.initialStartDelayMin, "Delay time min");
                    var newInitialDelayMax = RGUI.Field(animationParams.initialStartDelayMax, "Delay time max");
                    if (!animationParams.initialStartDelayMin.Equals(newInitialDelayMin) || !animationParams.initialStartDelayMax.Equals(newInitialDelayMax)) {
                        LinkedMovement.Log("SET initial delay range");
                        animationParams.initialStartDelayMin = newInitialDelayMin;
                        animationParams.initialStartDelayMax = newInitialDelayMax;
                    }
                }

                Space(5f);

                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_STEPS_INTRO);
                    Label("Animation Steps", RGUIStyle.popupTextNew);
                }

                if (Button("Add Step", RGUIStyle.roundedFlatButton)) {
                    animationParams.addNewAnimationStep();
                    controller.currentAnimationUpdated();
                }

                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(400f));
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
