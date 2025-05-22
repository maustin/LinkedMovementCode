// ATTRIB: HideScenery
using LinkedMovement.Selection;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LinkedMovement {
    class SelectionHandler : MonoBehaviour {
        private readonly CustomSelectionTool tool = new();
        internal Calc calc;
        public int NumberOfHiddenObjects => tool.NumberOfSelectedObjects;
        public readonly Options Options = new();
        private Park park;
        public bool ShowGui = true;
        public LinkedMovementController controller;

        private void Awake() {
            LinkedMovement.Log("SelectionHandler Awake");
            park = GameController.Instance.park;

            Options.Changed += OnOptionsChanged;
            tool.OnAddedSelectedObject += OnAddedSelectedObject;
            tool.OnRemovedSelectedObject += OnRemovedSelectedObject;
            tool.OnRemoved += OnToolDisabled;
            tool.DeselectOnRemove = false;

            calc = new Calc(this);
        }

        private void OnEnable() {
            LinkedMovement.Log("SelectionHandler OnEnable");
            //foreach (var o in tool.GetSelectedObjects()) {
            //    LinkedMovement.Log("SelectionHandler OnEnable ADD EXISTING");
            //    OnAddedSelectedObject(o);
            //}
            Injector.Instance.Apply(calc.BuildableObjectVisibility);
            tool.CalcIndividualVisibility = calc.BuildableObjectVisibility;
            tool.CalcBoxAction = calc.BoxAction;
        }

        // OLD UI call
        //private UI.InGame.MainWindow ui;
        //private void OnGUI() {
        //    if (ShowGui) {
        //        (ui ??= new UI.InGame.MainWindow(this)).Show();
        //    }
        //    else {
        //        UI.InGame.Indicator.Show();
        //    }
        //}

        private void OnDisable() {
            LinkedMovement.Log("SelectionHandler OnDisable");
            //LinkedMovement.Log(System.Environment.StackTrace);
            GameController.Instance.removeMouseTool(tool);

            tool.CalcIndividualVisibility = IndividualSelectionTool.DefaultVisibility;
            tool.CalcBoxAction = BoxSelectionTool.DefaultAction;
            Injector.Instance.Remove();
            //foreach (var o in tool.GetSelectedObjects()) {
            //    OnRemovedSelectedObject(o);
            //}
            DeselectAll();
        }

        private void OnDestroy() {
            LinkedMovement.Log("SelectionHandler OnDestroy");
            Options.Changed -= OnOptionsChanged;
            tool.OnAddedSelectedObject -= OnAddedSelectedObject;
            tool.OnRemovedSelectedObject -= OnRemovedSelectedObject;
            tool.OnRemoved -= OnToolDisabled;

            park = null;
            calc = null;
        }

        private void OnToolDisabled() {
            Options.Mode = Mode.None;
        }
        private void OnOptionsChanged(Options options, string property) {
            switch (property) {
                case nameof(Options.Transparency):
                    UpdateTransparency();
                    break;
                case nameof(Options.Mode):
                    ApplyMode();
                    break;
            }
        }

        private void ApplyMode() {
            LinkedMovement.Log($"Apply mode {Options.Mode}");
            switch (Options.Mode) {
                case Mode.Individual:
                    tool.Mode = Mode.Individual;
                    GameController.Instance.enableMouseTool(tool);
                    break;
                case Mode.Box:
                    tool.Mode = Mode.Box;
                    GameController.Instance.enableMouseTool(tool);
                    break;
                case Mode.None:
                    tool.Mode = Mode.None;
                    GameController.Instance.removeMouseTool(tool);
                    break;
            }
        }

        private Material _highlightMaterial = null;
        private Material HighlightMaterial {
            get {
                if (_highlightMaterial == null) {
                    _highlightMaterial = (Material)
                      typeof(Park)
                        .GetField("seeThroughMaterialInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(GameController.Instance.park);
                }
                return _highlightMaterial;
            }
        }

        public const string HideSceneryTag = "HideSceneryObject";
        private readonly List<SerializedMonoBehaviour> isHiddenBuffer = new();
        public bool IsHidden(BuildableObject o) {
            o.retrieveObjectsBelongingToThis(isHiddenBuffer);
            foreach (var c in isHiddenBuffer) {
                if (!Utility.isMaterialManagerApplied(o.transform, HideSceneryTag)) {
                    return false;
                }
            }
            isHiddenBuffer.Clear();
            return true;
        }

        private void UpdateTransparency() {
            // from Park.updateSeeThroughObjectsMaterialAlpha
            var color = HighlightMaterial.color;
            color.a = Mathf.Lerp(0.01960784f, 0.1764706f, Options.Transparency);
            HighlightMaterial.color = color;
        }

        //private readonly List<SerializedMonoBehaviour> selectedObjectBuffer = new();
        private void OnAddedSelectedObject(BuildableObject o) {
            if (o == null) {
                return;
            }

            LinkedMovement.Log($"OnAdd: {o.GetType().Name} -- {o.getName()}");
            controller.setSelectedBuildableObject(o);
            //DeselectAll();
            //return;

            //o.retrieveObjectsBelongingToThis(selectedObjectBuffer);
            //foreach (var c in selectedObjectBuffer) {
            //    Utility.attachMaterialManagerByObject(c, HideSceneryTag, HighlightMaterial, null, true);
            //}
            //selectedObjectBuffer.Clear();
        }
        private void OnRemovedSelectedObject(BuildableObject o) {
            if (o == null) {
                return;
            }

            // Mod.DebugLog($"OnRemove: {o.GetType().Name} -- {o.getName()}");

            //o.retrieveObjectsBelongingToThis(selectedObjectBuffer);
            //foreach (var c in selectedObjectBuffer) {
            //    Utility.destroyMaterialManagerByObject(c, HideSceneryTag);
            //}
            //selectedObjectBuffer.Clear();
        }
        public void DeselectAll() {
            LinkedMovement.Log("SelectionHandler DeselectAll");
            //LinkedMovement.Log(System.Environment.StackTrace);
            tool.DeselectAll();
        }

        private void Update() {
            if (GameController.Instance.isActiveMouseTool(tool)) {
                tool.tick();
            }
        }

        private Visibility CalcVisibility(BuildableObject o) {
            if (o.isPreview) {
                return Visibility.Ignore;
            }

            var isDeco = o is Deco;
            var isPath = o is Path;
            if (!(isDeco || isPath)) {
                return Visibility.Ignore;
            }

            if (isDeco && park.hideSceneryEnabled) {
                return Visibility.Ignore;
            }
            else if (isPath && park.hidePathsEnabled) {
                return Visibility.Ignore;
            }

            if (IsHidden(o)) {
                return Visibility.Hidden;
            }
            else {
                return Visibility.Visible;
            }
        }

        public void SceneryAbove(float height, SelectionOperation op) {
            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(0.0f, height, 0.0f), new Vector3(Park.MAX_SIZE, 10_000.0f, Park.MAX_SIZE));
            SceneryInside(bounds, op);
        }
        public void SceneryBelow(float height, SelectionOperation op) {
            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(0.0f, -10_000.0f, 0.0f), new Vector3(Park.MAX_SIZE, height, Park.MAX_SIZE));
            SceneryInside(bounds, op);
        }
        public void SceneryInside(Bounds bounds, SelectionOperation op) {
            LinkedMovement.Log("SceneryInside");
            var mc = MouseCollisions.Instance;

            //todo: cache?
            var results = new List<MouseCollider>();
            mc.octree.getColliding(bounds, results);

            foreach (var coll in results) {
                var o = coll.buildableObject;
                switch (calc.HideSceneryInBoundsAction(op, bounds, o)) {
                    case SelectionAction.Add:
                        tool.Add(o);
                        break;
                    case SelectionAction.Remove:
                        tool.Remove(o);
                        break;
                    case SelectionAction.DoNothing:
                    default:
                        break;
                }
            }
        }
    }
}
