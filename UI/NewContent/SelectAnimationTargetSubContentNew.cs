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
                Label("Select the object that will animate.", RGUIStyle.popupTextNew);

                Space(10f);

                var targetBO = controller.currentAnimation.targetBuildableObject;
                var hasTarget = targetBO != null;

                if (hasTarget) {
                    using (Scope.Horizontal()) {
                        var name = targetBO.getName();
                        Label(name);
                        if (Button("✕", RGUIStyle.roundedFlatButton, Width(40f))) {
                            controller.currentAnimation.removeTarget();
                        }
                    }
                }

                Space(10f);

                using (Scope.GuiEnabled(!hasTarget)) {
                    if (Button("Select Target", RGUIStyle.roundedFlatButton)) {
                        GUI.FocusControl(null);
                        controller.currentAnimation.startPicking();
                    }
                }
            }
        }
    }
}
