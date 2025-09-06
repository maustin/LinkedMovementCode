using RapidGUI;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicAnimationSubContent : IDoGUI {
        private IDoGUI animateSubContent;

        public EditAnimatronicAnimationSubContent() {
            animateSubContent = new CreateAnimationSubContent();
        }

        public void DoGUI() {
            animateSubContent.DoGUI();
        }
    }
}
