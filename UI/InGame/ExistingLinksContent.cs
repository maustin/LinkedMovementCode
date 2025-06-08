using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal sealed class ExistingLinksContent : IDoGUI {
        private LinkedMovementController controller;
        private Vector2 scrollPosition;

        private Dictionary<Pairing, string> selectedPairingsAndNames;
        private Dictionary<Pairing, Vector3> selectedPairingsBaseOffset;

        public ExistingLinksContent() {
            controller = LinkedMovement.GetController();
            selectedPairingsAndNames = new Dictionary<Pairing, string>();
            selectedPairingsBaseOffset = new Dictionary<Pairing, Vector3>();
        }

        public void DoGUI() {
            var pairings = controller.getPairings();
            scrollPosition = BeginScrollView(scrollPosition, Height(500f));
            foreach (var pairing in pairings) {
                buildPairingUI(pairing);
            }
            EndScrollView();
        }

        private void buildPairingUI(Pairing pairing) {
            using (Scope.Vertical()) {
                var name = pairing.getPairingName();
                var pairBase = pairing.getExistingPairBase();
                using (Scope.Horizontal()) {
                    if (Button(name, RGUIStyle.flatButtonLeft)) {
                        var alreadyHasPairingNameField = selectedPairingsAndNames.ContainsKey(pairing);
                        if (alreadyHasPairingNameField == true) {
                            // TODO: Make single method?
                            selectedPairingsAndNames.Remove(pairing);
                            selectedPairingsBaseOffset.Remove(pairing);
                        } else {
                            // TODO: Make single method?
                            selectedPairingsAndNames.Add(pairing, name);
                            selectedPairingsBaseOffset.Add(pairing, pairBase.getPositionOffset());
                        }
                    }
                    if (Button("Delete", Width(65))) {
                        pairing.destroy();
                    }
                }
                var hasPairingNameField = selectedPairingsAndNames.ContainsKey(pairing);
                if (hasPairingNameField) {
                    using (Scope.Horizontal()) {
                        var origPairName = selectedPairingsAndNames[pairing];
                        var newPairName = RGUI.Field(origPairName, "Pair Name: ");
                        if (newPairName != origPairName) {
                            LinkedMovement.Log("ExistingLinksContent Update pair name: " + newPairName);
                            selectedPairingsAndNames[pairing] = newPairName;

                            pairing.updatePairingName(newPairName);
                        }
                    }
                    using (Scope.Horizontal()) {
                        Label("Position offset");
                        var existingPositionOffset = selectedPairingsBaseOffset[pairing];
                        var newBasePositionOffset = RGUI.Field(existingPositionOffset);
                        if (!existingPositionOffset.Equals(newBasePositionOffset)) {
                            LinkedMovement.Log("ExistingLinksContent Update base offset");
                            selectedPairingsBaseOffset[pairing] = newBasePositionOffset;
                            pairing.updatePairingBaseOffset(newBasePositionOffset);
                        }
                    }
                    using (Scope.Horizontal()) {
                        Space(10f);
                        var baseName = TAUtils.GetGameObjectBuildableName(pairing.baseGO);
                        if (Button(baseName, RGUIStyle.flatButtonLeft)) {
                            LinkedMovement.Log("ExistingLinksContent Focus base " + baseName);
                            GameController.Instance.cameraController.focusOn(pairing.baseGO.transform.position);
                        }
                    }
                    foreach (var target in pairing.targetGOs) {
                        var targetName = TAUtils.GetGameObjectBuildableName(target);
                        using (Scope.Horizontal()) {
                            Space(20f);
                            Label(targetName);
                        }
                    }
                    using (Scope.Vertical()) {
                        Space(5f);
                        HorizontalLine.DrawHorizontalLine(Color.gray);
                    }
                }
                using (Scope.Vertical()) {
                    Space(5f);
                }
            }
        }
    }
}
