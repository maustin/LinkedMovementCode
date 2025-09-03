using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicSelectSubContent : IDoGUI {
        private LinkedMovementController controller;
        private Pairing pairing;

        public EditAnimatronicSelectSubContent(Pairing pairing) {
            controller = LinkedMovement.GetController();
            this.pairing = pairing;
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("To Do");
            }
        }
    }
}
