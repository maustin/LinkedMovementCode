using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class SelectLinkParentSubContentNew : IDoGUI {
        private LMController controller;

        public SelectLinkParentSubContentNew() {
            controller = LinkedMovement.GetLMController();
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Select the parent object that the targets will attach to.");

                Space(10f);

                var parentBO = controller.currentLink.getParentBuildableObject();
                var hasParent = parentBO != null;

                if (hasParent) {
                    using (Scope.Horizontal()) {
                        var name = parentBO.getName();
                        Label(name);
                        if (Button("✕", Width(40f))) {
                            controller.currentLink.removeParentObject();
                        }
                    }
                }

                Space(10f);

                using (Scope.GuiEnabled(!hasParent)) {
                    if (Button("Select Parent")) {
                        controller.enableObjectPicker(LMController.PickerMode.LINK_PARENT, Selection.Mode.Individual);
                    }
                }
            }
        }
    }
}
