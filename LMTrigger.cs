//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace LinkedMovement {
//    public class LMTrigger : MonoBehaviour, IEffect {
//        private SerializedMonoBehaviour effectBehaviour;
//        private List<EffectBoxHandle> effectBoxHandles;

//        private void Awake() {
//            effectBehaviour = GetComponent<SerializedMonoBehaviour>();
//        }

//        public SerializedMonoBehaviour getEffectBehaviour() => this.effectBehaviour;

//        EffectRunner.ExecutionHandle execute(EffectEntry effectEntry); // TODO

//        public EffectBoxHandle linkEffectBox(EffectBox effectBox) {
//            if (effectBoxHandles == null)
//                effectBoxHandles = new List<EffectBoxHandle>();
//            EffectBoxHandle effectBoxHandle = new EffectBoxHandle(effectBox);
//            effectBoxHandles.Add(effectBoxHandle);
//            return effectBoxHandle;
//        }

//        public void unlinkEffectBox(EffectBoxHandle effectBoxHandle) {
//            if (effectBoxHandles == null)
//                return;
//            effectBoxHandles.Remove(effectBoxHandle);
//            if (effectBoxHandles.Count != 0)
//                return;
//            effectBoxHandles = null;
//        }

//        public AbstractEditorPanel createEditorPanel(EffectEntry effectEntry, RectTransform parentRectTransform) {
//            AnimationTriggerEffectEditorPanel editorPanel = UnityEngine.Object.Instantiate<AnimationTriggerEffectEditorPanel>(ScriptableSingleton<UIAssetManager>.Instance.animationTriggerEffectEditorPanel, (Transform) parentRectTransform);
//            editorPanel.setEffectEntry(effectEntry);
//            return (AbstractEditorPanel) editorPanel;
//        }

//        public void initializeOnFirstAssignment(EffectEntry effectEntry) {
//            //
//        }

//        public string getName(EffectEntry effectEntry) => effectBehaviour.getName();

//        public Sprite getSprite(EffectEntry effectEntry) {
//            return ScriptableSingleton<UIAssetManager>.Instance.effectIconGeneric;
//        }
//    }
//}
