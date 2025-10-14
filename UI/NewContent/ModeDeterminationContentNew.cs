using LinkedMovement.UI.Utils;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class ModeDeterminationContentNew : LMWindowContent {
        public ModeDeterminationContentNew() {}

        override public void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                if (Button("Create Animation")) {
                    LinkedMovement.Log("Clicked Create Animation");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.CreateAnimationNew, null);
                }

                Space(3f);

                if (Button("Edit Animation")) {
                    LinkedMovement.Log("Clicked Edit Animation");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.EditAnimationNew, null);
                }

                Space(15f);

                if (Button("Create Link")) {
                    LinkedMovement.Log("Clicked Create Link");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.CreateLinkNew, null);
                }

                Space(3f);

                if (Button("Edit Link")) {
                    LinkedMovement.Log("Clicked Edit Link");
                    windowManager.removeWindow(this.window);
                    windowManager.createWindow(WindowManager.WindowType.EditLinkNew, null);
                }
            }
        }
    }
}
