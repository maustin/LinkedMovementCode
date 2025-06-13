using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal sealed class InfoContent : IDoGUI {
        private string message;
        public InfoContent(string message) {
            this.message = message;
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                Label("Animatronitect - Info", RGUIStyle.popupTitle);
                Space(10f);
                Label(message);
            }
        }
    }
}
