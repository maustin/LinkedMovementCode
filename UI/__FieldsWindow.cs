using RapidGUI;
using UnityEngine;

namespace LinkedMovement.UI {
    class __FieldsWindow : __WindowBase {
        protected override string title => "FieldsWindow";

        public string stringVal;
        public bool boolVal;
        public int intVal;
        public float floatVal;

        public override void DoGUI() {
            Debug.Log("DO GOO");
            stringVal = RGUI.Field(stringVal, "string");
            boolVal = RGUI.Field(boolVal, "bool");
            intVal = RGUI.Field(intVal, "int");
            floatVal = RGUI.Field(floatVal, "float");

            GUILayout.Button("TEST BUTTON 1");
            GUILayout.Button("TEST button 2");
        }
    }
}
