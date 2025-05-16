using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    class SelectWindow : MonoBehaviour {
        WindowLaunchers launchers;

        public void Start() {
            Debug.Log("LinkedMovement: SelectWindow Start");
            launchers = new WindowLaunchers {
                name = "Select Window"
            };

            var v = launchers.Add("Field", typeof(FieldsWindow));
            v.Open();

            Debug.Log("Contains? " + launchers.Contains("Field").ToString());
        }

        void OnGUI() {
            Debug.Log("LinkedMovement: SelectWindow OnGUI");
            launchers.DoGUI();
        }
    }
}
