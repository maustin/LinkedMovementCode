using HarmonyLib;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovement : AbstractMod, IModSettings {
        public const string VERSION_NUMBER = "RC 5 (11-19)";
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
        public static LinkedMovementSettings Settings;
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
            ICON_EYE,
            ICON_EYE_STRIKE,
        };
        private static Dictionary<LOOSE_TEXTURES, string> looseTextureFilenames = new Dictionary<LOOSE_TEXTURES, string>() {
            { LOOSE_TEXTURES.BUTTON_NORMAL, "roundedRect12-white.png" },
            { LOOSE_TEXTURES.BUTTON_DOWN, "roundedRect12-buttondown.png" },
            { LOOSE_TEXTURES.EXIT_BUTTON, "roundedRect12-red.png" },
            { LOOSE_TEXTURES.POPUP_CONTENT, "roundedRect12-popupcontent.png" },
            { LOOSE_TEXTURES.SCROLL_BACKGROUND, "roundedRect12-scrollbackground.png" },
            { LOOSE_TEXTURES.SCROLL_THUMB_NORMAL, "roundedRect12-scrollthumb.png" },
            { LOOSE_TEXTURES.SCROLL_THUMB_DOWN, "roundedRect12-scrollthumbdown.png" },
            { LOOSE_TEXTURES.ICON_EYE, "ui_icon_eye.png" },
            { LOOSE_TEXTURES.ICON_EYE_STRIKE, "ui_icon_eye_strike.png" },
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
                LMLogger.Info("Create LMController");
                GameObject go = new GameObject();
                go.name = "LMController";
                LMController = go.AddComponent<LMController>();
            }
            return LMController;
        }

        public static void ClearLMController() {
            LMLogger.Info("ClearLMController");
            if (LMController != null) {
                GameObject.Destroy(LMController);
                LMController = null;
            }
        }

        private KeybindManager _keybindManager;

        public LinkedMovement() {
            LMLogger.Info("Constructor");
            registerHotkeys();
            LMLogger.Info("Done register hotkeys");
        }

        public override void onEnabled() {
            LMLogger.Info("Starting v" + VERSION_NUMBER);
            Instance = this;

            LMLogger.Info("Load settings");
            loadSettings();
            LMLogger.Info("Loaded settings");

            Harmony = new Harmony(getIdentifier());

            LMLogger.Info("Patching...");
            Harmony.PatchAll();
            LMLogger.Info("Patching complete");

            //registerHotkeys();
            //Log("Done register hotkeys");

            var currentModDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            LMLogger.Info("Mod directory: " + currentModDirectory);

            loadLooseAssets(currentModDirectory);

            loadAssetpack(currentModDirectory);
            
            LMLogger.Info("Assets load complete");
            // TODO: Disable LM UI and only show error if unable to load assets

            LMLogger.Info("Initialize PrimeTween");
            PrimeTweenConfig.ManualInitialize();
            PrimeTweenConfig.warnZeroDuration = false;

            LMLogger.Info("Startup complete");
        }

        public override void onDisabled() {
            LMLogger.Info("onDisabled");
            //unregisterHotkeys();
            ClearLMController();

            if (Harmony == null)
                return;
            Harmony.UnpatchAll(getIdentifier());
            Harmony = null;
        }

        public void onDrawSettingsUI() {
            GUILayout.BeginVertical();

            Settings.debugLogging = GUILayout.Toggle(Settings.debugLogging, " Enable debug logging");

            GUILayout.EndVertical();
        }

        public void onSettingsOpened() {
            loadSettings();
        }

        public void onSettingsClosed() {
            saveSettings();
            GUI.FocusControl(null);
        }

        private string getSettingsPath() {
            return System.IO.Path.Combine(ModManager.Instance.getMod(getIdentifier()).path, "animate_things.json");
        }

        private void loadSettings() {
            if (File.Exists(getSettingsPath())) {
                LMLogger.Info("Loading settings");
                Settings = JsonUtility.FromJson<LinkedMovementSettings>(File.ReadAllText(getSettingsPath()));
            } else {
                LMLogger.Info("Initialize settings");
                Settings = new LinkedMovementSettings();
                this.saveSettings();
            }

            updateLogger();
        }

        private void saveSettings() {
            LMLogger.Info("Saving settings");

            File.WriteAllText(getSettingsPath(), JsonUtility.ToJson(Settings, true));
            updateLogger();
        }

        private void updateLogger() {
            if (Settings.debugLogging) {
                LMLogger.SetLogLevel(LogLevel.Debug);
            } else {
                LMLogger.SetLogLevel(LogLevel.Info);
            }
        }

        private void loadLooseAssets(string currentModDirectory) {
            LMLogger.Info("Attempt to load loose assets");

            foreach (LOOSE_TEXTURES value in Enum.GetValues(typeof(LOOSE_TEXTURES))) {
                loadLooseAsset(currentModDirectory, value);
            }

            LMLogger.Info("Finished loading loose assets");
        }
        private void loadLooseAsset(string currentModDirectory, LOOSE_TEXTURES looseTextureType) {
            if (!looseTextureFilenames.ContainsKey(looseTextureType)) {
                LMLogger.Info("No filepath for loose texture type: " + looseTextureType);
                return;
            }
            var filename = looseTextureFilenames[looseTextureType];
            LMLogger.Info("Attempt to load loose asset: " + filename);

            try {
                var filePath = System.IO.Path.Combine(currentModDirectory, "assets/" + filename);
                LMLogger.Info("file path: " + filePath);
                byte[] fileData;
                if (File.Exists(filePath)) {
                    fileData = File.ReadAllBytes(filePath);
                    var newTexture = new Texture2D(2, 2);
                    newTexture.LoadImage(fileData);
                    newTexture.wrapMode = TextureWrapMode.Clamp;
                    newTexture.filterMode = FilterMode.Bilinear;
                    looseTextures.Add(looseTextureType, newTexture);
                    LMLogger.Info("Loaded texture");
                } else {
                    LMLogger.Error("Couldn't find texture path");
                }
            }
            catch (Exception e) {
                LMLogger.Info("FAILED to load loose asset");
                LMLogger.Info(e.ToString());
            }
        }

        private void loadAssetpack(string currentModDirectory) {
            // TODO: try/catch
            LMLogger.Info("Attempt to load assetpack");
            var assetProjectPath = System.IO.Path.Combine(currentModDirectory, "assets/LinkedMovement.assetProject");
            LMLogger.Info("assetProject: " + assetProjectPath);

            var assembly = Assembly.Load("Parkitect");
            var type = assembly.GetType("Parkitect.Mods.AssetPacks.AssetPackMod");

            if (type == null) {
                LMLogger.Info("failed to get type");
            } else {
                LMLogger.Info("got type");

                //var isCampaign = GameController.Instance.isCampaignScenario;
                //GameController.Instance.isCampaignScenario = false;
                //var modsBlocked = GameController.Instance.modsBlocked;
                //GameController.Instance.modsBlocked = false;

                Type[] parameterTypes = new Type[] { typeof(string) };
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null);
                if (ctor == null) {
                    LMLogger.Info("failed to get constructor");
                } else {
                    LMLogger.Info("got constructor");
                    var instance = ctor.Invoke(new object[] { assetProjectPath }) as AbstractMod;
                    if (ModManager.Instance.hasMod(instance.getIdentifier(), instance.getVersionIdentifier())) {
                        LMLogger.Info("already loaded assets");
                    } else {
                        var folderPath = System.IO.Path.GetDirectoryName(assetProjectPath);
                        var orderPriority = instance.getOrderPriority();
                        LMLogger.Info($"folder path: {folderPath}, order: {orderPriority}");
                        var modEntry = ModManager.Instance.addMod(instance, folderPath, AbstractGameContent.ContentSource.USER_CREATED, orderPriority);
                        if (modEntry != null) {
                            LMLogger.Info("Added mod, enabling");
                            LMLogger.Info("Is enabled: " + modEntry.isEnabled);
                            LMLogger.Info("Is active: " + modEntry.isActive());
                            var existingModEntry = ScriptableSingleton<AssetManager>.Instance.modContext;

                            modEntry.setActive(true);
                            //modEntry.enableMod();

                            ScriptableSingleton<AssetManager>.Instance.modContext = existingModEntry;
                            ScriptableSingleton<InputManager>.Instance.modContext = existingModEntry;
                        } else {
                            LMLogger.Info("Failed to add mod");
                        }
                    }
                }

                //GameController.Instance.isCampaignScenario = isCampaign;
                //GameController.Instance.modsBlocked = modsBlocked;
            }
            LMLogger.Info("Finished loading assetpack");
        }

        private void registerHotkeys() {
            if (KeybindsRegistered) {
                LMLogger.Info("keybinds already registered");
                return;
            }
            LMLogger.Info("register hotkeys");
            _keybindManager = new KeybindManager(getIdentifier(), getName());
            _keybindManager.AddKeybind("LM_toggleGUI", "Show UI", "Show the Animate Things UI", KeyCode.Keypad3);
            _keybindManager.AddKeybind("LM_prevTargetObject", "Previous Target Object", "When selecting an object, cycle to the Previous object under the mouse", KeyCode.Minus);
            _keybindManager.AddKeybind("LM_nextTargetObject", "Next Target Object", "When selecting an object, cycle to the Next object under the mouse", KeyCode.Equals);

            _keybindManager.RegisterAll();
            KeybindsRegistered = true;
        }

        private void unregisterHotkeys() {
            LMLogger.Info("unregister hotkeys");
            _keybindManager.UnregisterAll();
            KeybindsRegistered = false;
        }
    }
}
