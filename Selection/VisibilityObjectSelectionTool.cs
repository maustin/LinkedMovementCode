// ATTRIB: HideScenery
using Parkitect.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GameController;

namespace LinkedMovement.Selection {
    internal delegate void OnAddedSelectedObjectHandler(BuildableObject selectedObject);
    internal delegate void OnRemovedSelectedObjectHandler(BuildableObject selectedObject);
    public enum Mode {
        None,
        Individual,
        Box,
    }

    internal enum SelectionAction {
        DoNothing,
        Add,
        Remove,
    }
    internal enum SelectionOperation {
        Add,
        Remove,
    }
    internal delegate SelectionAction CalcAction(SelectionOperation op, BuildableObject o);
    internal delegate SelectionAction CalcBoxAction(SelectionOperation op, Bounds bounds, BuildableObject o);
    internal interface ICustomSelectionTool {
        void OnEnable();
        void OnDisable();

        void Tick();
        event OnAddedSelectedObjectHandler OnAddedSelectedObject;
        event OnRemovedSelectedObjectHandler OnRemovedSelectedObject;
    }
    internal sealed class CustomSelectionTool : AbstractMouseTool, IMouseToolTicked, IMouseTool {
        static ICustomSelectionTool ToolInstance;
        public override bool canEscapeMouseTool() => true;
        public override GameController.GameFeature getDisallowedFeatures()
          => GameFeature.Picking | GameFeature.Delete | GameFeature.DragDelete;

        private Mode mode = Mode.Individual;
        public Mode Mode {
            get => mode;
            set {
                if (value != mode) {
                    var prev = mode;
                    mode = value;
                    OnModeChanged(value, prev);
                }
            }
        }
        private void OnModeChanged(Mode newMode, Mode prevMode) {
            LinkedMovement.Log($"OnModeChanged Switching mode from {prevMode} to {newMode}");
            DisableTool(prevMode);
            EnableTool(newMode);
        }
        private void DisableTool(Mode mode) {
            if (TryGetToolFor(mode, out var tool)) {
                tool.OnDisable();
                UnregisterEvents(tool);
            }
        }
        private void EnableTool(Mode mode) {
            if (TryGetToolFor(mode, out var tool)) {
                tool.OnEnable();
                RegisterEvents(tool);
            }
        }
        private void DisableCurrentTool()
          => DisableTool(mode);
        private void EnableCurrentTool()
          => EnableTool(mode);
        private bool TryGetToolFor(Mode mode, out ICustomSelectionTool tool) {
            switch (mode) {
                case Mode.Individual:
                    tool = individualSelectionTool;
                    return true;
                case Mode.Box:
                    tool = boxSelectionTool;
                    return true;
                default:
                    tool = default;
                    return false;
            }
        }
        private bool TryGetCurrentTool(out ICustomSelectionTool tool)
          => TryGetToolFor(mode, out tool);
        private void RegisterEvents(ICustomSelectionTool tool) {
            var toolType = tool.GetType();
            //LinkedMovement.Log("CustomSelectionTool RegisterEvents for tool: " + toolType.Name);

            if (ToolInstance == null) {
                //LinkedMovement.Log("CustomSelectionTool set ToolInstance and setup");
                ToolInstance = tool;
            }
            else if (ToolInstance != tool) {
                //LinkedMovement.Log("CustomSelectionTool different ToolInstance, disable old and setup new");
                ToolInstance.OnDisable();
                UnregisterEvents(ToolInstance);

                ToolInstance = tool;
            }
            else {
                //LinkedMovement.Log("CustomSelectionTool same ToolInstance, skip setup");
                return;
            }

            tool.OnAddedSelectedObject += Add;
            tool.OnRemovedSelectedObject += Remove;
        }
        private void UnregisterEvents(ICustomSelectionTool tool) {
            //LinkedMovement.Log("CustomSelectionTool UnregisterEvents");
            tool.OnAddedSelectedObject -= Add;
            tool.OnRemovedSelectedObject -= Remove;
            ToolInstance = null;
        }

        public event OnAddedSelectedObjectHandler OnAddedSelectedObject;
        public event OnRemovedSelectedObjectHandler OnRemovedSelectedObject;
        private readonly HashSet<BuildableObject> selectedObjects = new HashSet<BuildableObject>();
        public IEnumerable<BuildableObject> GetSelectedObjects() {
            return selectedObjects;
        }
        public int NumberOfSelectedObjects => selectedObjects.Count;
        public bool DeselectOnRemove = false;
        public void Add(BuildableObject o) {
            //LinkedMovement.Log("CustomSelectionTool Add");
            if (selectedObjects.Add(o)) {
                OnAdd(o);
            }
        }
        private void OnAdd(BuildableObject o) {
            //LinkedMovement.Log("CustomSelectionTool OnAdd");
            OnAddedSelectedObject?.Invoke(o);
        }
        public void Remove(BuildableObject o) {
            if (selectedObjects.Remove(o)) {
                OnRemove(o);
            }
        }
        private void OnRemove(BuildableObject o) {
            OnRemovedSelectedObject?.Invoke(o);
        }
        public void DeselectAll() {
            foreach (var o in selectedObjects) {
                OnRemove(o);
            }
            selectedObjects.Clear();
        }

        private readonly IndividualSelectionTool individualSelectionTool = new IndividualSelectionTool();
        public CalcAction CalcIndividualAction {
            get => individualSelectionTool.CalcAction;
            set => individualSelectionTool.CalcAction = value;
        }
        public HitUtility.CalcVisibility CalcIndividualVisibility {
            get => individualSelectionTool.CalcVisibility;
            set => individualSelectionTool.CalcVisibility = value;
        }
        private readonly BoxSelectionTool boxSelectionTool = new BoxSelectionTool();
        public CalcBoxAction CalcBoxAction {
            get => boxSelectionTool.CalcAction;
            set => boxSelectionTool.CalcAction = value;
        }

        public void tick() {
            if (TryGetCurrentTool(out var tool)) {
                tool.Tick();
            }
        }

        public override void onMouseToolEnable() {
            //LinkedMovement.Log("CustomSelectionTool onMouseToolEnable");
            base.onMouseToolEnable();
            EnableCurrentTool();
        }
        public override void onMouseToolDisable() {
            base.onMouseToolDisable();
            DisableCurrentTool();
        }
        public override void onMouseToolRemove() {
            base.onMouseToolRemove();
            if (DeselectOnRemove) {
                DeselectAll();
            }
        }
    }
    internal sealed class IndividualSelectionTool : ICustomSelectionTool {
        public event OnAddedSelectedObjectHandler OnAddedSelectedObject;
        private void OnAdd(BuildableObject o) {
            //LinkedMovement.Log("IndividualSelectionTool OnAdd");
            OnAddedSelectedObject?.Invoke(o);
        }
        public event OnRemovedSelectedObjectHandler OnRemovedSelectedObject;
        private void OnRemove(BuildableObject o) => OnRemovedSelectedObject?.Invoke(o);

        private void UpdateHintMessages(bool show) {
            // overlaps with gui
            // HintMessages.Instance.setVisible("ObjectSelectionAdd", show);
            // HintMessages.Instance.setVisible("ObjectSelectionRemove", show);
            HintMessages.Instance.hideAll();
        }
        public void OnEnable() {
            CursorManager.Instance.setCursorType(CursorType.ADD_OBJECT);

            UpdateHintMessages(show: true);
        }

        public void OnDisable() {
            UpdateHintMessages(show: false);

            CursorManager.Instance.setCursorType(CursorType.DEFAULT);
            UITooltipController.Instance.hideTooltip();
        }

        public CalcAction CalcAction = DefaultAction;
        public static SelectionAction DefaultAction(SelectionOperation op, BuildableObject o) {
            return op switch {
                SelectionOperation.Add => SelectionAction.Add,
                SelectionOperation.Remove => SelectionAction.Remove,
                _ => throw new InvalidOperationException("Unknown Operation " + op),
            };
        }
        public HitUtility.CalcVisibility CalcVisibility = DefaultVisibility;
        public static Visibility DefaultVisibility(BuildableObject o) {
            //todo: implement?
            return Visibility.Ignore;
        }

        /// <summary>
        /// List for HitUtility
        /// </summary>
        private List<BuildableObjectBelowMouseInfo> hits = new();
        /// <summary>
        /// List to detect changes in consecutive hits
        /// </summary>
        private List<BuildableObjectBelowMouseInfo> currentObjects = new();

        private int ObjectsCount => currentObjects.Count;
        private bool HasObjects => currentObjects.Count > 0;
        private bool TryGetLast(out BuildableObjectBelowMouseInfo o) {
            if (currentObjects.Count > 0) {
                o = currentObjects[currentObjects.Count - 1];
                return true;
            }
            else {
                o = default;
                return false;
            }
        }
        private bool HasVisibleObject
          => TryGetLast(out var o) && o.HitVisibility == Visibility.Visible;
        private int NumberOfHiddenObjects
          => HasVisibleObject ? currentObjects.Count - 1 : currentObjects.Count;
        private int selectedHiddenObjectIndex = -1;

        private bool TryGetVisibleObject(out BuildableObjectBelowMouseInfo o) {
            if (TryGetLast(out var om) && om.HitVisibility == Visibility.Visible) {
                o = om;
                return true;
            }
            else {
                o = default;
                return false;
            }
        }
        private bool TryGetSelectedHiddenObject(out BuildableObjectBelowMouseInfo o) {
            if (selectedHiddenObjectIndex < 0) {
                o = default;
                return false;
            }
            else {
                Debug.Assert(selectedHiddenObjectIndex >= 0 && selectedHiddenObjectIndex < currentObjects.Count);
                o = currentObjects[selectedHiddenObjectIndex];
                return true;
            }
        }

        public void Tick() {
            // true when objects below mouse are different from previous tick
            var objectsChanged = false;
            // true when selected hidden object changed
            var selectedObjectChanged = false;
            // true when mouse down
            var mouseDown = false;

            HitUtility.GetObjectsBelowMouse(CalcVisibility, hits);

            // check if changed compared to previous hits
            objectsChanged = hits.Count != currentObjects.Count;
            if (!objectsChanged) {
                Debug.Assert(hits.Count == currentObjects.Count);
                for (int i = hits.Count - 1; i >= 0; i--) {
                    var h = hits[i];
                    var c = currentObjects[i];
                    if (h.HitVisibility != c.HitVisibility || h.HitObject != c.HitObject) {
                        objectsChanged = true;
                        break;
                    }
                }
            }

            if (objectsChanged) {
                UpdateCurrentObjects();
                selectedObjectChanged = true;
            }

            hits.Clear();

            //handle mouse drag? not really usable for hidden objects?
            if (UIUtility.isMouseUsable()) {
                // add objects
                if (Input.GetMouseButtonDown(0)) {
                    if (TryGetVisibleObject(out var o)) {
                        OnSelectedObject(SelectionOperation.Add, o.HitObject);
                        mouseDown = true;
                    }
                }
                // remove
                else if (Input.GetMouseButtonDown(1)) {
                    if (TryGetSelectedHiddenObject(out var o)) {
                        OnSelectedObject(SelectionOperation.Remove, o.HitObject);
                        mouseDown = true;
                    }
                }
                else if (!objectsChanged) {
                    selectedObjectChanged = HandleChangeSelectedHiddenObject();
                }

                ShowTooltip(mouseDown, objectsChanged, selectedObjectChanged);
            }
        }

        private void OnSelectedObject(SelectionOperation op, BuildableObject o) {
            LinkedMovement.Log("OnSelectedObject");
            switch (CalcAction(op, o)) {
                case SelectionAction.Add:
                    OnAdd(o);
                    break;
                case SelectionAction.Remove:
                    OnRemove(o);
                    break;
            }
            // Don't like the deep linking but.. eh
            LinkedMovement.Controller.endSelection();
        }
        private bool HandleChangeSelectedHiddenObject() {
            if (NumberOfHiddenObjects > 1) {
                // TODO: this
                LinkedMovement.Log("HandleChangeSelectedHiddenObject: TODO");
                //if (InputManager.getKeyDown(KeyHandler.MoveHiddenCloserKeyIdentifier)) {
                //    var newIdx = selectedHiddenObjectIndex - 1;
                //    if (newIdx >= 0) {
                //        selectedHiddenObjectIndex = newIdx;
                //        return true;
                //    }
                //}
                //else if (InputManager.getKeyDown(KeyHandler.MoveHiddenAwayIdentifier)) {
                //    var newIdx = selectedHiddenObjectIndex + 1;
                //    if (newIdx < NumberOfHiddenObjects) {
                //        selectedHiddenObjectIndex = newIdx;
                //        return true;
                //    }
                //}
            }
            return false;
        }

        private void UpdateCurrentObjects() {
            // make new found object hierarchy selected
            var t = currentObjects;
            currentObjects = hits;
            hits = t;

            // mark closest hidden object as selected
            // -> last or 2nd last element
            if (currentObjects.Count > 0) {
                selectedHiddenObjectIndex = currentObjects.Count - 1;
                if (HasVisibleObject) {
                    // only one element that is visible: count = 1 -> idx = 0 -> selected = -1
                    selectedHiddenObjectIndex--;
                }
            }
            else {
                selectedHiddenObjectIndex = -1;
            }
        }

        //todo: change from time to mouse moved?
        private float updateTooltipTimeout = 0.0f;
        private const float updateTooltipEvery = 0.2f;
        private readonly StringBuilder tooltip = new();
        private void ShowTooltip(bool mouseDown, bool objectsChanged, bool objectChanged) {
            void HideTooltip() => UITooltipController.Instance.hideTooltip();

            bool updateTooltip = false;

            if (mouseDown) {
                // mouse down -> either new hidden or new visible object
                //  but registers not now but in next tick loop
                // -> force update in next loop
                updateTooltipTimeout = -1.0f;
                return;
            }
            // objects changed -> need to update tooltip
            if (objectsChanged | objectChanged) {
                updateTooltip = true;
            }
            // update tooltip every now and then (that's how Parkitect does it)
            if (updateTooltipTimeout <= 0.0f) {
                updateTooltip = true;
            }

            if (updateTooltip) {
                updateTooltipTimeout = updateTooltipEvery;

                if (!UIUtility.isMouseOverUIElement()) {
                    if (currentObjects.Count > 0) {
                        void Indent() => tooltip.Append(' ', 2);
                        string GetName(BuildableObject o) {
                            // paths don't have names
                            var name = o.getName();
                            if (string.IsNullOrWhiteSpace(name)) {
                                name = o.GetType().Name;
                            }
                            return name;
                        }
                        // void Name(BuildableObject o) => tooltip.Append(o.getName()).Append(" (").Append(o.getCategoryTag()).Append(')');
                        void Name(BuildableObject o) => tooltip.Append(GetName(o));
                        void EOL() => tooltip.AppendLine();

                        // first show visible object
                        {
                            if (TryGetVisibleObject(out var o)) {
                                Indent(); Name(o.HitObject); EOL();
                            }
                        }
                        tooltip.Append('-', 7); EOL();
                        // then all hidden objects and mark selected one
                        for (int i = NumberOfHiddenObjects - 1; i >= 0; i--) {
                            if (selectedHiddenObjectIndex == i) {
                                tooltip.Append("> ");
                            }
                            else {
                                Indent();
                            }
                            var o = currentObjects[i];
                            Name(o.HitObject); EOL();
                        }

                        UITooltipController.Instance.showTooltip(tooltip.ToString(), true, updateTooltipTimeout * 1.1f);
                        tooltip.Clear();
                    }
                    else {
                        HideTooltip();
                    }
                }
            }
            updateTooltipTimeout -= Time.unscaledDeltaTime;
        }
    }
    internal sealed class BoxSelectionTool : ICustomSelectionTool {
        public event OnAddedSelectedObjectHandler OnAddedSelectedObject;
        private void OnAdd(BuildableObject o) => OnAddedSelectedObject?.Invoke(o);
        public event OnRemovedSelectedObjectHandler OnRemovedSelectedObject;
        private void OnRemove(BuildableObject o) => OnRemovedSelectedObject?.Invoke(o);

        public CalcBoxAction CalcAction = DefaultAction;
        public static SelectionAction DefaultAction(SelectionOperation op, Bounds bounds, BuildableObject o) {
            return op switch {
                SelectionOperation.Add => SelectionAction.Add,
                SelectionOperation.Remove => SelectionAction.Remove,
                _ => throw new InvalidOperationException("Unknown Operation " + op),
            };
        }

        private void UpdateHintMessages(bool show) {
            // overlaps with gui
            // HintMessages.Instance.setVisible("ObjectSelectionBox", show);
            HintMessages.Instance.hideAll();
        }
        public void OnEnable() {
            CursorManager.Instance.setCursorType(CursorType.RECTANGLE_SELECTION);

            UpdateHintMessages(show: true);

            Reset();
        }
        public void OnDisable() {
            UpdateHintMessages(show: false);

            CursorManager.Instance.setCursorType(CursorType.DEFAULT);

            Cleanup();
        }

        private enum RectangleSelectionState {
            PLACE_START,
            DRAG_FLAT,
            DRAG_HEIGHT,
        }
        private RectangleSelectionState rectangleSelectionState = RectangleSelectionState.PLACE_START;
        private GameObject selectionMarker, selectionCube;
        private WireCube selectionOutline;
        private BuilderHeightMarker heightMarkerStart, heightMarkerEnd;
        private const float STEP_HEIGHT = 0.25f;  // from Deco.ctor -> heightChange delta
        private void Reset() {
            rectangleSelectionState = RectangleSelectionState.PLACE_START;

            if (selectionMarker == null) {
                selectionMarker = new GameObject("HideScenery-SelectionMarker");
                selectionCube = GameObject.Instantiate(AssetManager.Instance.selectionCubeGO);
                selectionCube.transform.SetParent(selectionMarker.transform);
                selectionOutline = GameObject.Instantiate(AssetManager.Instance.selectionOutlineGO);
                selectionOutline.transform.SetParent(selectionMarker.transform);
            }
            else {
                selectionMarker.SetActive(true);
                selectionCube.transform.localScale = Vector3.one;
                selectionOutline.setSize(Vector3.one);
            }
            if (heightMarkerStart == null) {
                heightMarkerStart = GameObject.Instantiate(AssetManager.Instance.builderHeightMarkerGO);
                heightMarkerStart.heightChangeDelta = STEP_HEIGHT;
                heightMarkerStart.transform.position = Vector3.zero;
                heightMarkerStart.transform.SetParent(selectionMarker.transform);
            }
            else {
                heightMarkerStart.gameObject.SetActive(true);
            }
            if (heightMarkerEnd == null) {
                heightMarkerEnd = GameObject.Instantiate(AssetManager.Instance.builderHeightMarkerGO);
                heightMarkerEnd.heightChangeDelta = STEP_HEIGHT;
                heightMarkerEnd.transform.position = Vector3.zero;
                heightMarkerEnd.transform.SetParent(selectionMarker.transform);
            }
            else {
                heightMarkerEnd.gameObject.SetActive(true);
            }
        }
        private void Cleanup() {
            //todo: unnecessary -> just set inactive? And Destroy(..) in Dispose?
            if (selectionMarker != null) {
                GameObject.Destroy(selectionMarker);
                selectionMarker = null;
            }
            if (heightMarkerStart != null) {
                GameObject.Destroy(heightMarkerStart);
                heightMarkerStart = null;
            }
            if (heightMarkerEnd != null) {
                GameObject.Destroy(heightMarkerEnd);
                heightMarkerEnd = null;
            }
        }

        private bool mouseDown, changeHeight, didChangeHeight;
        private Vector3 rectangleSelectionPlaceStartDragMouseDown, startPositionOffset, endPositionOffset, changeHeightStartWorldPosition, mouseDownWorldPosition, currentMouseWorldPosition;
        private float changeHeightStartTime;
        public void Tick() {
            var isMouseUsable = UIUtility.isMouseUsable();
            if (isMouseUsable) {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                    mouseDown = true;
                    if (rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                        var hitInfo = GetHitInfo(Input.mousePosition);
                        if (hitInfo.hitSomething) {
                            rectangleSelectionPlaceStartDragMouseDown = hitInfo.hitPosition.RoundToTile();
                        }
                    }
                }
                if (rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                    if (InputManager.getKeyDown("BuildingChangeHeight")) {
                        changeHeight = true;
                        BuilderMousePositionInfo hitInfo = GetHitInfo(Input.mousePosition);
                        if (hitInfo.hitSomething) {
                            changeHeightStartWorldPosition = hitInfo.hitPosition - startPositionOffset;
                        }
                        didChangeHeight = false;
                        changeHeightStartTime = Time.time;
                    }
                    else if (InputManager.getKeyUp("BuildingChangeHeight")) {
                        changeHeight = false;
                        if (!didChangeHeight && Time.time - changeHeightStartTime < 1.0f) {
                            startPositionOffset.y = 0.0f;
                        }
                    }
                }
                if (mouseDown && rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                    BuilderMousePositionInfo hitInfo = GetHitInfo(Input.mousePosition);
                    if (hitInfo.hitSomething) {
                        Vector3 tile = hitInfo.hitPosition.RoundToTile();
                        if (tile.x != rectangleSelectionPlaceStartDragMouseDown.x || tile.z != rectangleSelectionPlaceStartDragMouseDown.z) {
                            RectangleSelectionStart(rectangleSelectionPlaceStartDragMouseDown);
                        }
                    }
                }
                if (mouseDown && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))) {
                    var hitInfo = GetHitInfo(Input.mousePosition);
                    if (hitInfo.hitSomething) {
                        if (rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                            RectangleSelectionStart(hitInfo.hitPosition);
                            return;
                        }
                        if (rectangleSelectionState == RectangleSelectionState.DRAG_FLAT) {
                            rectangleSelectionState = RectangleSelectionState.DRAG_HEIGHT;
                            changeHeight = true;
                            var plane = new UnityEngine.Plane(Vector3.up, mouseDownWorldPosition + startPositionOffset + endPositionOffset);
                            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            if (plane.Raycast(ray, out var enter)) {
                                changeHeightStartWorldPosition = ray.GetPoint(enter) - endPositionOffset;
                            }
                            return;
                        }
                    }
                }
            }
            if (changeHeight) {
                var screenPoint = Camera.main.WorldToScreenPoint(changeHeightStartWorldPosition);
                var num2 = Mathf.Max(20f, Vector3.Distance(Camera.main.WorldToScreenPoint(changeHeightStartWorldPosition + Vector3.up), screenPoint));
                var height = MathUtility.roundToNearest((Input.mousePosition.y - screenPoint.y) / num2, STEP_HEIGHT);
                if (rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                    if (!startPositionOffset.y.Equals(height)) {
                        didChangeHeight = true;
                    }
                    startPositionOffset.y = height;
                }
                else if (rectangleSelectionState == RectangleSelectionState.DRAG_HEIGHT) {
                    if (!endPositionOffset.y.Equals(height)) {
                        didChangeHeight = true;
                    }
                    endPositionOffset.y = height;
                }
            }
            if (rectangleSelectionState != RectangleSelectionState.PLACE_START) {
                var plane = new UnityEngine.Plane(Vector3.up, mouseDownWorldPosition + startPositionOffset + endPositionOffset);
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out var enter)) {
                    var point = ray.GetPoint(enter);
                    if (rectangleSelectionState == RectangleSelectionState.DRAG_HEIGHT) {
                        currentMouseWorldPosition.y = point.y;
                    }
                    else {
                        currentMouseWorldPosition = point;
                    }
                }
            }
            else if (!changeHeight) {
                var hitInfo = GetHitInfo(Input.mousePosition);
                if (hitInfo.hitSomething) {
                    currentMouseWorldPosition = hitInfo.hitPosition;
                }
            }

            Vector3 vector3_1 = mouseDownWorldPosition + startPositionOffset;
            Vector3 vector3_2 = currentMouseWorldPosition;
            if (rectangleSelectionState == RectangleSelectionState.PLACE_START) {
                vector3_1 = currentMouseWorldPosition + startPositionOffset;
                vector3_2 = currentMouseWorldPosition + startPositionOffset;
            }
            Vector3 lhs = new((float)Mathf.FloorToInt(vector3_1.x + 0.0001f), MathUtility.roundToNearest(vector3_1.y, STEP_HEIGHT), (float)Mathf.FloorToInt(vector3_1.z + 0.0001f));
            Vector3 rhs = new((float)Mathf.FloorToInt(vector3_2.x + 0.0001f), MathUtility.roundToNearest(vector3_2.y, STEP_HEIGHT), (float)Mathf.FloorToInt(vector3_2.z + 0.0001f));
            Vector3 vector3_3 = Vector3.Min(lhs, rhs);
            Vector3 vector3_4 = Vector3.Max(lhs, rhs);
            Vector3 size = vector3_4 - vector3_3 + Vector3.one;
            Vector3 center = vector3_3 + ((vector3_4 - vector3_3) / 2f) + (Vector3.one / 2f);
            selectionCube.transform.localScale = size;
            selectionOutline.setSize(size);
            selectionMarker.transform.position = center;
            heightMarkerStart.transform.position = vector3_3 + new Vector3(0.5f, 0.0f, 0.5f);
            heightMarkerEnd.transform.position = vector3_4 + new Vector3(0.5f, 0.0f, 0.5f);
            heightMarkerEnd.gameObject.SetActive(vector3_3 != vector3_4);
            if (!isMouseUsable || rectangleSelectionState != RectangleSelectionState.DRAG_HEIGHT || (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1))) {
                return;
            }
            mouseDown = false;
            changeHeight = false;
            rectangleSelectionState = RectangleSelectionState.PLACE_START;
            EscapeHierarchy.Instance.remove(OnEscapeRectangleMode);
            selectionCube.transform.localScale = Vector3.one;
            selectionOutline.setSize(Vector3.one);
            Bounds bounds = new(center, size - new Vector3(0.01f, 0.0f, 0.01f));
            Vector3 max = bounds.max;
            max.y -= 0.005f;
            bounds.max = max;
            var thing = MouseCollisions.Instance.boxcastAll(bounds);
            LinkedMovement.Log("Box tool hit #: " + thing.Length.ToString());
            foreach (MouseCollider.HitInfo hitInfo in MouseCollisions.Instance.boxcastAll(bounds)) {
                var o = hitInfo.hitObject.GetComponentInParent<BuildableObject>();
                if (o != null) {
                    if (Input.GetMouseButtonUp(0)) {
                        OnSelectedObject(SelectionOperation.Add, bounds, o);
                    }
                    else if (Input.GetMouseButtonUp(1)) {
                        OnSelectedObject(SelectionOperation.Remove, bounds, o);
                    }
                }
            }
            endPositionOffset = Vector3.zero;
            // Don't like the deep linking but.. eh
            LinkedMovement.Controller.endSelection();
        }

        private void RectangleSelectionStart(Vector3 startPosition) {
            mouseDownWorldPosition = startPosition;
            rectangleSelectionState = RectangleSelectionState.DRAG_FLAT;
            EscapeHierarchy.Instance.push(OnEscapeRectangleMode);
        }
        private void OnEscapeRectangleMode() {
            rectangleSelectionState = RectangleSelectionState.PLACE_START;
            mouseDown = false;
            endPositionOffset = Vector3.zero;
            changeHeight = false;
            EscapeHierarchy.Instance.remove(OnEscapeRectangleMode);
        }
        private static BuilderMousePositionInfo GetHitInfo(Vector3 mousePosition) {
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            var mousePositionInfo = new BuilderMousePositionInfo {
                hitDistance = float.MaxValue
            };
            foreach (var raycastHit in Physics.RaycastAll(ray, float.PositiveInfinity, 4096)) {
                int num = 1 << raycastHit.collider.gameObject.layer;
                mousePositionInfo.hitSomething = true;
                mousePositionInfo.hitObject = raycastHit.collider.gameObject;
                mousePositionInfo.hitPosition = raycastHit.point;
                mousePositionInfo.hitDistance = raycastHit.distance;
                mousePositionInfo.hitNormal = raycastHit.normal;
                mousePositionInfo.hitLayerMask = num;
            }
            return mousePositionInfo;
        }

        private void OnSelectedObject(SelectionOperation op, Bounds bounds, BuildableObject o) {
            switch (CalcAction(op, bounds, o)) {
                case SelectionAction.Add:
                    OnAdd(o);
                    break;
                case SelectionAction.Remove:
                    OnRemove(o);
                    break;
            }
        }
    }
}
