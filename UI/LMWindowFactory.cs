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
                    width = 400;
                    position = getWindowPositionRight(width);
                    content = new CreateAnimatronicContent();
                    break;
                case WindowManager.WindowType.ShowExistingAnimatronics:
                    title = "Existing Animatronics";
                    width = 400;
                    fixedHeight = 500;
                    position = getWindowPositionCenter(width, (int)(fixedHeight * 0.5));
                    content = new ExistingAnimatronicsContent();
                    break;
                case WindowManager.WindowType.EditAnimatronic:
                    title = "Edit Animatronic";
                    width = 400;
                    position = getWindowPositionCenter(width, 300);
                    alwaysRender = true;
                    allowMultiple = true;
                    //content = 
                    break;
                case WindowManager.WindowType.EditAnimation:
                    title = "Edit Animation";
                    width = 400;
                    fixedHeight = 500;
                    position = getWindowPositionCenter(width, (int)(fixedHeight * 0.5));
                    alwaysRender = true;
                    allowMultiple = true;
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

            title = title.Length == 0 ? "Animatronitect" : "Animatronitect : " + title;

            var lmWindow = new LMWindow(type, title, content, alwaysRender, width);
            lmWindow.Configure(position, fixedHeight, windowManager);
            return lmWindow;
        }

        private static Vector2 getWindowPositionCenter(int width, int height) {
            return new Vector2(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f);
        }

        private static Vector2 getWindowPositionRight(int width) {
            return new Vector2(Screen.width - HORIZONTAL_PADDING - width, VERTICAL_PADDING);
        }
    }
}

//public void showMainWindow() {
//    if (mainWindow == null) {
//        LinkedMovement.Log("WindowManager Show Main Window");
//        var width = 400f;
//        mainWindow = new WindowLauncher("Animatronitect - Link Objects", width);
//        mainWindow.rect.position = new Vector2(Screen.width - 200.0f - width, 75.0f);
//        var mainContent = new MainContent();
//        mainWindow.Add(mainContent.DoGUI);
//        mainWindow.Open();
//        mainWindow.onClose += (WindowLauncher launcher) => mainWindow = null;
//    }
//}

//public void showInfoWindow(string message)
//{
//    LinkedMovement.Log("WindowManager Show info " + message);

//    if (infoWindow != null)
//    {
//        infoWindow.Close();
//    }

//    var width = 300f;
//    infoWindow = new WindowLauncher("Animatronitect - Info", width);
//    infoWindow.rect.position = new Vector2(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f);
//    var infoContent = new InfoContent(message);
//    infoWindow.Add(infoContent.DoGUI);
//    infoWindow.Open();
//    infoWindow.onClose += (WindowLauncher launcher) => infoWindow = null;
//}

//public void showExistingLinksWindow() {
//    if (existingLinksWindow == null) {
//        LinkedMovement.Log("WindowManager Show existing links");
//        var width = 400f;
//        existingLinksWindow = new WindowLauncher("Animatronitect - Existing Links", width);
//        existingLinksWindow.SetHeight(500f);
//        existingLinksWindow.rect.position = new Vector2(Screen.width - 400.0f - width, 175.0f);
//        var existingPairsContent = new ExistingLinksContent();
//        existingLinksWindow.Add(existingPairsContent.DoGUI);
//        existingLinksWindow.Open();
//        existingLinksWindow.onClose += (WindowLauncher launcher) => existingLinksWindow = null;
//    }
//}

//public void showCreateAnimationWindow() {
//    LinkedMovement.Log("WindowManagwer Show create animation");
//    if (createAnimationWindow != null) {
//        createAnimationWindow.Close();
//    }

//    var width = 400f;
//    createAnimationWindow = new WindowLauncher("Animatronitect - Create Animation", width);
//    createAnimationWindow.SetHeight(500f);
//    createAnimationWindow.rect.position = new Vector2(Screen.width - 400.0f - width, 175.0f);
//    var createAnimationContent = new CreateAmimationContent(createAnimationWindow);
//    createAnimationWindow.Add(createAnimationContent.DoGUI);
//    createAnimationWindow.Open();
//    createAnimationWindow.onClose += (WindowLauncher launcher) => createAnimationWindow = null;
//}