// ATTRIB: HideScenery
using UnityEngine;

namespace LinkedMovement.UI {
    static class Tooltip {
        private static readonly Vector2 Offset = new Vector2(10.0f, 13.0f);

        private static GUIStyle style;
        private static GUIStyle GetStyle() {
            if (style is null) {
                var s = new GUIStyle(GUI.skin.box);

                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.13f, 0.13f, 0.13f, 1.0f));
                tex.Apply();

                s.normal.background = tex;
                s.normal.textColor = Color.white;

                s.wordWrap = true;
                s.alignment = TextAnchor.UpperLeft;

                style = s;
            }

            return style;
        }

        public static void Collect() {
            if (Event.current.type == EventType.Repaint) {
                content.text = GUI.tooltip;
            }
        }

        private static readonly GUIContent content = new GUIContent();
        /// <summary>
        /// Must be called in `OnGUI`.
        /// Only call in one place -- NOT multiple places!
        /// </summary>
        public static void Show() {
            var evt = Event.current;
            if (!string.IsNullOrWhiteSpace(content.text)) {
                var style = GetStyle();
                var rect = GUILayoutUtility.GetRect(content, style, GUILayout.MaxWidth(175.0f));
                var pos = evt.mousePosition + Offset;
                // adjust when out of screen (top, bottom)
                if (pos.x + rect.width > Screen.width) {
                    pos.x -= (pos.x + rect.width) - Screen.width;
                }
                if (pos.y + rect.height > Screen.height) {
                    pos.y -= (pos.y + rect.height) - Screen.height;
                }
                rect.position = pos;

                GUI.Box(rect, content, style);
            }
        }
    }
}
