using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class SelectAnimationTargetSubContentNew : IDoGUI {
        private LMController controller;

        public SelectAnimationTargetSubContentNew() {
            controller = LinkedMovement.GetLMController();
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Select the object that will animate.");

                Space(10f);

                var targetObject = controller.currentAnimation.targetGameObject;
                var hasTarget = targetObject != null;

                if (hasTarget) {
                    using (Scope.Horizontal()) {
                        Label(targetObject.name);
                        if (Button("X", Width(40f))) {
                            controller.currentAnimation.removeTarget();
                        }

                        // TODO Offset?
                    }
                }

                Space(10f);

                using (Scope.GuiEnabled(!hasTarget)) {
                    if (Button("Select Target")) {
                        controller.enableObjectPicker();
                    }
                }
            }
        }
    }
}
