using LinkedMovement.UI.Utils;
using RapidGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal sealed class ExistingLinksContent : IDoGUI {
        private LinkedMovementController controller;
        private Vector2 scrollPosition;

        public ExistingLinksContent() {
            controller = LinkedMovement.GetController();
        }

        public void DoGUI() {
            var pairings = controller.getPairings();
            scrollPosition = BeginScrollView(scrollPosition, GUILayout.Height(500f));
            foreach (var pairing in pairings) {
                buildPairingUI(pairing);
            }
            EndScrollView();
        }

        private void buildPairingUI(Pairing pairing) {
            using (Scope.Vertical()) {
                var name = pairing.getPairingName();
                using (Scope.Horizontal()) {
                    GUILayout.Label(name);
                }
                using (Scope.Horizontal()) {
                    GUILayout.Space(10f);
                    if (GUILayout.Button(pairing.baseGO.name, RGUIStyle.flatButtonLeft)) {
                        LinkedMovement.Log("Focus base " + pairing.baseGO.name);
                        GameController.Instance.cameraController.focusOn(pairing.baseGO.transform.position);
                    }
                }
                foreach (var target in pairing.targetGOs) {
                    using (Scope.Horizontal()) {
                        GUILayout.Space(20f);
                        GUILayout.Label(target.name);
                    }
                }
                using (Scope.Vertical()) {
                    GUILayout.Space(5f);
                }
            }
        }
    }
}
