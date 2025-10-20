using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class SelectLinkTargetsSubContentNew : IDoGUI {
        private LMController controller;

        private string[] selectionModeNames = { Selection.Mode.Individual.ToString(), Selection.Mode.Box.ToString() };
        private Selection.Mode[] selectionModes = { Selection.Mode.Individual, Selection.Mode.Box };
        private int selectedSelectionMode = 0;
        private Vector2 targetsScrollPosition;

        public SelectLinkTargetsSubContentNew() {
            controller = LinkedMovement.GetLMController();
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Select the objects to attach to the parent.");

                Space(10f);

                using (Scope.Horizontal()) {
                    Label("Selection mode");
                    var newSelectionMod = Toolbar(selectedSelectionMode, selectionModeNames);
                    if (newSelectionMod != selectedSelectionMode) {
                        selectedSelectionMode = newSelectionMod;
                        controller.currentLink.stopPicking();
                    }
                }

                using (Scope.Horizontal()) {
                    string selectButtonText = string.Empty;
                    if (selectionModes[selectedSelectionMode] == Selection.Mode.Individual)
                        selectButtonText = "Select Child";
                    else if (selectionModes[selectedSelectionMode] == Selection.Mode.Box)
                        selectButtonText = "Select Children";

                    if (Button(selectButtonText)) {
                        controller.currentLink.startPickingTargets(selectionModes[selectedSelectionMode]);
                    }
                }

                var targetObjects = controller.currentLink.getTargetBuildableObjects();
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(300f));
                foreach (var targetObject in targetObjects) {
                    using (Scope.Horizontal()) {
                        Label(targetObject.getName());
                        if (Button("✕", Width(40))) {
                            controller.currentLink.removeSingleTargetObject(targetObject);
                        }
                    }
                }
                EndScrollView();

                FlexibleSpace();
                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(targetObjects != null && targetObjects.Count > 0)) {
                        if (Button("Remove All Children")) {
                            controller.currentLink.removeAllTargetObjects();
                        }
                    }
                }
            }
        }
    }
}
