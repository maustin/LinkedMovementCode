using LinkedMovement.Utils;
using PrimeTween;
using System;
using UnityEngine;

namespace LinkedMovement.Links {
    public class LMLinkParent : SerializedRawObject {
        [Serialized]
        public string linkName = string.Empty;

        [Serialized]
        public string linkId;

        [NonSerialized]
        public GameObject targetGameObject;
        [NonSerialized]
        public BuildableObject targetBuildableObject;

        // TODO: Name
        // TODO: ID

        public LMLinkParent() {
            LinkedMovement.Log("LMLinkParent constructor");
        }
    }
}
