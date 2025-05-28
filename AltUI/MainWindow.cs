// ATTRIB: CheatMod
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LinkedMovement.AltUI {
    class MainWindow : BaseWindow {
        private string[] selectionModeNames = {Selection.Mode.Individual.ToString(), Selection.Mode.Box.ToString()};
        private Selection.Mode[] selectionModes = {Selection.Mode.Individual, Selection.Mode.Box};
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
                controller.pickTargetObject(selectionModes[selectedSelectionMode]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Selection mode:");
            selectedSelectionMode = GUILayout.Toolbar(selectedSelectionMode, selectionModeNames);
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //GUILayout.Label("Blueprint:");
            //if (GUILayout.Button("Choose")) {
            //    var prints = BlueprintManager.Instance.getAllBlueprints();
            //    LinkedMovement.Log("Blueprints! " + prints.Count);

            //    var decoPrints = LinkedMovementController.FindDecoBlueprints(prints);
            //    LinkedMovement.Log("# deco prints: " + decoPrints.Count);
            //    foreach (var decoPrint in decoPrints) {
            //        LinkedMovement.Log(decoPrint.getName());
            //    }
            //}
            //GUILayout.EndHorizontal();

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
