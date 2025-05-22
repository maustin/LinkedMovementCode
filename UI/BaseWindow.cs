// ATTRIB: CheatMod
using UnityEngine;

namespace LinkedMovement.AltUI {
    class BaseWindow {
        private int id;

        protected LinkedMovementController controller;
        protected string windowName = "LMWindow";
        protected Rect baseDimensionsRect = new Rect(20f, 20f, 400f, 400f);
        protected Rect windowRect = new Rect();

        public bool isOpen;

        public BaseWindow(LinkedMovementController controller) {
            id = WindowIDManager.GetNextID();
            this.controller = controller;
        }

        public void toggleWindowOpen() => isOpen = !isOpen;

        public void openWindow() => isOpen = true;

        public void closeWindow() => isOpen = false;

        public void drawWindow() {
            windowRect = GUILayout.Window(id, baseDimensionsRect, new GUI.WindowFunction(drawMain), windowName);
        }

        public void drawMain(int windowId) {
            if (GUI.Button(new Rect(windowRect.width - 21f, 6f, 15f, 15f), "x"))
                closeWindow();

            GUI.BeginGroup(new Rect(0f, 0f, windowRect.width, windowRect.height));
            drawContent();
            GUI.EndGroup();
        }

        public virtual void drawContent() {
            //
        }
    }
}
