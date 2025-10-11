using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal sealed class InfoContent : LMWindowContent {
        private string message;

        public InfoContent(string message) {
            this.message = message;
            this.title = "Info";
        }

        override public void DoGUI() {
            base.DoGUI();
            using (Scope.Vertical()) {
                Label(message, RGUIStyle.infoText);
            }
        }
    }
}
