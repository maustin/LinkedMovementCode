using LinkedMovement.Animation;
using LinkedMovement.Links;
using LinkedMovement.UI.Content;
using LinkedMovement.UI.NewContent;
using System;
using UnityEngine;

namespace LinkedMovement.UI {
    internal static class LMWindowFactory {
        private const int VERTICAL_PADDING = 25;
        private const int HORIZONTAL_PADDING = 100;
        private const int EXISTING_WINDOW_OFFSET = 15;

        public static LMWindow BuildWindow(WindowManager.WindowType type, object data, WindowManager windowManager) {
            LMLogger.Debug("LMWindowFactory build " + type.ToString());
            //title, position, always render, data, content
            string title = "";
            Vector2 position = Vector2.zero;
            LMWindowContent content = null;
            bool alwaysRender = false;
            int width = 0;
            int fixedHeight = 0;
            bool allowMultiple = false;

            switch(type) {
                case WindowManager.WindowType.ModeDeterminationNew:
                    title = "Animate Things!";
                    width = 300;
                    position = getWindowPositionCenter(width, 300);
                    content = new ModeDeterminationContentNew();
                    break;
                case WindowManager.WindowType.ViewAnimationsNew:
                    title = "Animations";
                    width = 400;
                    fixedHeight = 400;
                    position = getWindowPositionCenter(width, fixedHeight);
                    content = new ViewExistingAnimationsContentNew();
                    break;
                case WindowManager.WindowType.CreateAnimationNew:
                    title = "Create Animation";
                    width = 450;
                    position = getWindowPositionRight(width);
                    content = new CreateAnimationContentNew();
                    break;
                case WindowManager.WindowType.EditAnimationNew:
                    title = "Edit Animation";
                    width = 450;
                    position = getWindowPositionRight(width);
                    content = new EditAnimationContentNew(data as LMAnimation);
                    break;
                case WindowManager.WindowType.ViewLinksNew:
                    title = "Links";
                    width = 400;
                    fixedHeight = 400;
                    position = getWindowPositionCenter(width, fixedHeight);
                    content = new ViewExistingLinksContentNew();
                    break;
                case WindowManager.WindowType.CreateLinkNew:
                    title = "Create Link";
                    width = 450;
                    position = getWindowPositionRight(width);
                    content = new CreateLinkContentNew();
                    break;
                case WindowManager.WindowType.EditLinkNew:
                    title = "Edit Link";
                    width = 450;
                    position = getWindowPositionRight(width);
                    content = new EditLinkContentNew(data as LMLink);
                    break;
                //
                //
                //
                case WindowManager.WindowType.Information:
                    title = "Information";
                    width = 500;
                    position = getWindowPositionCenter(width, 75);
                    //position = modifyPositionByOffset(position, windowManager.getNumberOfWindowsOfType(WindowManager.WindowType.Information));
                    position = modifyPositionByOffset(windowManager, position, WindowManager.WindowType.Information, WindowManager.WindowType.Error);
                    alwaysRender = true;
                    content = new InfoContent(data as string);
                    allowMultiple = true;
                    break;
                case WindowManager.WindowType.Error:
                    //title = "Animate Things - Error";
                    //var message = data as string;
                    //Notification nf = new Notification(title, message);
                    //NotificationBar.Instance.addNotification(nf);


                    title = "Error";
                    width = 500;
                    position = getWindowPositionCenter(width, 75);
                    //position = modifyPositionByOffset(position, windowManager.getNumberOfWindowsOfType(WindowManager.WindowType.Error));
                    position = modifyPositionByOffset(windowManager, position, WindowManager.WindowType.Information, WindowManager.WindowType.Error);
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
                LMLogger.Debug("Window type " + type.ToString() + " already exists and multiple not allowed.");
                return null;
            }

            title = title.Length == 0 ? "Animate Things" : title;

            var lmWindow = new LMWindow(type, title, content, alwaysRender, width);
            lmWindow.Configure(position, fixedHeight, windowManager);
            return lmWindow;
        }

        private static Vector2 getWindowPositionCenter(int width, int height) {
            //LinkedMovement.Log($"getWindowPositionCenter width: {width.ToString()}, height: {height.ToString()}");
            //var positionX = Screen.width * 0.5f / Settings.Instance.uiScale - width * 0.5f;
            //var positionY = Screen.height * 0.5f / Settings.Instance.uiScale - height * 0.5f;
            var positionX = Screen.width * 0.5f / Settings.Instance.uiScale - width * 0.5f / Settings.Instance.uiScale;
            var positionY = Screen.height * 0.5f / Settings.Instance.uiScale - height * 0.5f / Settings.Instance.uiScale;
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

        private static Vector2 modifyPositionByOffset(WindowManager windowManager, Vector2 position, params WindowManager.WindowType[] checkTypes) {
            var offsetMultiplier = 0;
            foreach (var type in checkTypes) {
                var numWindowsOfType = windowManager.getNumberOfWindowsOfType(type);
                offsetMultiplier += numWindowsOfType;
            }

            return modifyPositionByOffset(position, offsetMultiplier);
        }

        private static Vector2 modifyPositionByOffset(Vector2 position, int offsetMultiplier) {
            var offsetAmount = offsetMultiplier * EXISTING_WINDOW_OFFSET * Settings.Instance.uiScale;
            //LinkedMovement.Log($"!!! offsetModifer: {offsetMultiplier.ToString()}, offsetAmount: {offsetAmount.ToString()}");
            return new Vector2(position.x + offsetAmount,position.y + offsetAmount);
        }

        //private static float getUIScaledValue(float value) {
        //    // Shouldn't this be *?
        //    return value / Settings.Instance.uiScale;
        //}
    }
}
