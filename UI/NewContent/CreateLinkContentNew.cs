using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    public class CreateLinkContentNew : LMWindowContent {
        private LMController controller;

        private IDoGUI selectParentSubContent;
        private IDoGUI selectTargetsSubContent;

        public CreateLinkContentNew() {
            controller = LinkedMovement.GetLMController();

            selectParentSubContent = new SelectLinkParentSubContentNew();
            selectTargetsSubContent = new SelectLinkTargetsSubContentNew();

            controller.editLink();
        }

        public override void DoGUI() {
            if (controller.currentLink == null) {
                return;
            }

            base.DoGUI();

            using (Scope.Vertical()) {
                // Name
                using (Scope.Horizontal()) {
                    InfoPopper.DoInfoPopper(LMStringKey.TODO);
                    Label("Link name", RGUIStyle.popupTextNew);
                    var newName = RGUI.Field(controller.currentLink.name);
                    if (newName != controller.currentLink.name) {
                        controller.currentLink.name = newName;
                    }
                }

                Space(5f);

                // select parent sub
                selectParentSubContent.DoGUI();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                // select targets sub
                var hasParent = controller.currentLink.hasParent();
                using (Scope.GuiEnabled(hasParent)) {
                    selectTargetsSubContent.DoGUI();
                }

                FlexibleSpace();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                using (Scope.Horizontal()) {
                    var canFinish = controller.currentLink.isValid();
                    FlexibleSpace();
                    using (Scope.GuiEnabled(canFinish)) {
                        if (Button("Save ✓", RGUIStyle.roundedFlatButton, Width(65))) {
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
