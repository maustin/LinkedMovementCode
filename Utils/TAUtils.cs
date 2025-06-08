using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.Utils {
    static class TAUtils {
        static public void AttachTargetToBase(Transform baseObject, Transform targetObject) {
            LinkedMovement.Log("Find attach parent between " + baseObject.name + " and " + targetObject.name);
            var baseTransform = baseObject;
            bool foundPlatform = false;

            var baseChildrenCount = baseTransform.childCount;
            for (var i = 0; i < baseChildrenCount; i++) {
                var child = baseTransform.GetChild(i);
                var childName = child.gameObject.name;
                if (childName.Contains("[Platform]")) {
                    foundPlatform = true;
                    baseTransform = child;
                    //LinkedMovement.Log("Using Platform");
                    break;
                }
            }
            // TODO: Not sure about this case
            if (!foundPlatform && baseChildrenCount > 0) {
                // Take child at 0
                baseTransform = baseTransform.GetChild(0);
                //LinkedMovement.Log("Using child 0");
            }

            targetObject.SetParent(baseTransform);
        }

        static public List<BlueprintFile> FindDecoBlueprints(IList<BlueprintFile> blueprints) {
            var list = new List<BlueprintFile>();
            foreach (var blueprint in blueprints) {
                if (blueprint.getCategoryTags().Contains("Deco")) {
                    list.Add(blueprint);
                }
            }
            return list;
        }

        static public void UpdateMouseColliders(BuildableObject bo) {
            if (bo.mouseColliders != null) {
                foreach (MouseCollider mouseCollider in bo.mouseColliders)
                    mouseCollider.updatePosition();
            }
        }

        static public string GetGameObjectBuildableName(GameObject go) {
            var buildableObject = GetBuildableObjectFromGameObject(go);
            if (buildableObject != null)
                return buildableObject.getName();
            return go.name;
        }

        static public BuildableObject GetBuildableObjectFromGameObject(GameObject go) {
            var buildableObject = go.GetComponent<BuildableObject>();
            return buildableObject;
        }

        static public PairBase GetPairBaseFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairBase pairBase;
            smb.tryGetCustomData(out pairBase);
            return pairBase;
        }

        static public PairTarget GetPairTargetFromSerializedMonoBehaviour(SerializedMonoBehaviour smb) {
            PairTarget pairTarget;
            smb.tryGetCustomData(out pairTarget);
            return pairTarget;
        }
    }
}
