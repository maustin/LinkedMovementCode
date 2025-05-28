using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    public abstract class __WindowBase : MonoBehaviour, IDoGUI {
        protected abstract string title { get; }

        private void OnGUI() {
            if (transform.parent == null) {
                Debug.Log("LinkedMovement: WindowBase OnGUI NO PARENT");
                GUILayout.Label($"<b>{title}</b>");
                DoGUI();
            } else {
                Debug.Log("LinkedMovement: WindowBase OnGUI has parent");
            }
        }

        public abstract void DoGUI();
    }
}
