using LinkedMovement.Animation;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicContent : LMWindowContent {
        private LinkedMovementController controller;

        private IDoGUI selectSubContent;
        private IDoGUI animateSubContent;

        private Pairing pairing;
        private LMAnimationParams animationParams;
        private bool selectUIOpen = false;
        private bool animateUIOpen = false;

        public EditAnimatronicContent(Pairing pairing) {
            controller = LinkedMovement.GetController();
            this.pairing = pairing;
            this.animationParams = pairing.pairBase.animParams;

            foreach (var step in this.animationParams.animationSteps) {
                step.uiIsOpen = false;
            }

            selectSubContent = new EditAnimatronicSelectSubContent(pairing);
            animateSubContent = new EditAnimatronicAnimationSubContent(pairing);
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                using (Scope.Horizontal()) {
                    Label("Name");
                    var newPairingName = RGUI.Field(pairing.pairingName);
                    if (newPairingName != pairing.pairingName) {
                        pairing.pairingName = newPairingName;
                        pairing.pairBase.pairName = newPairingName;
                        title = "Edit Animatronic: " + newPairingName;
                    }
                }
                Space(5f);

                using (Scope.Horizontal()) {
                    if (Button($"{(selectUIOpen ? "▼" : "►")} Targets", RGUIStyle.flatButtonLeft)) {
                        selectUIOpen = !selectUIOpen;
                    }
                }
                if (selectUIOpen) {
                    selectSubContent.DoGUI();
                }
                using (Scope.Horizontal()) {
                    if (Button($"{(animateUIOpen ? "▼" : "►")} Animation", RGUIStyle.flatButtonLeft)) {
                        animateUIOpen = !animateUIOpen;
                    }
                }
                if (animateUIOpen) {
                    animateSubContent.DoGUI();
                }
            }
        }
    }
}
