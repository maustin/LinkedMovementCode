using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    public class LMWindow : WindowLauncher {
        private LMWindowContent content;
        public WindowManager.WindowType type;
        public bool alwaysRender;

        public LMWindow(WindowManager.WindowType type, string title, LMWindowContent content, bool alwaysRender, int width) : base(title, width) {
            LinkedMovement.Log("LMWindow: " + title);
            this.type = type;
            this.content = content;
            this.alwaysRender = alwaysRender;
            this.content.title = title;
        }

        public void Configure(Vector2 position, int fixedHeight, WindowManager windowManager) {
            this.content.windowManager = windowManager;
            this.content.window = this;

            if (fixedHeight > 0) {
                this.SetHeight(fixedHeight);
            }

            this.rect.position = position;
            this.Add(content.DoGUI);
            this.Open();
            this.onClose += (WindowLauncher launcher) => {
                windowManager.removeWindow(launcher as LMWindow);
                // TODO: NEED a better way to do this
                if (type == WindowManager.WindowType.CreateNewAnimatronic) {
                    LinkedMovement.Log("New Animatronic Window closing");
                    LinkedMovement.GetController().discardChanges();
                }
                if (type == WindowManager.WindowType.EditAnimatronic) {
                    LinkedMovement.Log("CLOSE EditAnimatronic window");
                    LinkedMovement.GetController().discardChanges();
                }
            };
        }
    }
}
