using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal sealed class MainContent : IDoGUI {
        private LinkedMovementController controller;

        private string[] selectionModeNames = { Selection.Mode.Individual.ToString(), Selection.Mode.Box.ToString() };
        private Selection.Mode[] selectionModes = { Selection.Mode.Individual, Selection.Mode.Box };
        private int selectedSelectionMode = 0;
        private Vector2 targetsScrollPosition;
        private string selectedBlueprintName;

        private Vector3 basePositionOffset;
        private Vector3 baseRotationOffset;

        static private string[] GetBlueprintNames(List<BlueprintFile> blueprints) {
            var names = new List<string>();
            foreach (BlueprintFile blueprint in blueprints) {
                names.Add(blueprint.getName());
            }
            names.Sort();
            return names.ToArray();
        }

        static private BlueprintFile GetBlueprintWithName(List<BlueprintFile> blueprints, string blueprintName) {
            foreach (BlueprintFile blueprint in blueprints) {
                if (blueprint.getName().Equals(blueprintName))
                    return blueprint;
            }
            LinkedMovement.Log("ERROR: MainContent.GetBlueprint failed to find blueprint with name " + blueprintName);
            return null;
        }

        public MainContent() {
            controller = LinkedMovement.GetController();
        }

        // TODO: Possibly should call controller methods after UI update has completed (e.g. call later)

        public void DoGUI() {
            LinkedMovement.Log("MainContent DoGUI");
            if (controller == null) {
                LinkedMovement.Log("NO CONTROLLER SET!");
                return;
            }

            if (!LinkedMovement.GetController().basePositionOffset.Equals(basePositionOffset)) {
                LinkedMovement.GetController().setBasePositionOffset(basePositionOffset);
            }
            if (!LinkedMovement.GetController().baseRotationOffset.Equals(baseRotationOffset)) {
                LinkedMovement.GetController().setBaseRotationOffset(baseRotationOffset);
            }

            using (Scope.Vertical()) {
                Space(10f);
                ShowBaseSelect();
                Space(20f);
                ShowTargetsSelect();
                Space(20f);
                ShowJoin();
            }
        }

        private void ShowBaseSelect() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Base");
                    if (Button("Select", Width(65))) {
                        controller.pickBaseObject();
                    }
                }

                using (Scope.Horizontal()) {
                    Label("Position offset");
                    basePositionOffset = RGUI.Field(basePositionOffset);
                }

                using (Scope.Horizontal()) {
                    Label("Rotatoin offset");
                    baseRotationOffset = RGUI.Field(baseRotationOffset);
                }

                var baseObject = controller.baseObject;
                if (baseObject != null) {
                    using (Scope.Horizontal()) {
                        Label(baseObject.getName());
                        if (Button("Clear", Width(65)))
                            controller.clearBaseObject();
                    }
                }
            }
        }

        private void ShowTargetsSelect() {
            bool hasSelectedBlueprint = selectedBlueprintName != null;
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Targets");
                    using (Scope.GuiEnabled(!hasSelectedBlueprint)) {
                        if (Button("Select", Width(65))) {
                            controller.pickTargetObject(selectionModes[selectedSelectionMode]);
                        }
                    }
                }

                using (Scope.Horizontal()) {
                    using (Scope.GuiEnabled(!hasSelectedBlueprint)) {
                        Label("Selection mode");
                        selectedSelectionMode = Toolbar(selectedSelectionMode, selectionModeNames);
                    }
                }

                // TODO: Calculate less often
                var prints = BlueprintManager.Instance.getAllBlueprints();
                var decoPrints = LinkedMovementController.FindDecoBlueprints(prints);
                var decoPrintNames = GetBlueprintNames(decoPrints);

                using (Scope.Horizontal()) {
                    if (hasSelectedBlueprint && controller.selectedBlueprint?.getName() != selectedBlueprintName) {
                        controller.setSelectedBlueprint(GetBlueprintWithName(decoPrints, selectedBlueprintName));
                        selectedBlueprintName = null;
                    }
                    Label("Blueprint");
                    selectedBlueprintName = RGUI.SelectionPopup(controller.selectedBlueprint?.getName(), decoPrintNames);
                }

                var targetObjects = controller.targetObjects;
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, GUILayout.Height(300f));
                foreach (var targetObject in targetObjects) {
                    using (Scope.Horizontal()) {
                        Label(targetObject.getName());
                        if (Button("Clear", Width(65)))
                            controller.clearTargetObject(targetObject);
                    }
                }
                EndScrollView();
            }
        }

        private void ShowJoin() {
            using (Scope.Vertical()) {
                var showJoin = controller.baseObject != null && (controller.targetObjects.Count > 0 || controller.selectedBlueprint != null);
                var showClearAll = controller.baseObject != null || controller.targetObjects.Count > 0 || controller.selectedBlueprint != null;

                using (Scope.GuiEnabled(showJoin)) {
                    if (Button("Join Objects!")) {
                        selectedBlueprintName = null;
                        basePositionOffset = Vector3.zero;
                        baseRotationOffset = Vector3.zero;
                        controller.joinObjects();
                    }
                }
                
                using (Scope.GuiEnabled(showClearAll)) {
                    if (Button("Clear All")) {
                        selectedBlueprintName = null;
                        basePositionOffset = Vector3.zero;
                        baseRotationOffset = Vector3.zero;
                        controller.clearAllSelections();
                    }
                }
            }
        }
    }
}
