using LinkedMovement.Animation;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private Pairing pairing;
        private LMAnimationParams animationParams;

        public EditAnimatronicAnimationSubContent(Pairing pairing) {
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
