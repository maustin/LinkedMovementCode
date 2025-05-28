using System;
using UnityEngine;

namespace LinkedMovement.UI.Utils {
    public sealed class Scope : IDisposable {
        private static readonly Scope horizontal = new(GUILayout.EndHorizontal);
        public static Scope Horizontal() {
            GUILayout.BeginHorizontal();
            return horizontal;
        }

        private static readonly Scope vertical = new(GUILayout.EndVertical);
        public static Scope Vertical() {
            GUILayout.BeginVertical();
            return vertical;
        }

        public const int DefaultIndentation = 16;
        private static readonly Scope indentation = new(static () => {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        });
        public static Scope Indentation(float width = DefaultIndentation) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(width);
            GUILayout.BeginVertical();
            return indentation;
        }

        private static readonly Scope area = new(GUILayout.EndArea);
        public static Scope Area(Rect rect) {
            GUILayout.BeginArea(rect);
            return area;
        }

        private static readonly Scope doNothing = new(static () => { });
        private static readonly Scope disableGuiEnd = new(static () => GUI.enabled = false);
        private static readonly Scope enableGuiEnd = new(static () => GUI.enabled = true);
        public static Scope GuiEnabled(bool value) {
            if (GUI.enabled == value) {
                return doNothing;
            }
            else {
                GUI.enabled = value;
                return value ? disableGuiEnd : enableGuiEnd;
            }
        }

        private readonly Action _end;
        private Scope(Action end) {
            _end = end;
        }

        private void End() {
            _end();
        }

        public void Dispose() {
            End();
        }
    }
}
