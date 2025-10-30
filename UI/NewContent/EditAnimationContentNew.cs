using LinkedMovement.Animation;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class EditAnimationContentNew : LMWindowContent {
        private LMController controller;

        private IDoGUI animateSubContent;

        public EditAnimationContentNew(LMAnimation animation) {
            controller = LinkedMovement.GetLMController();

            controller.editAnimation(animation);
            animateSubContent = new CreateAnimationSubContentNew();
        }

        public override void DoGUI() {
            if (controller.currentAnimation == null) {
                return;
            }

            base.DoGUI();

            using (Scope.Vertical()) {
                // Name
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.CREATE_NEW_ANIM_NAME);
                    Label("Animation name", RGUIStyle.popupTextNew);
                    var newName = RGUI.Field(controller.currentAnimation.name);
                    if (newName != controller.currentAnimation.name) {
                        controller.currentAnimation.name = newName;
                    }
                }

                Space(5f);

                HorizontalLine.DrawHorizontalLine(Color.grey);
                animateSubContent.DoGUI();

                FlexibleSpace();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                using (Scope.Horizontal()) {
                    var canFinish = controller.currentAnimation.isValid();
                    FlexibleSpace();
                    using (Scope.GuiEnabled(canFinish)) {
                        if (Button("Save ✓", Width(65))) {
                            controller.commitEdit();

                            // TODO: Can this call be moved to LMWindowContent?
                            windowManager.removeWindow(window);
                        }
                    }
                }
            }
        }
    }
}
