using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class ExistingAnimatronicsContent : LMWindowContent {
        private LinkedMovementController controller;

        private Vector2 targetsScrollPosition;

        public ExistingAnimatronicsContent() {
            controller = LinkedMovement.GetController();
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                var pairings = controller.getPairings();
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(300f));
                foreach (var pairing in pairings) {
                    using (Scope.Horizontal()) {
                        if (Button(pairing.pairingName, RGUIStyle.flatButtonLeft)) {
                            controller.windowManager.createWindow(WindowManager.WindowType.EditAnimatronic, pairing);
                        }
                        using (Scope.GuiEnabled(false)) {
                            if (Button("X", Width(40))) {
                                controller.confirmDeletePairing(pairing);
                            }
                        }
                    }
            }
                EndScrollView();
            }
        }
    }
}
