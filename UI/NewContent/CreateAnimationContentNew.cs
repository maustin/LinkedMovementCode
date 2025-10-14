using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    public class CreateAnimationContentNew : LMWindowContent {
        private LMController controller;

        private IDoGUI selectSubContent;

        public CreateAnimationContentNew() {
            controller = LinkedMovement.GetLMController();
            controller.editAnimation();

            selectSubContent = new SelectAnimationTargetSubContentNew();
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                // Select target subcontent
                selectSubContent.DoGUI();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                // Animation subcontent
            }
        }
    }
}
