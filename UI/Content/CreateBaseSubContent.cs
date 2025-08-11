using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using System.Collections.Generic;
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
                GUILayout.Label("Assemble", RGUIStyle.popupTitle);

                Space(10f);

                bool hasBase = controller.originObject != null;
                string generateButtonLabel = hasBase ? "Re-Generate Origin" : "Generate Origin";
                if (Button(generateButtonLabel)) {
                    controller.generateOrigin();
                }

                Space(10f);

                if (controller.originObject != null) {
                    var newOriginPosition = RGUI.Field(controller.originPosition, "Position");
                    if (newOriginPosition != controller.originPosition) {
                        controller.originPosition = newOriginPosition;
                    }
                    //Label("Rotation");

                    //Space(10f);
                    //Label("Offset Position");
                    //Label("Offset Rotation");
                }
            }
        }
    }
}
