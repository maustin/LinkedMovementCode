// ATTRIB: CheatMod
using UnityEngine;

namespace LinkedMovement.AltUI {
    class MainWindow : BaseWindow {
        public MainWindow(LinkedMovementController controller) : base(controller) {
            windowName = "Link Objects";
        }

        public override void drawContent() {
            bool baseIsSet = controller.baseObject != null;
            bool targetIsSet = controller.targetObject != null;

            if (GUILayout.Button("Select Base object"))
                controller.pickBaseObject();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Base object:");
            GUILayout.Label(baseIsSet ? controller.baseObject.getName() : "[unset]");
            if (baseIsSet) {
                if (GUILayout.Button("Clear"))
                    controller.clearBaseObject();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            if (GUILayout.Button("Select Target object"))
                controller.pickTargetObject();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target object:");
            GUILayout.Label(targetIsSet ? controller.targetObject.getName() : "[unset]");
            if (targetIsSet) {
                if (GUILayout.Button("Clear"))
                    controller.clearTargetObject();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            if (baseIsSet && targetIsSet) {
                if (GUILayout.Button("Join Objects!"))
                    controller.joinObjects();
            }
        }
    }
}
