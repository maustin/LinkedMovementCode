using LinkedMovement.Utils;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Animation {
    public class LMAnimation {
        
        public GameObject targetGameObject;
        public BuildableObject targetBuildableObject;

        public Sequence sequence;

        private LMAnimationParams animationParams;
        // Used when editing params. Allow ability to discard changes.
        // If discard, simply delete the temp. If save, replace params data on target and swap temp to base.
        private LMAnimationParams tempAnimationParams;

        public LMAnimation() {
            LinkedMovement.Log("LMAnimation constructor");
            //animationParams = new LMAnimationParams();
        }

        // TODO: Constructor that takes an object?

        public LMAnimation(LMAnimationParams animationParams) {
            LinkedMovement.Log("LMAnimation constructor w/ params");
            this.animationParams = animationParams;
        }

        public LMAnimationParams getAnimationParams() {
            if (tempAnimationParams != null) return tempAnimationParams;
            return animationParams;
        }

        public void setTarget(BuildableObject buildableObject) {
            removeTarget();

            targetBuildableObject = buildableObject;
            targetGameObject = targetBuildableObject.gameObject;
            LMUtils.AddObjectHighlight(targetBuildableObject, Color.red);
        }

        public void removeTarget() {
            if (targetGameObject != null) {
                LMUtils.RemoveObjectHighlight(targetBuildableObject);
                targetGameObject = null;
                targetBuildableObject = null;
            }
        }

        public void discardChanges() {
            LinkedMovement.Log("LMAnimation discardChanges");
            // TODO
        }

        public void saveChanges() {
            LinkedMovement.Log("LMAnimation saveChanges");
            // TODO
        }

        // Remove the animation from the target
        public void removeAnimation() {
            // TODO
        }
    }
}
