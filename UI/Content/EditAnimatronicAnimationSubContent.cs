using LinkedMovement.Animation;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicAnimationSubContent : IDoGUI {
        private LinkedMovementController controller;
        private Pairing pairing;
        private IDoGUI animateSubContent;

        private bool didSetTargetPairing = false;

        public EditAnimatronicAnimationSubContent(Pairing pairing) {
            controller = LinkedMovement.GetController();
            this.pairing = pairing;
            animateSubContent = new CreateAnimationSubContent();
        }

        public void DoGUI() {
            if (!didSetTargetPairing) {
                setTargetPairing();
                didSetTargetPairing = true;
            }

            animateSubContent.DoGUI();
        }

        private void setTargetPairing() {
            controller.setTargetPairing(pairing);
        }
    }
}
