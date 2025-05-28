using System;
using RapidGUI;
using UnityEngine;
using static RapidGUI.RGUI;

namespace LinkedMovement.UI.Utils {
    internal static class Controls {
        internal struct ValidationTextFieldResult<T> {
            public bool Valid;
            public bool Changed;
            public T Value;
        }

        private static readonly GUILayoutOption[] DefaultValidationTextFieldOptions = new[] { GUILayout.MinWidth(80f) };
        public static ValidationTextFieldResult<T> ValidationTextField<T>(T value, TryParse<T> parser, params GUILayoutOption[] options) {
            //todo: `4.0` is invalid (but correctly reduced to `4`)
            //todo: parser param with TryParse?
            var unparsedStr = CachedUnparsedString.Create();
            var isValid = !unparsedStr.HasString || unparsedStr.CanParse(parser);
            var color = isValid ? GUI.color : Color.red;

            using (new ColorScope(color)) {
                var text = unparsedStr.String ?? ((value != null) ? value.ToString() : "");
                var guiOptions = options is null || options.Length == 0 ? DefaultValidationTextFieldOptions : options;
                var displayStr = GUILayout.TextField(text, guiOptions);
                if (displayStr != text) {
                    var res = new ValidationTextFieldResult<T>() {
                        Valid = false,
                        Changed = true,
                        Value = value,
                    };

                    if (parser(displayStr, out var result)) {
                        res.Valid = true;
                        res.Value = result;
                        if (result.ToString() == displayStr) {
                            displayStr = null;
                        }
                    }

                    unparsedStr.String = displayStr;

                    return res;
                }
                else {
                    return new() {
                        Valid = isValid,
                        Changed = false,
                        Value = value,
                    };
                }
            }
        }

        public static bool ExpandCollapseButton(bool expanded, params GUILayoutOption[] options) {
            var foldStr = expanded ? "▲" : "▼";
            expanded ^= GUILayout.Button(foldStr, Fold.Style.Fold, options);
            return expanded;
        }
    }

    // Based on `RapidGUI.UnparsedStr`
    internal struct CachedUnparsedString {
        private static string LastString;
        private static int LastControlId;

        public static CachedUnparsedString Create() {
            if (ForcusChecker.IsChanged()) {
                Reset();
            }

            return new CachedUnparsedString();
        }

        private static void Reset() {
            LastString = null;
            LastControlId = 0;
        }

        private readonly int ControlId;
        public CachedUnparsedString() {
            ControlId = GUIUtility.GetControlID(FocusType.Passive);
        }

        public bool HasString => ControlId == LastControlId;
        public string String {
            get => HasString ? LastString : null;
            set {
                if (value is null) {
                    if (HasString) {
                        Reset();
                    }
                }
                else {
                    LastString = value;
                    LastControlId = ControlId;
                }
            }
        }
        public bool TryGetString(out string s) {
            if (HasString) {
                s = LastString;
                return true;
            }
            else {
                s = default;
                return false;
            }
        }

        public bool CanParse<T>(TryParse<T> parser)
          => TryGetString(out var s) && parser(s, out var _);
        public bool TryParse<T>(TryParse<T> parser, out T result) {
            if (TryGetString(out var s)) {
                return parser(s, out result);
            }
            else {
                result = default;
                return false;
            }
        }
    }
    internal delegate bool TryParse<T>(string s, out T result);
}
