using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI.Utils {
    public static class Styles {
        public static GUIStyle DarkWindowTitleBarStyle;
        private static void InitializeDarkWindowTitleBarStyle() {
            var style = new GUIStyle(GUI.skin.box);

            style.normal.background = CreateTexDarkTitleBar(RGUIStyle.darkWindowTexNormal);
            style.normal.textColor = RGUIStyle.darkWindow.normal.textColor;
            style.name = "DarkWindowTitleBar";
            style.alignment = TextAnchor.MiddleCenter;

            DarkWindowTitleBarStyle = style;
        }
        public static GUIStyle DarkWindowTitleBarMinimizedStyle;
        private static void InitializeDarkWindowTitleBarMinimizedStyle() {
            var style = new GUIStyle(DarkWindowTitleBarStyle);

            style.alignment = TextAnchor.MiddleLeft;
            style.contentOffset = new Vector2(5.0f, 0.0f);
            style.name = "DarkWindowTitleBarMinimized";

            DarkWindowTitleBarMinimizedStyle = style;
        }

        static Styles() {
            InitializeDarkWindowTitleBarStyle();
            InitializeDarkWindowTitleBarMinimizedStyle();
        }

        private static Texture2D CreateTexDarkTitleBar(Texture2D src) {
            // based on `RGUIStyle.CreateTexDark`

            // copy texture trick.
            // Graphics.CopyTexture(src, dst) must same format src and dst.
            // but src format can't call GetPixels().
            var tmp = RenderTexture.GetTemporary(src.width, src.height);
            Graphics.Blit(src, tmp);

            var prev = RenderTexture.active;
            RenderTexture.active = prev;

            var dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            dst.ReadPixels(new Rect(0f, 0f, src.width, src.height), 0, 0);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tmp);

            var pixels = dst.GetPixels32();
            var srcLine = dst.height / 2;
            var srcStart = srcLine * dst.width;
            // go line by line
            // tex is upside-down
            for (var l = srcLine - 1; l >= 0; l--) {
                var lStart = l * dst.width;
                // row by row
                for (var r = 0; r < dst.width; r++) {
                    pixels[lStart + r] = pixels[srcStart + r];
                }
            }

            dst.SetPixels32(pixels);
            dst.Apply();

            return dst;
        }
    }
}
