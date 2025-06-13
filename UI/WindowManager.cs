using LinkedMovement.UI.InGame;
using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    class WindowManager {
        // For now, just direct references to windows
        WindowLauncher mainWindow;
        WindowLauncher infoWindow;
        WindowLauncher existingLinksWindow;
        WindowLauncher createAnimationWindow;

        public WindowManager() {
            LinkedMovement.Log("Create WindowManager");
        }

        public void destroy() {
            LinkedMovement.Log("Destroy WindowManager");
            mainWindow = null;
            infoWindow = null;
            existingLinksWindow = null;
        }

        public void showMainWindow()
        {
            if (mainWindow == null)
            {
                LinkedMovement.Log("WindowManager Show Main Window");
                var width = 400f;
                mainWindow = new WindowLauncher("Animatronitect - Link Objects", width);
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
            infoWindow = new WindowLauncher("Animatronitect - Info", width);
            infoWindow.rect.position = new Vector2(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f);
            var infoContent = new InfoContent(message);
            infoWindow.Add(infoContent.DoGUI);
            infoWindow.Open();
            infoWindow.onClose += (WindowLauncher launcher) => infoWindow = null;
        }

        public void showExistingLinksWindow() {
            if (existingLinksWindow == null) {
                LinkedMovement.Log("WindowManager Show existing links");
                var width = 400f;
                existingLinksWindow = new WindowLauncher("Animatronitect - Existing Links", width);
                existingLinksWindow.SetHeight(500f);
                existingLinksWindow.rect.position = new Vector2(Screen.width - 400.0f - width, 175.0f);
                var existingPairsContent = new ExistingLinksContent();
                existingLinksWindow.Add(existingPairsContent.DoGUI);
                existingLinksWindow.Open();
                existingLinksWindow.onClose += (WindowLauncher launcher) => existingLinksWindow = null;
            }
        }

        public void showCreateAnimationWindow() {
            LinkedMovement.Log("WindowManagwer Show create animation");
            if (createAnimationWindow != null) {
                createAnimationWindow.Close();
            }

            var width = 400f;
            createAnimationWindow = new WindowLauncher("Animatronitect - Create Animation", width);
            createAnimationWindow.SetHeight(500f);
            createAnimationWindow.rect.position = new Vector2(Screen.width - 400.0f - width, 175.0f);
            var createAnimationContent = new CreateAmimationContent();
            createAnimationWindow.Add(createAnimationContent.DoGUI);
            createAnimationWindow.Open();
            createAnimationWindow.onClose += (WindowLauncher launcher) => createAnimationWindow = null;
        }

        public void DoGUI()
        {
            if (mainWindow != null) {
                mainWindow.DoGUI();
            }
            if (infoWindow != null) {
                infoWindow.DoGUI();
            }
            if (existingLinksWindow != null) {
                existingLinksWindow.DoGUI();
            }
            if (createAnimationWindow != null) {
                createAnimationWindow.DoGUI();
            }
        }
    }
}
