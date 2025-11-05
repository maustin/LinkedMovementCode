using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class ViewExistingLinksContentNew : LMWindowContent {
        private LMController controller;
        private Vector2 targetsScrollPosition;

        public ViewExistingLinksContentNew() {
            controller = LinkedMovement.GetLMController();
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                var links = controller.getLinks();
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(400f));

                foreach (var link in links) {
                    using (Scope.Horizontal()) {
                        var forceShowHighlight = link.ForceShowHighlight;
                        if (forceShowHighlight) {
                            if (LMStyles.GetIconEyeButton()) {
                                link.ForceShowHighlight = false;
                            }
                        } else {
                            if (LMStyles.GetIconEyeStrikeButton()) {
                                link.ForceShowHighlight = true;
                            }
                        }
                        Space(2f);

                        if (Button(link.name, RGUIStyle.roundedFlatButtonLeft)) {
                            windowManager.removeWindow(this.window);
                            windowManager.createWindow(WindowManager.WindowType.EditLinkNew, link);
                        }
                        Space(3f);
                        if (Button("✕", RGUIStyle.roundedFlatButton, Width(40f))) {
                            controller.queueLinkToRemove(link);
                        }
                    }
                }

                EndScrollView();
            }
        }
    }
}
