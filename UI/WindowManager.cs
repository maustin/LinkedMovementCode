using LinkedMovement.UI.InGame;
using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    class WindowManager {
        // For now, just direct references to windows
        // Main / selector window
        // Existing pairings
        // Info popup
        WindowLauncher mainWindow;
        WindowLauncher infoWindow;
        WindowLauncher existingLinks;

        public WindowManager() {
            LinkedMovement.Log("Create WindowManager");
        }

        public void destroy() {
            LinkedMovement.Log("Destroy WindowManager");
            mainWindow = null;
            infoWindow = null;
            existingLinks = null;
        }

        public void showMainWindow()
        {
            if (mainWindow == null)
            {
                LinkedMovement.Log("WindowManager Show Main Window");
                var width = 400f;
                mainWindow = new WindowLauncher("Link Objects", width);
                mainWindow.rect.position = new Vector2(Screen.width - 200.0f - width, 75.0f);
                var mainContent = new MainContent();
                mainWindow.Add(mainContent.DoGUI);
                mainWindow.Open();
                mainWindow.onClose += (WindowLauncher launcher) => mainWindow = null;
            }
        }

        public void showInfoWindow(string message)
        {
            LinkedMovement.Log("WindowManager Show info " + message);

            if (infoWindow != null)
            {
                infoWindow.Close();
            }

            var width = 300f;
            infoWindow = new WindowLauncher("Info", width);
            infoWindow.rect.position = new Vector2(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f);
            infoWindow.Add(() => GUILayout.Label(message));
            infoWindow.Open();
            infoWindow.onClose += (WindowLauncher launcher) => infoWindow = null;
        }

        public void showExistingLinks() {
            if (existingLinks == null) {
                LinkedMovement.Log("WindowManager Show existing links");
                var width = 400f;
                existingLinks = new WindowLauncher("Existing Links", width);
                existingLinks.SetHeight(500f);
                existingLinks.rect.position = new Vector2(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - 250f);
                var existingPairsContent = new ExistingLinksContent();
                existingLinks.Add(existingPairsContent.DoGUI);
                existingLinks.Open();
                existingLinks.onClose += (WindowLauncher launcher) => existingLinks = null;
            }
        }

        public void DoGUI()
        {
            //LinkedMovement.Log("WindowManager.OnGUI start");
            if (mainWindow != null) {
                //LinkedMovement.Log("WindowManager.OnGUI mainWindow");
                mainWindow.DoGUI();
            }
            if (infoWindow != null) {
                //LinkedMovement.Log("WindowManager.OnGUI infoWindow");
                infoWindow.DoGUI();
            }
            if (existingLinks != null) {
                //LinkedMovement.Log("WindowManager.OnGUI existingLinks");
                existingLinks.DoGUI();
            }
            //LinkedMovement.Log("WindowManager.OnGUI end");
        }
    }
}
