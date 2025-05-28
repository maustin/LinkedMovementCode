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

        public WindowManager() {
            //
        }

        public void showMainWindow()
        {
            if (mainWindow == null)
            {
                LinkedMovement.Log("Show Main Window");
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
            LinkedMovement.Log("Show info " + message);

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

        public void OnGUI()
        {
            if (mainWindow != null) {
                LinkedMovement.Log("WindowManager OnGUI");
                mainWindow.DoGUI();
            }
            if (infoWindow != null) {
                infoWindow.DoGUI();
            }
        }
    }
}
