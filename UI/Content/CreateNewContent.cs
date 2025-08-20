using LinkedMovement.UI.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class CreateNewContent : LMWindowContent {
        private LinkedMovementController controller;

        private string[] selectionModeNames = { Selection.Mode.Individual.ToString(), Selection.Mode.Box.ToString() };
        private Selection.Mode[] selectionModes = { Selection.Mode.Individual, Selection.Mode.Box };
        private int selectedSelectionMode = 0;
        private Vector2 targetsScrollPosition;

        private Vector3 originPosition = Vector3.zero;
        private Vector3 offsetPosition = Vector3.zero;
        private Vector3 offsetRotation = Vector3.zero;

        private string animatronicName = string.Empty;

        public CreateNewContent() {
            controller = LinkedMovement.GetController();
            title = "Create New Animatronic";
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                // origin position
                showOriginPositionUI();
                Space(10f);

                // offset position
                showOffsetPositionUI();
                Space(10f);

                // offset rotation
                showOffsetRotationUI();
                Space(10f);

                showTargetsSelectUI();
                Space(15f);

                showNameUI();
                Space(10f);

                showNextUI();
            }
        }

        private void showOriginPositionUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Origin");
                    originPosition = RGUI.Field(originPosition);
                }
            }
        }

        private void showOffsetPositionUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Position offset");
                    offsetPosition = RGUI.Field(offsetPosition);
                }
            }
        }

        private void showOffsetRotationUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Rotation offset");
                    offsetRotation = RGUI.Field(offsetRotation);
                }
            }
        }

        private void showTargetsSelectUI() {
            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Target Objects");
                    if (Button("Select", Width(64))) {
                        controller.pickingTargetObject(selectionModes[selectedSelectionMode]);
                    }
                }

                using (Scope.Horizontal()) {
                    Label("Selection mode");
                    selectedSelectionMode = Toolbar(selectedSelectionMode, selectionModeNames);
                }

                var targetObjects = controller.targetObjects;
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, GUILayout.Height(300f));
                foreach (var targetObject in targetObjects) {
                    using (Scope.Horizontal()) {
                        Label(targetObject.getName());
                        //if (Button("Clear", Width(65)))
                            //controller.removeTargetBuildableObject(targetObject);
                    }
                }
                EndScrollView();
            }
        }

        private void showNameUI() {
            using (Scope.Horizontal()) {
                //controller.pairName = RGUI.Field(controller.pairName, "Pair name:");
            }
        }

        private void showNextUI() {
            //
        }
    }
}
