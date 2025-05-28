// ATTRIB: HideScenery
using UnityEngine;

namespace LinkedMovement.UI {
    class Indicator {
        public static void Show() {
            var c = new GUIContent("<size=8><color=brown><b>HS</b></color></size>");
            var size = GUI.skin.label.CalcSize(c);
            var pos = new Rect(Screen.width - 1.0f - size.x, -8.0f, size.x, size.y);
            GUI.Label(pos, c);
        }
    }
}
