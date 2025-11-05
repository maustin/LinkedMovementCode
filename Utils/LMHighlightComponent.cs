using System;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {

    [Flags]
    public enum HighlightType {
        None = 0,
        MouseOver = 1 << 0,
        AnimationTarget = 1 << 1,
        LinkParent = 1 << 2,
        LinkTarget = 1 << 3,
    }

    // TODO: Flag colors

    public class LMHighlightComponent : MonoBehaviour {
        private HighlightType currentFlags = HighlightType.None;
        private HighlightOverlayController.HighlightHandle highlightHandle;

        public void addHighlightFlag(HighlightType flag) {
            LinkedMovement.Log("LMHighlightComponent.addHighlightFlag " + flag.ToString());
            HighlightType oldFlags = currentFlags;
            currentFlags |= flag;

            if (oldFlags != currentFlags) {
                rebuildHighlight();
            }
        }

        public void removeHighlightFlag(HighlightType flag) {
            LinkedMovement.Log("LMHighlightComponent.removeHighlightFlag " + flag.ToString());
            HighlightType oldFlags = currentFlags;
            currentFlags &= ~flag;

            if (oldFlags != currentFlags) {
                rebuildHighlight();
            }
        }

        public bool hasNoHighlights() {
            var highlightIsNone = currentFlags == HighlightType.None;
            //LinkedMovement.Log("LMHighlightComponent.hasNoHighlights: " + highlightIsNone);
            return highlightIsNone;
        }

        //public void clearHighlight() {
        //    LinkedMovement.Log("LMHighlightComponent.clearHighlight");
        //    currentFlags = HighlightType.None;
        //    rebuildHighlight();
        //}

        private bool hasFlag(HighlightType flag) {
            return (currentFlags & flag) == flag;
        }

        // TODO: This can be simplified. Many of these combinations are not possible with current UI flow.
        private void rebuildHighlight() {
            LinkedMovement.Log("LMHighlightComponent.rebuildHighlight");
            if (highlightHandle != null) {
                LinkedMovement.Log("Remove existing");
                highlightHandle.remove();
                highlightHandle = null;
            }

            if (hasFlag(HighlightType.MouseOver)) {
                //LinkedMovement.Log("MouseOver");
                //buildHighlightWithColor(Color.grey);
                buildHighlightWithColor(Color.white);
                return;
            }
            if (hasFlag(HighlightType.AnimationTarget) && hasFlag(HighlightType.LinkParent) && hasFlag(HighlightType.LinkTarget)) {
                //LinkedMovement.Log("AnimationTarget, LinkParent, LinkTarget");
                buildHighlightWithColor(Color.white);
                return;
            }
            if (hasFlag(HighlightType.AnimationTarget) && hasFlag(HighlightType.LinkParent)) {
                //LinkedMovement.Log("AnimationTarget, LinkParent");
                buildHighlightWithColor(Color.magenta);
                return;
            }
            if (hasFlag(HighlightType.AnimationTarget) && hasFlag(HighlightType.LinkTarget)) {
                //LinkedMovement.Log("AnimationTarget, LinkTarget");
                buildHighlightWithColor(Color.cyan);
                return;
            }
            if (hasFlag(HighlightType.LinkParent) && hasFlag(HighlightType.LinkTarget)) {
                //LinkedMovement.Log("LinkParent, LinkTarget");
                buildHighlightWithColor(Color.yellow);
                return;
            }
            if (hasFlag(HighlightType.AnimationTarget)) {
                //LinkedMovement.Log("AnimationTarget");
                buildHighlightWithColor(Color.blue);
                return;
            }
            if (hasFlag(HighlightType.LinkParent)) {
                //LinkedMovement.Log("LinkParent");
                buildHighlightWithColor(Color.red);
                return;
            }
            if (hasFlag(HighlightType.LinkTarget)) {
                //LinkedMovement.Log("LinkTarget");
                buildHighlightWithColor(Color.green);
                return;
            }
            //LinkedMovement.Log("NONE! " + currentFlags.ToString());
        }

        private void buildHighlightWithColor(Color color) {
            LinkedMovement.Log("LMHighlightComponent.buildHighlightWithColor " + color.ToString());
            var buildableObject = LMUtils.GetBuildableObjectFromGameObject(this.gameObject);
            List<Renderer> renderers = new List<Renderer>();
            buildableObject.retrieveRenderersToHighlight(renderers);
            highlightHandle = HighlightOverlayController.Instance.add(renderers, -1, color);
        }
    }
}
