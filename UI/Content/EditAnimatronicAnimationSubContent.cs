using RapidGUI;

namespace LinkedMovement.UI.Content {
    internal class EditAnimatronicAnimationSubContent : IDoGUI {
        private IDoGUI animateSubContent;

        // TODO: Can this class be eliminated?
        public EditAnimatronicAnimationSubContent() {
            animateSubContent = new CreateAnimationSubContent();
        }

        public void DoGUI() {
            animateSubContent.DoGUI();
        }
    }
}
