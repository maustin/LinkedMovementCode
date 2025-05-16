// ATTRIB: MrUnit64 / TransformAnarchy
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement {
    class KeybindManager {
        private List<KeybindManager.Keybind> _allRegistered = new List<KeybindManager.Keybind>();
        private KeyGroup _group;
        private string _name;
        private string _id;

        public KeybindManager(string id, string name) {
            this._group = new KeyGroup(id);
            this._id = id;
            this._name = name;
            this._group.keyGroupName = this._name;
        }

        public void AddKeybind(string id, string name, string description, KeyCode defaultKey) {
            this._allRegistered.Add(new KeybindManager.Keybind(id, name, this._id, defaultKey, description));
        }

        public void ClearAllKeybinds() => this._allRegistered.Clear();

        public void RegisterAll() {
            ScriptableSingleton<InputManager>.Instance.registerKeyGroup(this._group);
            foreach (KeybindManager.Keybind keybind in this._allRegistered)
                ScriptableSingleton<InputManager>.Instance.registerKeyMapping(keybind.mapping);
        }

        public void UnregisterAll() {
            ScriptableSingleton<InputManager>.Instance.unregisterKeyGroup(this._group);
            foreach (KeybindManager.Keybind keybind in this._allRegistered)
                ScriptableSingleton<InputManager>.Instance.unregisterKeyMapping(keybind.mapping);
        }

        public class Keybind {
            public string id;
            public string name;
            public string description;
            public readonly KeyMapping mapping;

            public Keybind(string id, string name, string keyGroupID, KeyCode key, string description) {
                this.id = id;
                this.name = name;
                this.description = description;
                this.mapping = new KeyMapping(id, key, KeyCode.None);
                this.mapping.keyName = this.name;
                this.mapping.keyDescription = this.description;
                this.mapping.keyGroupIdentifier = keyGroupID;
            }
        }
    }
}
