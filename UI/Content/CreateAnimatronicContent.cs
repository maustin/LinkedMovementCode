using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimatronicContent : LMWindowContent {
        private LinkedMovementController controller;

        private IDoGUI selectSubContent;
        private IDoGUI assembleSubContent;
        private IDoGUI animateSubContent;

        public CreateAnimatronicContent() {
            controller = LinkedMovement.GetController();
            controller.setCreationStep(LinkedMovementController.CreationSteps.Select);

            selectSubContent = new CreateAnimatronicSelectSubContent();
            assembleSubContent = new CreateBaseSubContent();
            animateSubContent = new CreateAnimationSubContent();
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.CREATE_NEW_ANIM_NAME);
                    Label("Name");
                    controller.animatronicName = RGUI.Field(controller.animatronicName);
                }

                Space(5f);

                Label((controller.getCreationStep() == LinkedMovementController.CreationSteps.Select ? "> " : "") + "Select Objects");
                Label((controller.getCreationStep() == LinkedMovementController.CreationSteps.Assemble ? "> " : "") + "Origin");
                Label((controller.getCreationStep() == LinkedMovementController.CreationSteps.Animate ? "> " : "") + "Animate");

                Space(5f);
                HorizontalLine.DrawHorizontalLine(Color.grey);
                Space(5f);
            }

            if (controller.getCreationStep() == LinkedMovementController.CreationSteps.Select) {
                selectSubContent.DoGUI();

                FlexibleSpace();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(false)) {
                        Button("< Back", Width(65));
                    }

                    FlexibleSpace();

                    using (Scope.GuiEnabled(controller.targetObjects.Count > 0)) {
                        if (Button("Next >", Width(65))) {
                            controller.setCreationStep(LinkedMovementController.CreationSteps.Assemble);
                        }
                    }
                }
            }
            if (controller.getCreationStep() == LinkedMovementController.CreationSteps.Assemble) {
                assembleSubContent.DoGUI();

                FlexibleSpace();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                using (Scope.Horizontal()) {
                    if (Button("< Back", Width(65))) {
                        controller.setCreationStep(LinkedMovementController.CreationSteps.Select);
                    }

                    FlexibleSpace();

                    using (Scope.GuiEnabled(controller.originObject != null)) {
                        if (Button("Next >", Width(65))) {
                            controller.setCreationStep(LinkedMovementController.CreationSteps.Animate);
                        }
                    }
                }
            }
            if (controller.getCreationStep() == LinkedMovementController.CreationSteps.Animate) {
                GUILayout.Label("Animate", RGUIStyle.popupTitle);
                animateSubContent.DoGUI();

                FlexibleSpace();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                using (Scope.Horizontal()) {
                    if (Button("< Back", Width(65))) {
                        controller.setCreationStep(LinkedMovementController.CreationSteps.Assemble);
                    }

                    FlexibleSpace();

                    var canFinish = controller.animationParams != null && controller.animationParams.animationSteps.Count > 0;
                    using (Scope.GuiEnabled(canFinish)) {
                        if (Button("Finish ✓", Width(65))) {
                            windowManager.removeWindow(window);
                            controller.setCreationStep(LinkedMovementController.CreationSteps.Finish);
                        }
                    }
                }
            }
        }
    }
}
