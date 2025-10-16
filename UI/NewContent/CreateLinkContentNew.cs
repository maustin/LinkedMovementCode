using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    public class CreateLinkContentNew : LMWindowContent {
        private LMController controller;

        private IDoGUI selectParentSubContent;
        private IDoGUI selectTargetsSubContent;

        public CreateLinkContentNew() {
            controller = LinkedMovement.GetLMController();

            selectParentSubContent = new SelectLinkParentSubContentNew();
            selectTargetsSubContent = new SelectLinkTargetsSubContentNew();

            controller.editLink();
        }

        public override void DoGUI() {
            if (controller.currentLink == null) {
                return;
            }

            base.DoGUI();

            using (Scope.Vertical()) {
                // TODO: Info i

                // select parent sub
                selectParentSubContent.DoGUI();

                HorizontalLine.DrawHorizontalLine(Color.grey);

                // select targets sub
                var hasParent = controller.currentLink.hasParent();
                if (hasParent) {
                    selectTargetsSubContent.DoGUI();
                }
            }
        }
    }
}
