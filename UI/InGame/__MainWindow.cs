using LinkedMovement.UI.InGame;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI.InGame {
    internal sealed class __MainWindow {
        public bool isOpen;
        private readonly MainContent content;
        private readonly CollapsibleWindow window;

        public __MainWindow() {
            content = new MainContent();

            const float width = 400.0f;
            window = new CollapsibleWindow("Link Objects") {
                CustomWidth = width,
                MinimizedName = "Link",
                Minimizable = true,
                Collapsible = false,
                Pinnable = false,
                IsOpen = true,
            };
            window.Rect.position = new Vector2(Screen.width - 200.0f - width, 75.0f);
            window.Add(content.DoGUI);
        }

        public void Show() {
            if (GameController.Instance.isGameInputLocked()) {
                return;
            }

            window.DoGUIWindow();
        }
    }
}
