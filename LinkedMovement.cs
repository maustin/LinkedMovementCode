using HarmonyLib;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovement : AbstractMod {
        public const string VERSION_NUMBER = "beta 11-02";
        public const string HELPER_OBJECT_NAME = "Animation Helper (autohides)";
        public override string getIdentifier() => "com.themeparkitect.LinkedMovementCode";
        public override string getName() => "Animate Things";
        public override string getDescription() => "Animate Things";
        public override string getVersionNumber() {
            return VERSION_NUMBER;
        }

        public override int getOrderPriority() => -9001;

        // NOT MP compatible
        //public override bool isMultiplayerModeCompatible() => true;
        //public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public static LinkedMovement Instance;
        public static Harmony Harmony;
        private static LMController LMController;
        private static bool KeybindsRegistered;

        // TODO: Get rid of this mess
        public enum LOOSE_TEXTURES {
            BUTTON_NORMAL,
            BUTTON_DOWN,
            EXIT_BUTTON,
            POPUP_CONTENT,
            SCROLL_BACKGROUND,
            SCROLL_THUMB_NORMAL,
            SCROLL_THUMB_DOWN,
        };
        private static Dictionary<LOOSE_TEXTURES, string> looseTextureFilenames = new Dictionary<LOOSE_TEXTURES, string>() {
            { LOOSE_TEXTURES.BUTTON_NORMAL, "roundedRect12-white.png" },
            { LOOSE_TEXTURES.BUTTON_DOWN, "roundedRect12-buttondown.png" },
            { LOOSE_TEXTURES.EXIT_BUTTON, "roundedRect12-red.png" },
            { LOOSE_TEXTURES.POPUP_CONTENT, "roundedRect12-popupcontent.png" },
            { LOOSE_TEXTURES.SCROLL_BACKGROUND, "roundedRect12-scrollbackground.png" },
            { LOOSE_TEXTURES.SCROLL_THUMB_NORMAL, "roundedRect12-scrollthumb.png" },
            { LOOSE_TEXTURES.SCROLL_THUMB_DOWN, "roundedRect12-scrollthumbdown.png" },
        };
        private static Dictionary<LOOSE_TEXTURES, Color> looseTextureDefaultColors = new Dictionary<LOOSE_TEXTURES, Color>() {
            { LOOSE_TEXTURES.BUTTON_NORMAL, new Color(1f, 1f, 1f) },
            { LOOSE_TEXTURES.BUTTON_DOWN, new Color(0.78f, 0.78f, 0.78f) },
            { LOOSE_TEXTURES.EXIT_BUTTON, new Color(0.91f, 0.25f, 0.18f) },
            { LOOSE_TEXTURES.POPUP_CONTENT, new Color(0.87f, 0.87f, 0.87f) },
            { LOOSE_TEXTURES.SCROLL_BACKGROUND, new Color(0.46f, 0.46f, 0.46f) },
            { LOOSE_TEXTURES.SCROLL_THUMB_NORMAL, new Color(0.70f, 0.70f, 0.70f) },
            { LOOSE_TEXTURES.SCROLL_THUMB_DOWN, new Color(0.67f, 0.67f, 0.67f) },
        };
        private static Dictionary<LOOSE_TEXTURES, Texture2D> looseTextures = new Dictionary<LOOSE_TEXTURES, Texture2D>();
        public static Texture2D GetLooseTexture(LOOSE_TEXTURES looseTextureType) {
            if (looseTextures.ContainsKey(looseTextureType)) return looseTextures[looseTextureType];
            return null;
        }
        public static Color GetLooseTextureDefaultColor(LOOSE_TEXTURES looseTextureType) {
            return looseTextureDefaultColors[looseTextureType];
        }

        public static bool HasLMController() {
            return LMController != null;
        }

        public static LMController GetLMController() {
            if (LMController == null) {
                Log("Create LMController");
                GameObject go = new GameObject();
                go.name = "LMController";
                LMController = go.AddComponent<LMController>();
            }
            return LMController;
        }

        public static void ClearLMController() {
            Log("ClearLMController");
            if (LMController != null) {
                GameObject.Destroy(LMController);
                LMController = null;
            }
        }

        private KeybindManager _keybindManager;

        public static void Log(string msg) {
            Debug.Log("LinkedMovement: " + msg);
        }

        public LinkedMovement() {
            Log("Constructor");
        }

        public override void onEnabled() {
            Log("Starting v" + VERSION_NUMBER);
            Instance = this;

            Harmony = new Harmony(getIdentifier());

            Log("Patching...");
            Harmony.PatchAll();
            Log("Patching complete");

            registerHotkeys();
            Log("Done register hotkeys");

            var currentModDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log("Mod directory: " + currentModDirectory);

            loadLooseAssets(currentModDirectory);

            loadAssetpack(currentModDirectory);
            
            Log("Assets load complete");
            // TODO: Disable LM UI and only show error if unable to load assets

            Log("Initialize PrimeTween");
            PrimeTweenConfig.ManualInitialize();
            PrimeTweenConfig.warnZeroDuration = false;

            Log("Startup complete");
        }

        public override void onDisabled() {
            Log("onDisabled");
            unregisterHotkeys();
            ClearLMController();

            if (Harmony == null)
                return;
            Harmony.UnpatchAll(getIdentifier());
            Harmony = null;
        }

        private void loadLooseAssets(string currentModDirectory) {
            Log("Attempt to load loose assets");

            foreach (LOOSE_TEXTURES value in Enum.GetValues(typeof(LOOSE_TEXTURES))) {
                loadLooseAsset(currentModDirectory, value);
            }

            Log("Finished loading loose assets");
        }
        private void loadLooseAsset(string currentModDirectory, LOOSE_TEXTURES looseTextureType) {
            if (!looseTextureFilenames.ContainsKey(looseTextureType)) {
                Log("No filepath for loose texture type: " + looseTextureType);
                return;
            }
            var filename = looseTextureFilenames[looseTextureType];
            Log("Attempt to load loose asset: " + filename);

            try {
                var filePath = System.IO.Path.Combine(currentModDirectory, "assets/" + filename);
                Log("file path: " + filePath);
                byte[] fileData;
                if (File.Exists(filePath)) {
                    fileData = File.ReadAllBytes(filePath);
                    var newTexture = new Texture2D(2, 2);
                    newTexture.LoadImage(fileData);
                    newTexture.wrapMode = TextureWrapMode.Clamp;
                    newTexture.filterMode = FilterMode.Bilinear;
                    looseTextures.Add(looseTextureType, newTexture);
                    Log("Loaded texture");
                } else {
                    Log("ERROR: Couldn't find texture path");
                }
            }
            catch (Exception e) {
                Log("FAILED to load loose asset");
                Log(e.ToString());
            }
        }

        private void loadAssetpack(string currentModDirectory) {
            // TODO: try/catch
            Log("Attempt to load assetpack");
            var assetProjectPath = System.IO.Path.Combine(currentModDirectory, "assets/LinkedMovement.assetProject");
            Log("assetProject: " + assetProjectPath);

            var assembly = Assembly.Load("Parkitect");
            var type = assembly.GetType("Parkitect.Mods.AssetPacks.AssetPackMod");

            if (type == null) {
                Log("failed to get type");
            } else {
                Log("got type");

                Type[] parameterTypes = new Type[] { typeof(string) };
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null);
                if (ctor == null) {
                    Log("failed to get constructor");
                } else {
                    Log("got constructor");
                    var instance = ctor.Invoke(new object[] { assetProjectPath }) as AbstractMod;
                    if (ModManager.Instance.hasMod(instance.getIdentifier(), instance.getVersionIdentifier())) {
                        Log("already loaded assets");
                    } else {
                        var folderPath = System.IO.Path.GetDirectoryName(assetProjectPath);
                        var orderPriority = instance.getOrderPriority();
                        Log($"folder path: {folderPath}, order: {orderPriority}");
                        var modEntry = ModManager.Instance.addMod(instance, folderPath, AbstractGameContent.ContentSource.USER_CREATED, orderPriority);
                        if (modEntry != null) {
                            Log("Added mod, enabling");
                            Log("Is enabled: " + modEntry.isEnabled);
                            Log("Is active: " + modEntry.isActive());
                            var existingModEntry = ScriptableSingleton<AssetManager>.Instance.modContext;

                            modEntry.setActive(true);

                            ScriptableSingleton<AssetManager>.Instance.modContext = existingModEntry;
                            ScriptableSingleton<InputManager>.Instance.modContext = existingModEntry;
                        } else {
                            Log("Failed to add mod");
                        }
                    }
                }
            }
            Log("Finished loading assetpack");
        }

        private void registerHotkeys() {
            if (KeybindsRegistered) {
                Log("keybinds already registered");
                return;
            }
            Log("register hotkeys");
            _keybindManager = new KeybindManager(getIdentifier(), getName());
            _keybindManager.AddKeybind("LM_toggleGUI", "Toggle Linker UI", "Toggles whether the Linker UI is visible", KeyCode.Keypad3);
            _keybindManager.AddKeybind("LM_prevTargetObject", "Previous Target Object", "Previous target object", KeyCode.Minus);
            _keybindManager.AddKeybind("LM_nextTargetObject", "Next Target Object", "Next target object", KeyCode.Equals);

            _keybindManager.RegisterAll();
            KeybindsRegistered = true;
        }

        private void unregisterHotkeys() {
            Log("unregister hotkeys");
            _keybindManager.UnregisterAll();
            KeybindsRegistered = false;
        }
    }
}
