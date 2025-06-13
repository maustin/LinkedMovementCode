using DG.Tweening;
using LinkedMovement.UI.Components;
using LinkedMovement.UI.Utils;
using LinkedMovement.Utils;
using RapidGUI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace LinkedMovement.UI.InGame {
    internal class CreateAmimationContent : IDoGUI {
        private WindowLauncher launcher;
        private LinkedMovementController controller;

        private BuildableObject baseBO;

        private Vector3 startingPosition;
        private Vector3 targetPosition = Vector3.zero;
        //private Vector3 startingRotation;
        //private Vector3 targetRotation = Vector3.zero;

        private bool isTriggerable = false;
        private float toDuration = 1f;
        private string toEase = "";
        private float fromDelay = 0f;
        private float fromDuration = 1f;
        private string fromEase = "";
        private float restartDelay = 0f;

        private Sequence sequence;

        // TODO: Need ability to load existing animation

        public CreateAmimationContent(WindowLauncher launcher) {
            controller = LinkedMovement.GetController();
            baseBO = controller.baseObject;
            startingPosition = baseBO.transform.position;
            this.launcher = launcher;
        }

        public void DoGUI() {
            using (Scope.Vertical()) {
                GUILayout.Label("Animatronitect - Create Animation", RGUIStyle.popupTitle);
                Space(10f);

                using (Scope.Horizontal()) {
                    GUILayout.Label("Is Triggerable?");
                    var newIsTriggerable = RGUI.Field(isTriggerable);
                    if (newIsTriggerable != isTriggerable) {
                        isTriggerable = newIsTriggerable;
                        rebuildSequence();
                    }
                }

                using (Scope.Horizontal()) {
                    GUILayout.Label("Target Position");
                    var newTargetPosition = RGUI.Field(targetPosition);
                    if (newTargetPosition != targetPosition) {
                        targetPosition = newTargetPosition;
                        rebuildSequence();
                    }
                }

                // TO Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Target Animation Duration");
                }

                // TO Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Target Animation Easing");
                }

                Space(20f);

                // FROM Delay
                using (Scope.Horizontal()) {
                    GUILayout.Label("Pause at Target Duration");
                }

                // FROM Duration
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return to Start Duration");
                }

                // FROM Ease
                using (Scope.Horizontal()) {
                    GUILayout.Label("Return to Start Easing");
                }

                GUILayout.FlexibleSpace();

                using (Scope.Horizontal()) {
                    if (Button("Save", Width(65f))) {
                        killSequence();
                        // TODO
                    }
                    GUILayout.FlexibleSpace();
                    if (Button("Cancel", Width(65f))) {
                        killSequence();
                        launcher.CloseWindow();
                    }
                }
            }
        }

        private void killSequence() {
            if (sequence != null) {
                LinkedMovement.Log("kill existing seq");
                sequence.Kill();
                baseBO.transform.position = startingPosition;
                sequence = null;
            }
        }

        private void rebuildSequence() {
            LinkedMovement.Log("rebuildSequence");
            killSequence();

            sequence = DOTween.Sequence();
            //var toTween = baseBO.transform.DOMove(targetPosition, toDuration);
            var toTween = DOTween.To(() => baseBO.transform.position, x => baseBO.transform.position = x, startingPosition + targetPosition, toDuration);
            //toTween.SetEase()
            //var fromTween = baseBO.transform.DOMove(startingPosition, fromDuration);
            var fromTween = DOTween.To(() => baseBO.transform.position, x => baseBO.transform.position = x, startingPosition, fromDuration);
            sequence.Append(toTween);
            sequence.Append(fromTween);
            sequence.SetLoops(-1);
        }
    }
}
