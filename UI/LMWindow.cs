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
                //windowManager.removeWindow(launcher as LMWindow);
                LinkedMovement.Log("LMWindow.onClose");
                
                // TODO: Refactor so controller subscribes to onClose and handles this

                if (type == WindowManager.WindowType.CreateAnimationNew) {
                    LinkedMovement.Log("CLOSE CreateAnimationNew window");
                    LinkedMovement.GetLMController().clearEditMode();
                }
                if (type == WindowManager.WindowType.CreateLinkNew) {
                    LinkedMovement.Log("CLOSE CreateLinkNew window");
                    LinkedMovement.GetLMController().clearEditMode();
                }

                // OLD UI
                if (type == WindowManager.WindowType.CreateNewAnimatronic) {
                    LinkedMovement.Log("CLOSE CreateNewAnimatronic window");
                    LinkedMovement.GetController().discardChanges();
                }
                if (type == WindowManager.WindowType.EditAnimatronic) {
                    LinkedMovement.Log("CLOSE EditAnimatronic window");
                    LinkedMovement.GetController().discardChanges();
                }

                windowManager.removeWindow(launcher as LMWindow);
            };
        }
    }
}
