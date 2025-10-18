using LinkedMovement.Utils;
using System;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLinkParent : SerializedRawObject {
        [Serialized]
        public string name = string.Empty;

        [Serialized]
        public string id;

        [NonSerialized]
        public GameObject targetGameObject;
        [NonSerialized]
        public BuildableObject targetBuildableObject;

        public LMLinkParent() {
            LinkedMovement.Log("LMLinkParent constructor");
        }

        public LMLinkParent(string name, string id, BuildableObject buildableObject) {
            LinkedMovement.Log("LMLinkParent contructor with params");

            this.name = name;
            this.id = id;
            this.targetGameObject = buildableObject.gameObject;
            this.targetBuildableObject = buildableObject;
        }

        public void setTarget(BuildableObject buildableObject) {
            this.targetGameObject = buildableObject.gameObject;
            this.targetBuildableObject = buildableObject;
        }

        public void setTarget(GameObject gameObject) {
            this.targetGameObject = gameObject;
            this.targetBuildableObject = LMUtils.GetBuildableObjectFromGameObject(gameObject);
        }
    }
}
