using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateBaseSubContent : IDoGUI {
        private LinkedMovementController controller;

        public CreateBaseSubContent() {
            controller = LinkedMovement.GetController();
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Origin", RGUIStyle.popupTitle);
                Label("Select or Generate the object that the target objects will animate with.");

                Space(10f);

                var originObject = controller.originObject;
                bool hasBase = originObject != null;

                if (hasBase) {
                    using (Scope.Horizontal()) {
                        Label(originObject.getName());
                        if (Button("X", Width(40))) {
                            controller.removeOrigin();
                        }
                    }

                    var newOriginPosition = RGUI.Field(controller.originPosition, "Position");
                    if (newOriginPosition != controller.originPosition) {
                        controller.originPosition = newOriginPosition;
                    }
                }

                Space(10f);

                using (Scope.GuiEnabled(!hasBase)) {
                    if (Button("Generate Origin")) {
                        controller.generateOrigin();
                    }
                }

                Space(5f);

                using (Scope.GuiEnabled(!hasBase)) {
                    if (Button("Select Origin")) {
                        controller.pickingOriginObject();
                    }
                }
            }
        }
    }
}
