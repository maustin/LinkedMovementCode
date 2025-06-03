using System;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUI;

namespace LinkedMovement.UI.Utils {
    //todo: don't show when `Esc` Menu
    // TODO: Think this can be eliminated
    internal sealed class CollapsibleWindow : TitleContent<CollapsibleWindow>, IDoGUIWindow {
        public CollapsibleWindow(string name) : base(name) { }

        public Rect Rect;

        /// <summary>
        /// When set, first size is used as default size (if no custom width/height set).
        /// Usefull when using Folds, that are collapsed by default.
        /// </summary>
        public bool TryKeepFirstSize = true;

        public float? CustomWidth = null, CustomHeight = null;
        private Vector2 GetSize() {
            var size = Rect.size;
            if (CustomWidth.HasValue) {
                size.x = CustomWidth.Value;
            }
            if (CustomHeight.HasValue) {
                size.y = CustomHeight.Value;
            }
            return size;
        }
        private void ApplyCustomSize() {
            if (CustomWidth.HasValue || CustomHeight.HasValue) {
                Rect.size = GetSize();
            }
        }

        public void UseAutoWidth() => CustomWidth = null;
        public void UseAutoHeight() => CustomHeight = null;

        public bool IsOpen = false;

        public bool Minimizable = true;
        public bool Minimized = false;
        public string MinimizedName = "";
        public bool Collapsible = true;
        public bool Collapsed = false;

        public bool Pinnable = true;
        public bool Pinned = false;

        private void DoHeader(float rectWidth) {
            // ordered by order of method calls
            // from right to left (right border inwards)
            int i = 0;
            void Btn(bool enabled, ref bool value, string checkedString, string uncheckedString) {
                if (!enabled) {
                    return;
                }

                var buttonSize = new Vector2(25.0f, 15.0f);
                var x = rectWidth - buttonSize.x - ((2 + buttonSize.x) * i);
                var pos = new Vector2(x, 2.0f);
                var rect = new Rect(pos, buttonSize);
                if (GUI.Button(rect, value ? checkedString : uncheckedString, RGUIStyle.flatButton)) {
                    value = !value;
                }
                i++;
            }

            Btn(Minimizable, ref Minimized, "◄", "►");
            if (!Minimized) {
                Btn(Collapsible, ref Collapsed, "❐", "‒");
                Btn(Pinnable, ref Pinned, "!", "~");
            }
        }

        private Vector2 lastMousePos = Vector2.zero;

        private Vector2 firstSize = Vector2.zero;
        public void DoGUIWindow() {
            if (!IsOpen) {
                return;
            }

            Debug.Log("CollapsibleWindow DoGUIWindow~");

            ApplyCustomSize();

            if (Minimized) {
                const float width = (25.0f * 2) + 2 + (2 * 2);
                var rect = new Rect(Rect.x + Rect.width - width, Rect.y, width, 17.0f);
                using (Scope.Area(rect)) {
                    GUI.Box(new Rect(0.0f, 0.0f, rect.size.x, rect.size.y), MinimizedName, Styles.DarkWindowTitleBarMinimizedStyle);

                    var preCollapsible = Collapsible;
                    Collapsible = false;
                    DoHeader(rect.width);
                    Collapsible = preCollapsible;
                }

                var newRect = DoDrag(rect);
                if (newRect.position != rect.position) {
                    var pos = newRect.position;
                    pos.x = pos.x + width - Rect.width;
                    Rect.position = pos;
                }
            }
            else if (Collapsed) {
                const int margin = 6;

                var rect = new Rect(Rect.x - margin, Rect.y, Rect.width + margin * 2, 17.0f);
                var prevPos = rect.position;

                using (Scope.Area(rect)) {
                    var style = Styles.DarkWindowTitleBarStyle;
                    // style.alignment = TextAnchor.MiddleCenter;
                    GUI.Box(new Rect(0.0f, 0.0f, rect.size.x, rect.size.y), name, Styles.DarkWindowTitleBarStyle);
                    DoHeader(rect.width - margin);
                }

                // make draggabble
                rect = DoDrag(rect);
                if (prevPos != rect.position) {
                    var pos = rect.position;
                    pos.x += margin;
                    Rect.position = pos;
                }
            }
            else {
                Rect = RGUI.ResizableWindow(GetHashCode(), Rect,
                  _ => {
                      DoHeader(Rect.width);

                      foreach (var func in GetGUIFuncs()) {
                          func();
                      }
                      if (!Pinned) {
                          GUI.DragWindow();
                      }

                      if (Event.current.type == EventType.Used) {
                          WindowInvoker.SetFocusedWindow(this);
                      }
                  }, name, RGUIStyle.darkWindow);

                if (TryKeepFirstSize) {
                    if (firstSize == Vector2.zero) {
                        firstSize = Rect.size;
                    }
                    else {
                        Rect.size = firstSize;
                    }
                }
            }

            ApplyCustomSize();
        }

        private Rect DoDrag(Rect rect) {
            if (Pinned) {
                return rect;
            }

            var id = String.IsNullOrWhiteSpace(name) ? GetHashCode() : name.GetHashCode();
            var controlId = GUIUtility.GetControlID(id, FocusType.Passive);

            var ev = Event.current;
            switch (ev.GetTypeForControl(controlId)) {
                case EventType.MouseDown: {
                        if (rect.Contains(ev.mousePosition)) {
                            GUIUtility.hotControl = controlId;
                            lastMousePos = ev.mousePosition;
                            ev.Use();
                        }
                    }
                    break;

                case EventType.MouseUp: {
                        if (GUIUtility.hotControl == controlId) {
                            GUIUtility.hotControl = 0;
                            ev.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag: {
                        if (GUIUtility.hotControl == controlId) {
                            var d = ev.mousePosition - lastMousePos;
                            rect.position += d;

                            lastMousePos = ev.mousePosition;
                            ev.Use();
                        }
                    }
                    break;
            }

            return rect;
        }

        public void CloseWindow() {
            IsOpen = false;
        }
    }
}
