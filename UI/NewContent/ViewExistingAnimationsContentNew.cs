using LinkedMovement.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.NewContent {
    internal class ViewExistingAnimationsContentNew : LMWindowContent {
        private LMController controller;
        private Vector2 targetsScrollPosition;
        
        public ViewExistingAnimationsContentNew() {
            controller = LinkedMovement.GetLMController();
        }

        public override void DoGUI() {
            base.DoGUI();

            using (Scope.Vertical()) {
                var animations = controller.getAnimations();
                targetsScrollPosition = BeginScrollView(targetsScrollPosition, Height(400f));

                foreach (var animation in animations) {
                    using (Scope.Horizontal()) {
                        var forceShowHighlight = animation.ForceShowHighlight;
                        if (forceShowHighlight) {
                            if (LMStyles.GetIconEyeButton()) {
                                animation.ForceShowHighlight = false;
                            }
                        } else {
                            if (LMStyles.GetIconEyeStrikeButton()) {
                                animation.ForceShowHighlight = true;
                            }
                        }
                        Space(2f);
                        
                        if (Button(animation.name, RGUIStyle.roundedFlatButtonLeft)) {
                            LinkedMovement.Log("TEST CLICKED edit animation");
                            
                            windowManager.removeWindow(this.window);
                            windowManager.createWindow(WindowManager.WindowType.EditAnimationNew, animation);
                        }
                        Space(3f);
                        if (Button("✕", RGUIStyle.roundedFlatButton, Width(40f))) {
                            LinkedMovement.Log("TEST CLICKED delete animation");
                            
                            controller.queueAnimationToRemove(animation);
                        }
                    }
                }

                EndScrollView();
            }
        }
    }
}
