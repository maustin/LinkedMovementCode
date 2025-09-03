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

        private bool didSetTargetPairing = false;

        public EditAnimatronicAnimationSubContent(Pairing pairing) {
            controller = LinkedMovement.GetController();
            this.pairing = pairing;
        }

        public void DoGUI() {
            if (!didSetTargetPairing) {
                setTargetPairing();
                didSetTargetPairing = true;
            }

            using (Scope.Vertical()) {
                Label("To Do");
            }
        }

        private void setTargetPairing() {
            controller.setTargetPairing(pairing);
            animationParams = controller.animationParams;
        }
    }
}
