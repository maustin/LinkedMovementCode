using RapidGUI;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Utils {
    public class InfoPopper {
        public static void DoInfoPopper(LMStringKey key) {
            if (Button("ⓘ", RGUIStyle.flatButton, Width(15f))) {
                LinkedMovement.GetController().windowManager.createWindow(WindowManager.WindowType.Information, LMStringSystem.GetText(key));
            }
        }
    }
}
