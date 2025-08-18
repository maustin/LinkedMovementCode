using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.Utils {
    public class InfoPopper {
        public static void DoInfoPopper() {
            // TODO
            if (Button("ⓘ", RGUIStyle.popupFlatButton, Width(40f))) {
                LinkedMovement.GetController().windowManager.createWindow(WindowManager.WindowType.Information, "This is a test");
            }
        }
    }
}
