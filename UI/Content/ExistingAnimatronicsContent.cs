using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
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
                    var depth = LMUtils.GetPairingDepth(pairing);
                    using (Scope.Horizontal()) {
                        string prefix = depth > 0 ? (new string('-', depth) + " ") : "";
                        if (Button(prefix + pairing.pairingName, RGUIStyle.flatButtonLeft)) {
                            windowManager.removeWindow(this.window);
                            windowManager.createWindow(WindowManager.WindowType.EditAnimatronic, pairing);
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
