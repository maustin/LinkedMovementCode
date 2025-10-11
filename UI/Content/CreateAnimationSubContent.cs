using LinkedMovement.UI.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private LMAnimationParams animationParams;
        private Vector2 targetsScrollPosition;
        private List<EditAnimationStepSubContent> animationStepContents;
        private long lastAnimationStepsUpdate = 0;

        public CreateAnimationSubContent() {
            controller = LinkedMovement.GetController();
        }

        public void DoGUI() {
            // TODO: Info! ⓘ

            if (animationParams == null) {
                animationParams = controller.animationParams;
            }

            rebuildStepContents();

            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    // TODO
                    InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_IS_TRIGGERABLE);
                    GUILayout.Label("Is Triggerable");
                    var newIsTriggerable = RGUI.Field(animationParams.isTriggerable);
                    if (newIsTriggerable != animationParams.isTriggerable) {
                        LinkedMovement.Log("SET isTriggerable");
                        animationParams.isTriggerable = newIsTriggerable;
                        controller.rebuildSampleSequence();
                    }
                }

                //using (Scope.GuiEnabled(!animationParams.isTriggerable)) {
                using (Scope.GuiEnabled(false)) {
                    using (Scope.Horizontal()) {
                        GUILayout.Label("Delay animation on park load");
                        var newUseInitialStartDelay = RGUI.Field(animationParams.useInitialStartDelay);
                        if (animationParams.useInitialStartDelay != newUseInitialStartDelay) {
                            LinkedMovement.Log("SET use initial start delay");
                            animationParams.useInitialStartDelay = newUseInitialStartDelay;
                        }
                    }
                }

                //using (Scope.GuiEnabled(!animationParams.isTriggerable && animationParams.useInitialStartDelay)) {
                using (Scope.GuiEnabled(false)) {
                    using (Scope.Horizontal()) {
                        var newInitialDelayMin = RGUI.Field(animationParams.initialStartDelayMin, "Delay time min");
                        var newInitialDelayMax = RGUI.Field(animationParams.initialStartDelayMax, "Delay time max");
                        if (!animationParams.initialStartDelayMin.Equals(newInitialDelayMin) || !animationParams.initialStartDelayMax.Equals(newInitialDelayMax)) {
                            LinkedMovement.Log("SET initial delay range");
                            animationParams.initialStartDelayMin = newInitialDelayMin;
                            animationParams.initialStartDelayMax = newInitialDelayMax;
                        }
                    }
                }

                Space(5f);

                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.ANIMATE_STEPS_INTRO);
                    GUILayout.Label("Animation Steps");
                }

                if (Button("Add Step")) {
                    animationParams.addNewAnimationStep();
                    controller.rebuildSampleSequence();
                }

                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(400f));
                foreach (var animationStepContent in animationStepContents) {
                    animationStepContent.DoGUI();
                }
                EndScrollView();

                GUILayout.FlexibleSpace();
            }
        }

        private void rebuildStepContents() {
            if (animationParams.timeOfLastStepsUpdate > lastAnimationStepsUpdate) {
                animationStepContents = new List<EditAnimationStepSubContent>();

                var numSteps = animationParams.animationSteps.Count;
                for (int i = 0; i < numSteps; i++) {
                    animationStepContents.Add(new EditAnimationStepSubContent(animationParams, animationParams.animationSteps[i], i + 1));
                }

                lastAnimationStepsUpdate = animationParams.timeOfLastStepsUpdate;
            }
        }
    }
}
