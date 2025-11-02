using UnityEngine;

namespace RapidGUI
{
    public static class RGUIStyle
    {
        public static GUIStyle flatButton;
        public static GUIStyle flatButtonLeft;
        public static GUIStyle popupFlatButton;
        public static GUIStyle popupFlatButtonSelected;
        public static GUIStyle popup;
        public static GUIStyle popupTitle;
        public static GUIStyle darkWindow;
        public static GUIStyle alignLeftBox;

        public static GUIStyle warningLabel;
        public static GUIStyle warningLabelNoStyle;

        public static GUIStyle animationStep;
        public static GUIStyle dimText;
        public static GUIStyle infoText;

        // NEW
        public static GUIStyle popupWindowNew;
        public static GUIStyle popupWindowTitleNew;
        public static GUIStyle popupWindowContentNew;
        public static GUIStyle popupWindowContentInnerNew;
        public static GUIStyle popupTextNew;
        public static GUIStyle closeWindowButton;
        public static GUIStyle roundedFlatButton;
        public static GUIStyle roundedFlatButtonLeft;
        public static GUIStyle infoPopperButtonNew;
        public static GUIStyle scrollBackground;
        public static GUIStyle scrollThumb;
        public static GUIStyle fieldLabel;
        public static GUIStyle flatButtonLeftNew;

        // GUIStyleState.background will be null 
        // if it set after secound scene load and don't use a few frame
        // to keep textures, set it to other member. at unity2019
        private static Texture2D flatButtonTex;
        private static Texture2D flatButtonLeftTex;
        private static Texture2D popupTex;
        private static Texture2D popupTitleTex;
        private static Texture2D darkWindowTexNormal;
        private static Texture2D darkWindowTexOnNormal;
        private static Texture2D animationStepTex;

        // NEW
        private static Texture2D popupTextureNew;
        private static Texture2D popupWindowContentTextureNew;
        private static Texture2D popupWindowContentInnerTextureNew;
        private static Texture2D closeWindowButtonTexture;
        private static Texture2D roundedFlatButtonWhiteTexture;
        private static Texture2D roundedFlatButtonOffWhiteTexture;
        private static Texture2D scrollBackgroundTexture;
        private static Texture2D scrollThumbNormalTexture;
        private static Texture2D scrollThumbDownTexture;
        private static Texture2D flatButtonLeftTextureNew;


        static RGUIStyle()
        {
            CreateStyles();
        }

        public static void CreateStyles()
        {
            CreateFlatButton();
            CreateFlatButtonLeft();
            CreatePopupFlatButton();
            CreatePopupFlatButtonSelected();
            CreatePopup();
            CreatePopupTitle();
            CreateDarkWindow();
            CreateAlignLeftBox();
            CreateWarningLabel();
            CreateWarningLabelNoStyle();
            CreateAnimationStep();
            CreateDimText();
            CreateInfoText();

            // NEW
            CreatePopupWindowNew();
            CreatePopupWindowTitleNew();
            CreatePopupWindowContentNew();
            CreatePopupWindowContentInnerNew();
            CreatePopupTextNew();
            CreateCloseWindowButton();
            CreateRoundedFlatButton();
            CreateRoundedFlatButtonLeft();
            CreateInfoPopperNew();
            CreateScrollBackground();
            CreateScrollThumb();
            CreateFieldLabel();
            CreateFlatButtonLeftNew();
        }

        static void CreateFlatButton()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                wordWrap = false, 
                alignment = TextAnchor.MiddleCenter
            };

            var toggle = GUI.skin.toggle;
            style.normal.textColor = toggle.normal.textColor;
            style.hover.textColor = toggle.hover.textColor;

            flatButtonTex = new Texture2D(1, 1);
            flatButtonTex.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
            flatButtonTex.Apply();
            style.hover.background = flatButtonTex;

            style.name = nameof(flatButton);
            flatButton = style;
        }

        static void CreateRoundedFlatButton() {
            var style = new GUIStyle(GUI.skin.button);
            //var style = new GUIStyle(GUI.skin.label);

            style.wordWrap = false;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = style.hover.textColor = style.active.textColor = new Color(0.2f, 0.2f, 0.2f);

            roundedFlatButtonWhiteTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.BUTTON_NORMAL);
            style.normal.background = style.hover.background = roundedFlatButtonWhiteTexture;
            roundedFlatButtonOffWhiteTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.BUTTON_DOWN);
            style.active.background = roundedFlatButtonOffWhiteTexture;

            style.border = new RectOffset(3, 3, 3, 3);
            
            style.name = nameof(roundedFlatButton);
            roundedFlatButton = style;
        }

        static void CreateRoundedFlatButtonLeft() {
            var style = new GUIStyle(roundedFlatButton);
            style.alignment= TextAnchor.MiddleLeft;
            var oldPadding = style.padding;
            style.padding = new RectOffset(oldPadding.left + 10, oldPadding.right, oldPadding.top, oldPadding.bottom);

            style.name = nameof(roundedFlatButtonLeft);
            roundedFlatButtonLeft = style;
        }

        static void CreateCloseWindowButton() {
            var style = new GUIStyle(GUI.skin.label) {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            style.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            style.hover.textColor = new Color(1f, 1f, 1f);

            closeWindowButtonTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.EXIT_BUTTON);
            style.border = new RectOffset(3, 3, 3, 3);
            style.normal.background = style.hover.background = closeWindowButtonTexture;

            style.name = nameof(closeWindowButton);
            closeWindowButton = style;
        }

        static void CreateInfoPopperNew() {
            var style = new GUIStyle(GUI.skin.label) {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
            };

            style.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
            style.hover.textColor = new Color(0.27f, 0.27f, 0.27f);

            style.name = nameof(infoPopperButtonNew);
            infoPopperButtonNew = style;
        }

        static void CreateDimText() {
            var style = new GUIStyle(GUI.skin.label) {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
            };

            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

            style.name = nameof(dimText);
            dimText = style;
        }

        static void CreatePopupTextNew() {
            var style = new GUIStyle(GUI.skin.label);
            style.wordWrap = false;

            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = style.hover.textColor = new Color(0.2f, 0.2f, 0.2f);

            style.name = nameof(popupTextNew);
            popupTextNew = style;
        }

        static void CreateFieldLabel() {
            var style = new GUIStyle(GUI.skin.label);

            style.normal.textColor = new Color(0.2f, 0.2f, 0.2f);

            style.name = nameof(fieldLabel);
            fieldLabel = style;
        }

        static void CreateInfoText() {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            style.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
            
            style.name = nameof(infoText);
            infoText = style;
        }

        static void CreateFlatButtonLeft() {
            var style = new GUIStyle(GUI.skin.label) {
                wordWrap = false,
                alignment = TextAnchor.MiddleLeft
            };

            var toggle = GUI.skin.toggle;
            style.normal.textColor = toggle.normal.textColor;
            style.hover.textColor = toggle.hover.textColor;

            flatButtonLeftTex = new Texture2D(1, 1);
            flatButtonLeftTex.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
            flatButtonLeftTex.Apply();
            style.hover.background = flatButtonLeftTex;

            style.name = nameof(flatButtonLeft);
            flatButtonLeft = style;
        }

        static void CreateFlatButtonLeftNew() {
            var style = new GUIStyle(GUI.skin.label);
            style.wordWrap = false;
            style.alignment = TextAnchor.MiddleLeft;

            style.normal.textColor = style.hover.textColor = new Color(0.2f, 0.2f, 0.2f);

            flatButtonLeftTextureNew = new Texture2D(1, 1);
            flatButtonLeftTextureNew.SetPixel(0, 0, new Color(0.83f, 0.83f, 0.83f));
            flatButtonLeftTextureNew.Apply();
            style.hover.background = flatButtonLeftTextureNew;

            style.name = nameof(flatButtonLeftNew);
            flatButtonLeftNew = style;
        }

        static void CreatePopupFlatButton()
        {
            var style = new GUIStyle(flatButton)
            {
                alignment = GUI.skin.label.alignment,
                padding = new RectOffset(24, 48, 2, 2),
                name = nameof(popupFlatButton)
            };

            popupFlatButton = style;
        }

        static void CreatePopupFlatButtonSelected() {
            var style = new GUIStyle(flatButton) {
                alignment = GUI.skin.label.alignment,
                padding = new RectOffset(24, 48, 2, 2),
                fontStyle = FontStyle.Bold,
                name = nameof(popupFlatButtonSelected),
            };
            style.normal.textColor = Color.yellow;

            popupFlatButtonSelected = style;
        }

        static void CreateAnimationStep() {
            var style = new GUIStyle(GUI.skin.box);
            
            animationStepTex = new Texture2D(1, 1);
            animationStepTex.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f));
            animationStepTex.Apply();

            style.normal.background = animationStepTex;

            style.name = nameof(animationStep);
            animationStep = style;
        }

        static void CreatePopup()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset()
            };

            popupTex = new Texture2D(1, 1);
            var brightness = 0.207f;
            popupTex.SetPixel(0, 0, new Color(brightness, brightness, brightness));
            popupTex.Apply();

            style.normal.background = style.hover.background = popupTex;

            style.name = nameof(popup);
            popup = style;
        }

        static void CreatePopupTitle() {
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            
            popupTitleTex = new Texture2D(1, 1);
            var bgB = 0.238f;
            popupTitleTex.SetPixel(0, 0, new Color(bgB, bgB, bgB));
            popupTitleTex.Apply();

            var textB = 0.129f;
            style.normal.textColor = new Color(textB, textB, textB);
            style.normal.background = popupTitleTex;

            popupTitle = style;
        }

        static void CreatePopupWindowNew() {
            var style = new GUIStyle(GUI.skin.window);

            popupTextureNew = new Texture2D(1, 1);
            var colorValue = 0.70f;
            popupTextureNew.SetPixel(0, 0, new Color(colorValue, colorValue, colorValue));
            popupTextureNew.Apply();

            style.normal.background = popupTextureNew;
            style.onNormal.background = popupTextureNew;

            style.name = nameof(popupWindowNew);
            popupWindowNew = style;
        }

        static void CreatePopupWindowTitleNew() {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;

            var textColorVal = 0.36f;
            style.normal.textColor = new Color(textColorVal, textColorVal, textColorVal);

            style.name = nameof(popupWindowTitleNew);
            popupWindowTitleNew = style;
        }

        static void CreatePopupWindowContentNew() {
            var style = new GUIStyle(GUI.skin.box);
            style.padding = new RectOffset(0, 0, 10, 0);
            style.margin = new RectOffset();
            style.border = new RectOffset(-10, -10, 0, -10);
            
            var bgValue = 0.60f;
            popupWindowContentTextureNew = new Texture2D(1, 1);
            popupWindowContentTextureNew.SetPixel(0, 0, new Color(bgValue, bgValue, bgValue));
            popupWindowContentTextureNew.Apply();

            style.normal.background = popupWindowContentTextureNew;

            style.name = nameof(popupWindowContentTextureNew);
            popupWindowContentNew = style;
        }

        static void CreatePopupWindowContentInnerNew() {
            var style = new GUIStyle(GUI.skin.box);
            style.padding = new RectOffset(5, 5, 5, 5);
            style.margin = new RectOffset(0, 0, 5, 0);
            
            popupWindowContentInnerTextureNew = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.POPUP_CONTENT);
            style.border = new RectOffset(3, 3, 3, 3);

            style.normal.background = popupWindowContentInnerTextureNew;

            style.name = nameof(popupWindowContentInnerNew);
            popupWindowContentInnerNew = style;
        }

        static void CreateDarkWindow()
        {
            var style = new GUIStyle(GUI.skin.window);

            style.normal.background = darkWindowTexNormal = CreateTexDark(style.normal.background, 0.6f, 2f);
            style.onNormal.background = darkWindowTexOnNormal = CreateTexDark(style.onNormal.background, 0.5f, 1.9f);

            style.name = nameof(darkWindow);

            darkWindow = style;
        }

        static void CreateAlignLeftBox()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                name = nameof(alignLeftBox)
            };

            alignLeftBox = style;
        }

        static Texture2D CreateTexDark(Texture2D src, float colorRate, float alphaRate)
        {
            LinkedMovement.LinkedMovement.Log($"RGUIStyle.CreateTexDark w: {src.width}, h: {src.height}");
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


            var pixels = dst.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
            {
                var col = pixels[i];
                col.r *= colorRate;
                col.g *= colorRate;
                col.b *= colorRate;
                col.a *= alphaRate;

                pixels[i] = col;
            }

            dst.SetPixels(pixels);
            dst.Apply();

            return dst;
        }


        static void CreateWarningLabel()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = GUI.skin.label.alignment,
                richText = true, 
                name = nameof(warningLabel)
            };

            warningLabel = style;
        }

        static void CreateWarningLabelNoStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                name = nameof(warningLabelNoStyle)
            };

            warningLabelNoStyle = style;
        }

        static void CreateScrollBackground() {
            var style = new GUIStyle(GUI.skin.verticalScrollbar);

            scrollBackgroundTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.SCROLL_BACKGROUND);
            style.border = new RectOffset(3, 3, 3, 3);
            style.normal.background = scrollBackgroundTexture;

            style.name = nameof(scrollBackground);
            scrollBackground = style;
        }

        static void CreateScrollThumb() {
            var style = new GUIStyle(GUI.skin.verticalScrollbarThumb);

            scrollThumbNormalTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.SCROLL_THUMB_NORMAL);
            scrollThumbDownTexture = GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES.SCROLL_THUMB_DOWN);
            style.border = new RectOffset(3, 3, 3, 3);
            style.normal.background = style.hover.background = scrollThumbNormalTexture;
            style.active.background = scrollThumbDownTexture;

            style.name = nameof(scrollThumb);
            scrollThumb = style;
        }

        static Texture2D GetRoundedRectTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES looseTextureType) {
            var texture = LinkedMovement.LinkedMovement.GetLooseTexture(looseTextureType);
            if (texture == null) texture = CreateDefaultRoundedTexture(looseTextureType);
            return texture;
        }

        static Texture2D CreateDefaultRoundedTexture(LinkedMovement.LinkedMovement.LOOSE_TEXTURES looseTextureType) {
            int width = 12;
            int height = 12;
            int radius = 3;

            var color = LinkedMovement.LinkedMovement.GetLooseTextureDefaultColor(looseTextureType);

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (IsInsideRoundedRect(x, y, width, height, radius)) {
                        pixels[y * width + x] = color;
                    } else {
                        pixels[y * width + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius) {
            // Check if in corner regions
            if (x < radius && y < radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
            if (x >= width - radius && y < radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, radius)) <= radius;
            if (x < radius && y >= height - radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius - 1)) <= radius;
            if (x >= width - radius && y >= height - radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, height - radius - 1)) <= radius;

            return true; // Inside the rectangle body
        }
    }
}
