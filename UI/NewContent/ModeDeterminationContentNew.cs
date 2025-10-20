using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class ModeDeterminationContentNew : LMWindowContent {
        public ModeDeterminationContentNew() {}

        override public void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                Label("Animation & Links UI");
                Space(3f);
                if (Button("Create Animation")) {
                    LinkedMovement.Log("Clicked Create Animation");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.CreateAnimationNew, null);
                }

                Space(3f);

                using (Scope.GuiEnabled(false)) {
                    if (Button("Edit Animation")) {
                        LinkedMovement.Log("Clicked Edit Animation");
                        windowManager.removeWindow(this.window);
                        windowManager.createWindow(WindowManager.WindowType.EditAnimationNew, null);
                    }
                }

                Space(15f);

                if (Button("Create Link")) {
                    LinkedMovement.Log("Clicked Create Link");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.CreateLinkNew, null);
                }

                Space(3f);

                using (Scope.GuiEnabled(false)) {
                    if (Button("Edit Link")) {
                        LinkedMovement.Log("Clicked Edit Link");
                        windowManager.removeWindow(this.window);
                        windowManager.createWindow(WindowManager.WindowType.EditLinkNew, null);
                    }
                }

                Space(5f);
                HorizontalLine.DrawHorizontalLine(Color.grey);
                Space(5f);
                Label("OLD target-origin UI");
                Space(3f);

                if (Button("Create New Animatronic")) {
                    LinkedMovement.Log("Clicked Create New");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.CreateNewAnimatronic, null);
                }

                Space(10f);

                if (Button("View Existing Animatronics")) {
                    LinkedMovement.Log("Clicked View Existing");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.ShowExistingAnimatronics, null);
                }
            }
        }
    }
}
