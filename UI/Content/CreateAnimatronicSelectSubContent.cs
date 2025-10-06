using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateAnimatronicSelectSubContent : IDoGUI {
        private LinkedMovementController controller;

        private string[] selectionModeNames = { Selection.Mode.Individual.ToString(), Selection.Mode.Box.ToString() };
        private Selection.Mode[] selectionModes = { Selection.Mode.Individual, Selection.Mode.Box };
        private int selectedSelectionMode = 0;
        private Vector2 targetsScrollPosition;

        public CreateAnimatronicSelectSubContent() {
            controller = LinkedMovement.GetController();
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Select Objects", RGUIStyle.popupTitle);
                Label("Select the objects that will attach to the origin.");
                Space(10f);

                using (Scope.Horizontal()) {
                    Label("Selection mode");
                    selectedSelectionMode = Toolbar(selectedSelectionMode, selectionModeNames);
                }

                using (Scope.Horizontal()) {
                    if (Button("Select")) {
                        controller.pickingTargetObject(selectionModes[selectedSelectionMode]);
                    }
                }

                var targetObjects = controller.targetObjects;
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(300f));
                foreach (var targetObject in targetObjects) {
                    using (Scope.Horizontal()) {
                        Label(targetObject.getName());
                        if (Button("X", Width(40)))
                            controller.queueRemoveTargetBuildableObject(targetObject);
                    }
                }
                EndScrollView();

                FlexibleSpace();
                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(controller.targetObjects.Count > 0)) {
                        if (Button("Remove All")) {
                            controller.queueRemoveAllTargetBuildableObjects();
                        }
                    }
                }
            }
        }
    }
}
