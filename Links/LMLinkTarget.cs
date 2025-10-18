using LinkedMovement.Utils;
using System;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLinkTarget : SerializedRawObject {
        [Serialized]
        public string id;

        [NonSerialized]
        public GameObject targetGameObject;
        [NonSerialized]
        public BuildableObject targetBuildableObject;

        public LMLinkTarget() {
            LinkedMovement.Log("LMLinkTarget constructor");
        }

        public LMLinkTarget(string id, BuildableObject buildableObject) {
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
