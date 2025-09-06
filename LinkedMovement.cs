using HarmonyLib;
using Parkitect.Mods.AssetPacks;
using PrimeTween;
using System;
using System.Reflection;
using UnityEngine;

namespace LinkedMovement {
    public class LinkedMovement : AbstractMod {
        public const string VERSION_NUMBER = "beta 9-06";
        //public override string getIdentifier() => "artex.linkedMovement";
        //public override string getIdentifier() => "com.themeparkitect.LinkedMovement";
        public override string getIdentifier() => "com.themeparkitect.LinkedMovementCode";
        public override string getName() => "Linked Movement";
        public override string getDescription() => "move things";
        public override string getVersionNumber() {
            return VERSION_NUMBER;
        }

        // NOT MP compatible
        //public override bool isMultiplayerModeCompatible() => true;
        //public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public static LinkedMovement Instance;
        public static Harmony Harmony;
        private static LinkedMovementController Controller;
        private static bool KeybindsRegistered;

        public static LinkedMovementController GetController() {
            if (Controller == null) {
                Log("Create Controller!");
                GameObject go = new GameObject();
                go.name = "LinkedMovementController";
                Controller = go.AddComponent<LinkedMovementController>();
            }
            return Controller;
        }

        public static void ClearController() {
            Log("ClearController");
            GameObject.Destroy(Controller);
            Controller = null;
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

            // TODO: Remove. Currently unnecessary as we're using built-in object for generated origin object.
            Log("Attempt to load assets");
            var currentModDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log("Mod directory: " + currentModDirectory);
            var assetProjectPath = System.IO.Path.Combine(currentModDirectory, "assets\\LinkedMovement.assetProject");
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
            destroyController();

            if (Harmony == null)
                return;
            Harmony.UnpatchAll(getIdentifier());
            Harmony = null;
        }

        private void destroyController() {
            Log("destroyController");
            if (Controller != null) {
                GameObject.Destroy(Controller.gameObject);
                Controller = null;
            }
        }

        private void registerHotkeys() {
            if (KeybindsRegistered) {
                Log("keybinds already registered");
                return;
            }
            Log("register hotkeys");
            _keybindManager = new KeybindManager(getIdentifier(), getName());
            _keybindManager.AddKeybind("LM_toggleGUI", "Toggle Linker UI", "Toggles whether the Linker UI is visible", KeyCode.Keypad3);
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
