using LinkedMovement.UI.InGame;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.UI {
    public class WindowManager {

        // TODO: Move out
        public enum WindowType {
            ModeDetermination,
            CreateNewAnimatronic,
            ShowExistingAnimatronics,
            EditAnimatronic,
            EditAnimation,
            Information,
            Error,
            ConfirmAction,
        }

        private List<LMWindow> activeWindows = new List<LMWindow>();
        private List<LMWindow> renderWindows;
        private bool dirtyActiveWindows = true;

        public WindowManager() {
            LinkedMovement.Log("WindowManager constructor");
        }

        public void destroy() {
            LinkedMovement.Log("WindowManager destructor");
        }

        public bool uiPresent() {
            foreach (LMWindow window in activeWindows) {
                if (!window.alwaysRender) return true;
            }
            return false;
        }

        public void createWindow(WindowType type, object data) {
            LinkedMovement.Log($"WindowManager.createWindow {type.ToString()}");
            LMWindow window = LMWindowFactory.BuildWindow(type, data, this);
            if (window != null) {
                activeWindows.Add(window);
                dirtyActiveWindows = true;
            } else {
                LinkedMovement.Log("Failed to create window");
            }
        }

        public bool hasWindowOfType(WindowType type) {
            return activeWindows.Exists(window => window.type == type);
        }

        public void DoGUI()
        {
            if (activeWindows.Count == 0) return;

            if (dirtyActiveWindows) {
                dirtyActiveWindows = false;

                renderWindows = new List<LMWindow>();
                var didFindNonOverlay = false;

                for (var i = activeWindows.Count - 1; i >= 0; i--) {
                    var window = activeWindows[i];
                    if (window.alwaysRender) {
                        renderWindows.Insert(0, window);
                    } else {
                        if (!didFindNonOverlay) {
                            didFindNonOverlay = true;
                            renderWindows.Insert(0, window);
                        }
                    }
                }
            }

            foreach (var window in renderWindows) {
                window.DoGUI();
            }
        }

        public void removeWindow(LMWindow window) {
            activeWindows.Remove(window);
            dirtyActiveWindows = true;
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
