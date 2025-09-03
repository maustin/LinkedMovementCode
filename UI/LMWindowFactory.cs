using LinkedMovement.UI.Content;
using LinkedMovement.UI.InGame;
using System;
using UnityEngine;

namespace LinkedMovement.UI {
    internal static class LMWindowFactory {
        private const int VERTICAL_PADDING = 75;
        private const int HORIZONTAL_PADDING = 100;

        public static LMWindow BuildWindow(WindowManager.WindowType type, object data, WindowManager windowManager) {
            LinkedMovement.Log("LMWindowFactory build " + type.ToString());
            //title, position, always render, data, content
            string title = "";
            Vector2 position = Vector2.zero;
            LMWindowContent content = null;
            bool alwaysRender = false;
            int width = 0;
            int fixedHeight = 0;
            bool allowMultiple = false;

            switch(type) {
                case WindowManager.WindowType.ModeDetermination:
                    width = 300;
                    position = getWindowPositionCenter(width, 100);
                    content = new ModeDeterminationContent();
                    break;
                case WindowManager.WindowType.CreateNewAnimatronic:
                    title = "Create Animatronic";
                    width = 450;
                    position = getWindowPositionRight(width);
                    content = new CreateAnimatronicContent();
                    break;
                case WindowManager.WindowType.ShowExistingAnimatronics:
                    title = "Existing Animatronics";
                    width = 400;
                    fixedHeight = 500;
                    position = getWindowPositionCenter(width, fixedHeight);
                    content = new ExistingAnimatronicsContent();
                    break;
                case WindowManager.WindowType.EditAnimatronic:
                    var pairing = data as Pairing;
                    // TODO: Throw if pairing null
                    title = "Edit Animatronic: " + pairing.getPairingName();
                    width = 450;
                    fixedHeight = 500;
                    //position = getWindowPositionCenter(width, fixedHeight);
                    position = getWindowPositionRight(width);
                    //alwaysRender = true;
                    //allowMultiple = true;
                    content = new EditAnimatronicContent(pairing);
                    break;
                case WindowManager.WindowType.EditAnimation:
                    title = "Edit Animation";
                    width = 400;
                    fixedHeight = 500;
                    position = getWindowPositionCenter(width, (int)(fixedHeight * 0.5));
                    //alwaysRender = true;
                    //allowMultiple = false;
                    //content = 
                    break;
                case WindowManager.WindowType.Information:
                    title = "Information";
                    width = 300;
                    position = getWindowPositionCenter(width, 75);
                    alwaysRender = true;
                    content = new InfoContent(data as string);
                    allowMultiple = true;
                    break;
                case WindowManager.WindowType.Error:
                    title = "Error";
                    width = 300;
                    position = getWindowPositionCenter(width, 75);
                    alwaysRender = true;
                    content = new InfoContent(data as string);
                    allowMultiple = true;
                    break;
                case WindowManager.WindowType.ConfirmAction:
                    // TODO: Confirmation window
                    break;
                default:
                    throw new Exception("Unknown window type");
            }

            if (!allowMultiple && windowManager.hasWindowOfType(type)) {
                LinkedMovement.Log("Window type " + type.ToString() + " already exists and multiple not allowed.");
                return null;
            }

            title = title.Length == 0 ? "Animatronitect" : title;

            var lmWindow = new LMWindow(type, title, content, alwaysRender, width);
            lmWindow.Configure(position, fixedHeight, windowManager);
            return lmWindow;
        }

        private static Vector2 getWindowPositionCenter(int width, int height) {
            //LinkedMovement.Log($"getWindowPositionCenter width: {width.ToString()}, height: {height.ToString()}");
            var positionX = Screen.width * 0.5f / Settings.Instance.uiScale - width * 0.5f;
            var positionY = Screen.height * 0.5f / Settings.Instance.uiScale - height * 0.5f;
            //LinkedMovement.Log($"position x: {positionX.ToString()}, y: {positionY.ToString()}");
            return new Vector2 { x = positionX, y = positionY };
        }

        private static Vector2 getWindowPositionRight(int width) {
            //LinkedMovement.Log($"getWindowPositionRight width: {width.ToString()}");
            var positionX = Screen.width / Settings.Instance.uiScale - width - HORIZONTAL_PADDING / Settings.Instance.uiScale;
            var positionY = VERTICAL_PADDING / Settings.Instance.uiScale;
            //LinkedMovement.Log($"position x: {positionX.ToString()}, y: {positionY.ToString()}");
            return new Vector2 { x = positionX,y = positionY };
        }

        private static float getUIScaledValue(float value) {
            return value / Settings.Instance.uiScale;
        }
    }
}
