using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
using System;
using System.Text;
using UnityEngine;

namespace LinkedMovement {
    public class LMAnimationParams : SerializedRawObject {
        [NonSerialized]
        public Vector3 startingPosition = Vector3.zero;
        [NonSerialized]
        public Vector3 startingLocalPosition = Vector3.zero;
        [NonSerialized]
        public Vector3 startingRotation = Vector3.zero;
        [NonSerialized]
        public Vector3 startingLocalRotation = Vector3.zero;
        //[Serialized]
        //public float staringScale = 1f;

        [Serialized]
        public Vector3 originalRotation = Vector3.zero;
        [Serialized]
        public Vector3 originalLocalRotation = Vector3.zero;

        [Serialized]
        public string name = string.Empty;
        [Serialized]
        public bool generatedOrigin = false;
        [Serialized]
        public Vector3 targetPosition = Vector3.zero;
        [Serialized]
        public Vector3 targetRotation = Vector3.zero;
        //[Serialized]
        //public float targetScale = 1f;
        [Serialized]
        public bool isTriggerable = false;
        [Serialized]
        public float toDuration = 1f;
        [Serialized]
        public string toEase = LMEase.InOutQuad.ToString();
        [Serialized]
        public float fromDelay = 0f;
        [Serialized]
        public float fromDuration = 1f;
        [Serialized]
        public string fromEase = LMEase.InOutQuad.ToString();
        [Serialized]
        public float restartDelay = 0f;
        [Serialized]
        public bool useInitialStartDelay = false;
        [Serialized]
        public float initialStartDelayMin = 0f;
        [Serialized]
        public float initialStartDelayMax = 0f;

        public LMAnimationParams() {
            LinkedMovement.Log("LMAnimationParams base constructor");
        }

        // TODO: Worth changing "startingRotation" to "startingLocalRotation"?
        // startingRotation is the local origin rotation
        // startingPosition is the global origin position (currently unused)
        //public LMAnimationParams(Vector3 startingPosition, Vector3 startingRotation) {
        //    LinkedMovement.Log("LMAnimationParams constructor with starting values");
        //    this.startingPosition = startingPosition;
        //    this.startingRotation = startingRotation;
        //}

        public void setOriginalValues(Transform originTransform) {
            LinkedMovement.Log("LMAnimationParams constructor with params");
            originalRotation = originTransform.eulerAngles;
            originalLocalRotation = originTransform.localEulerAngles;
            LinkedMovement.Log("originalRotation: " + originalRotation.ToString());
            LinkedMovement.Log("originalLocalRotation: " + originalLocalRotation.ToString());
        }

        public void setStartingValues(Transform originTransform, bool originIsGenerated) {
            LinkedMovement.Log("LMAnimationParams setStartingValues");
            generatedOrigin = originIsGenerated;
            startingPosition = originTransform.position;
            startingLocalPosition = originTransform.localPosition;
            startingRotation = originTransform.eulerAngles;
            startingLocalRotation = originTransform.localEulerAngles;

            LinkedMovement.Log("startingPosition: " + startingPosition.ToString());
            LinkedMovement.Log("startingLocalPosition: " + startingLocalPosition.ToString());
            LinkedMovement.Log("startingRotation: " + startingRotation.ToString());
            LinkedMovement.Log("startingLocalRotation: " + startingLocalRotation.ToString());
            LinkedMovement.Log("originalRotation: " + originalRotation.ToString());
            LinkedMovement.Log("originalLocalRotation: " + originalLocalRotation.ToString());
        }

        // TODO: Do we need to ensure this only runs once?
        public void calculateRotationOffset() {
            // When object is built rotated, we need to adjust the target position
            LinkedMovement.Log("calculateRotatationOffset");
            Vector3 rotationOffset = startingLocalRotation - originalLocalRotation;
            LinkedMovement.Log("rotationOffset: " + rotationOffset.ToString());
            Vector3 rotatedPositionTarget = Quaternion.Euler(rotationOffset) * targetPosition;
            LinkedMovement.Log("Original targetPosition: " + targetPosition.ToString());
            LinkedMovement.Log("New targetPosition: " + rotatedPositionTarget.ToString());
            targetPosition = rotatedPositionTarget;
            //Vector3 rotationOffset = animationParams.startingLocalRotation - animationParams.originalLocalRotation;
            //Vector3 rotatedPositionTarget = Quaternion.Euler(rotationOffset) * animationParams.targetPosition;
        }

        public override string ToString() {
            var sb = new StringBuilder("LMAnimationParams\n");
            sb.AppendLine("name: " + name);
            sb.AppendLine("startingPosition: " + startingPosition.ToString());
            sb.AppendLine("startingRotation: " + startingRotation.ToString());
            sb.AppendLine("targetPosition: " + targetPosition.ToString());
            sb.AppendLine("targetRotation: " + targetRotation.ToString());
            sb.AppendLine("isTriggerable: " + isTriggerable.ToString());
            sb.AppendLine("toDuration: " + toDuration.ToString());
            sb.AppendLine("toEase: " + toEase);
            sb.AppendLine("fromDelay: " + fromDelay.ToString());
            sb.AppendLine("fromDuration: " + fromDuration.ToString());
            sb.AppendLine("fromEase: " + fromEase);
            sb.AppendLine("restartDelay: " + restartDelay.ToString());
            sb.AppendLine("useInitialStartDelay: " + useInitialStartDelay.ToString());
            sb.AppendLine("initialStartDelayMin: " + initialStartDelayMin.ToString());
            sb.AppendLine("initialStartDelayMax: " + initialStartDelayMax.ToString());
            return sb.ToString();
        }
    }
}
