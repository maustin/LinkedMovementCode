using LinkedMovement.Animation;
using LinkedMovement.Links;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    static class PairingConverter {
        public static void ConvertPairings() {
            LinkedMovement.Log("ConvertPairings to Animations & Links");

            var controller = LinkedMovement.GetController();
            var lmController = LinkedMovement.GetLMController();

            var pairings = controller.getPairings();
            LinkedMovement.Log($"{pairings.Count} Pairings to convert");

            // First we need to stop all associated animations
            // Then we need to Split each Pairing into an Animation and a Link
            // Finally setup the Links, and then the Animations

            // Stop all associated animations
            var baseGameObjects = new List<GameObject>();
            foreach (var pairing in pairings) {
                var baseGameObject = pairing.baseGO;
                if (!baseGameObjects.Contains(baseGameObject)) {
                    baseGameObjects.Add(baseGameObject);
                }
            }
            LinkedMovement.Log($"{baseGameObjects.Count} objects to stop animations");

            LMUtils.EditAssociatedAnimations(baseGameObjects, LMUtils.AssociatedAnimationEditMode.Stop, false);

            // Convert Pairing to Animation
            // Build Links list
            var createdAnimations = new List<LMAnimation>();
            var linksToCreate = new Dictionary<GameObject, List<GameObject>>();
            LinkedMovement.Log($"{pairings.Count} Pairings to convert to Animations");
            foreach (var pairing in pairings) {
                LinkedMovement.Log($"Process Pairing name: {pairing.pairingName}, id: {pairing.pairingId}");

                var animationParams = pairing.pairBase.animParams;
                linksToCreate.Add(pairing.baseGO, pairing.targetGOs);

                var baseGameObject = pairing.baseGO;

                pairing.disconnect();
                // TODO: Do we need to unparent the children?

                var newAnimation = new LMAnimation(animationParams, baseGameObject, true);
                // Don't create new id, will use params id
                newAnimation.setCustomData();
                createdAnimations.Add(newAnimation);
                LinkedMovement.Log($"Created Animation name: {newAnimation.name}, id: {newAnimation.id}");
            }
            // Animations have been created but not started, wait until Links created

            // Create Links
            var createdLinks = new List<LMLink>();
            LinkedMovement.Log($"{linksToCreate.Count} Links to create");
            foreach (KeyValuePair<GameObject, List<GameObject>> linkEntry in linksToCreate) {
                var parentGameObject = linkEntry.Key;
                var targetGameObjects = linkEntry.Value;
                var linkName = $"Link parent {parentGameObject.name} to {targetGameObjects.Count} targets";
                var linkId = LMUtils.GetNewId();
                LinkedMovement.Log($"Create link name: {linkName}, id: {linkId}");

                var parentBuildableObject = LMUtils.GetBuildableObjectFromGameObject(parentGameObject);
                var targetBuildableObjects = LMUtils.GetBuildableObjectsFromGameObjects(targetGameObjects);

                var newLink = new LMLink(linkName, linkId, parentBuildableObject, targetBuildableObjects);
                createdLinks.Add(newLink);
            }

            LinkedMovement.Log("Add Links to Controller and build");
            lmController.setupLinks(createdLinks);
            LinkedMovement.Log("Add Animations to Controller and build");
            lmController.setupAnimations(createdAnimations);

            LinkedMovement.Log("Finished converting Pairings, clear references");
            controller.getPairings().Clear();
        }

        public static bool HasPairingsToConvert() {
            return LinkedMovement.GetController().getPairings().Count > 0;
        }
    }
}
