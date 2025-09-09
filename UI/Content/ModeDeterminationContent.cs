using LinkedMovement.UI.Utils;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Content {
    internal class ModeDeterminationContent : LMWindowContent {

        public ModeDeterminationContent() {}

        override public void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
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
