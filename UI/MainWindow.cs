// ATTRIB: CheatMod
using UnityEngine;

namespace LinkedMovement.AltUI {
    class MainWindow : BaseWindow {
        // TODO: Get this from elsewhere
        private string[] selectionModes = {"Individual", "Box"};
        private int selectedSelectionMode = 0;
        private Vector2 targetsScrollPosition;

        public MainWindow(LinkedMovementController controller) : base(controller) {
            windowName = "Link Objects";
        }

        public override void drawContent() {
            var baseObject = controller.baseObject;
            var targetObjects = controller.targetObjects;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Base:");
            if (GUILayout.Button("Select", GUILayout.Width(65)))
                controller.pickBaseObject();
            GUILayout.EndHorizontal();

            if (baseObject != null) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(baseObject.getName());
                if (GUILayout.Button("Clear", GUILayout.Width(65)))
                    controller.clearBaseObject();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Targets:");
            if (GUILayout.Button("Select", GUILayout.Width(65)))
                controller.pickTargetObject(selectedSelectionMode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Selection mode:");
            selectedSelectionMode = GUILayout.Toolbar(selectedSelectionMode, selectionModes);
            GUILayout.EndHorizontal();

            targetsScrollPosition = GUILayout.BeginScrollView(targetsScrollPosition);
            foreach (var targetObject in targetObjects) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(targetObject.getName());
                if (GUILayout.Button("Clear", GUILayout.Width(65)))
                    controller.clearTargetObject(targetObject);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (baseObject != null && targetObjects.Count > 0) {
                GUILayout.Space(10f);
                if (GUILayout.Button("Join Objects!"))
                    controller.joinObjects();
            }
        }
    }
}
