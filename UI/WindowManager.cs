using LinkedMovement.UI.InGame;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.UI {
    public class WindowManager {

        // TODO: Move out to class
        public enum WindowType {
            ModeDetermination,
            CreateNewAnimatronic,
            ShowExistingAnimatronics,
            EditAnimatronic,
            EditAnimation,
            Information,
            Error,
        }

        private List<LMWindow> activeWindows = new List<LMWindow>();

        public WindowManager() {
            LinkedMovement.Log("Create WindowManager");
        }

        public void destroy() {
            LinkedMovement.Log("Destroy WindowManager");
        }

        public bool uiPresent() {  return activeWindows.Count > 0; }

        public void createWindow(WindowType type, object data) {
            LMWindow window = LMWindowFactory.BuildWindow(type, data, this);
            if (window != null) {
                activeWindows.Add(window);
            } else {
                LinkedMovement.Log("Failed to create window");
            }
        }

        public bool hasWindowOfType(WindowType type) {
            return activeWindows.Exists(window => window.type == type);
        }

        //public void showMainWindow()
        //{
        //    if (mainWindow == null)
        //    {
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

        public void DoGUI()
        {
            // TODO: Do we need to clone the list to prevent reorder?
            for (var i = 0; i < activeWindows.Count; i++) {
                if (activeWindows[i].alwaysRender || i == activeWindows.Count - 1)
                    activeWindows[i].DoGUI();
            }
        }

        public void removeWindow(LMWindow window) {
            activeWindows.Remove(window);
        }

        public void removeWindowOfType(WindowType type) {
            foreach (var window in activeWindows) {
                if (window.type == type) {
                    removeWindow(window);
                    break;
                }
            }
        }
    }
}
