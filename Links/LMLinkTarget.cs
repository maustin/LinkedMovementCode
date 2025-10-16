using LinkedMovement.Utils;
using PrimeTween;
using System;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLinkTarget : SerializedRawObject {
        [Serialized]
        public string linkId;

        [NonSerialized]
        public GameObject targetGameObject;
        [NonSerialized]
        public BuildableObject targetBuildableObject;

        public LMLinkTarget() {
            LinkedMovement.Log("LMLinkTarget constructor");
        }
    }
}
