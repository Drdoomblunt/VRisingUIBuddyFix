using System;
using System.Collections.Generic;
using System.Linq;
using UIBuddy.UI;
using UIBuddy.UI.Classes;
using UIBuddy.UI.Panel;
using UIBuddy.UI.ScrollView;
using UIBuddy.UI.ScrollView.Cells;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Classes
{
    public class PanelManager: IDisposable
    {
        private Vector3 _previousMousePosition;
        private MouseState.ButtonState _previousMouseButtonState;
        private static List<ScrollPool<CheckButtonCell>> _pools = new();
        private static readonly List<IUIElementDrag> _draggers = new();
        public static bool WasAnyDragging;
        public static bool DraggerHandledThisFrame;
        protected virtual bool MouseInTargetDisplay => true;
        public static GameObject PoolHolder { get; private set; }

        public static PanelManager Instance { get; private set; }
        public static GameObject CanvasRoot { get; private set; }
        public static CanvasScaler Scaler { get; set; }
        public static Canvas Canvas { get; set; }
        public static GameObject PanelHolder { get; private set; }
        // Main control panel
        public static  MainControlPanel MainPanel { get; private set; }
        public static ElementListPanel ElementListPanel { get; private set; }
        protected internal static bool FocusHandledThisFrame;

        protected virtual bool ShouldUpdateFocus
        {
            get => MouseInTargetDisplay &&
                   (InputManager.Mouse.Button0 == MouseState.ButtonState.Down) &&
                   !WasAnyDragging;
        }

        public PanelManager()
        {
            Instance = this;
            CreateRootCanvas();
            CreateMainPanel();
            CreateElementListPanel();
        }

        private void CreateElementListPanel()
        {
            ElementListPanel = new ElementListPanel(PanelHolder);
            ElementListPanel.Initialize();
            _draggers.Add(ElementListPanel.Dragger);
            Plugin.Log.LogInfo("Elements list panel created and initialized successfully");
        }


        private void CreateMainPanel()
        {
            try
            {
                // Create the main control panel
                MainPanel = new MainControlPanel(PanelHolder);
                MainPanel.Initialize();
                _draggers.Add(MainPanel.Dragger);
                Plugin.Log.LogInfo("Main panel created and initialized successfully");

            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error creating main panel: {ex.Message}");
            }
        }

        private static void CreateRootCanvas()
        {
            CanvasRoot = new GameObject("Canvas");
            var canvasRootRect = CanvasRoot.AddComponent<RectTransform>();
            UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
            CanvasRoot.SetActive(false);
            CanvasRoot.hideFlags |= HideFlags.HideAndDontSave;
            CanvasRoot.layer = 5;
            CanvasRoot.transform.position = new Vector3(0f, 0f, 1f);

            Canvas = CanvasRoot.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.referencePixelsPerUnit = 100;
            Canvas.sortingOrder = 30000;
            Canvas.overrideSorting = true;

            Scaler = CanvasRoot.AddComponent<CanvasScaler>();
            Scaler.referenceResolution = new Vector2(3840, 2160);
            Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            CanvasRoot.AddComponent<GraphicRaycaster>();
            canvasRootRect.anchorMin = Vector2.zero;
            canvasRootRect.anchorMax = Vector2.one;
            canvasRootRect.pivot = new Vector2(0.5f, 0.5f);

            PanelHolder = UIFactory.CreateUIObject("PanelHolder", CanvasRoot);
            RectTransform rect = PanelHolder.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            CanvasRoot.SetActive(true);


            PoolHolder = new GameObject("UIBuddy_PoolHolder");
            PoolHolder.transform.parent = CanvasRoot.transform;
            PoolHolder.SetActive(false);
        }


        public void Update()
        {
            if(!MainPanel.IsRootActive)
                return;

            //if (ShouldUpdateFocus)
            //    UpdateFocus();

            foreach (var pool in _pools)
            {
                pool.Update();
            }

            foreach (var panel in _draggers.Select(a=> a.Panel))
            {
                panel.Update();
            }

            if (!DraggerHandledThisFrame)
                UpdateDraggers();

            DraggerHandledThisFrame = false;
            FocusHandledThisFrame = false;
        }

        protected virtual void UpdateFocus()
        {
            bool clickedInAny = false;

            // If another UIBase has already handled a user's click for focus, don't update it for this UIBase.
            if (!FocusHandledThisFrame)
            {
                Vector3 mousePos = InputManager.Mouse.Position;
                int count = _draggers.Count;// PanelHolder.transform.childCount;


                var panelsThroughClick = _draggers.Where(a =>
                        a.Panel.IsRootActive &&
                        a.Panel.RootRect.rect.Contains(a.Panel.RootRect.InverseTransformPoint(mousePos)))
                    .Select(a => a.Panel).ToList();

                if (panelsThroughClick.Count > 1)
                {
                    foreach (var panel in panelsThroughClick)
                    {
                        if (MainPanel.SelectedElementPanel != panel)
                        {
                            SelectPanel(panel);
                            //if(panel is not ElementPanel)
                            //    panel.RootObject.transform.SetAsLastSibling();
                            break;
                        }
                    }
                }
                else
                {
                    var panel = panelsThroughClick.FirstOrDefault();
                    if (panel != null)
                    {
                        SelectPanel(panel);
                        if(panel is not ElementPanel)
                            panel.RootObject.transform.SetAsLastSibling();
                    }
                }
                FocusHandledThisFrame = true;


                /*for (int i = count - 1; i >= 0; i--)
                {
                    // make sure this is a real recognized panel
                    var panel = _draggers[i].Panel;
                    Transform transform = panel.RootObject.GetComponent<Transform>();

                    // check if our mouse is clicking inside the panel
                    Vector3 pos = panel.RootRect.InverseTransformPoint(mousePos);
                    if (!panel.IsRootActive || !panel.RootRect.rect.Contains(pos)) continue;

                    // Panel was clicked in.
                    focusHandledThisFrame = true;
                    clickedInAny = true;

                   // int offset = CanvasRoot.transform.childCount - RootRect.GetSiblingIndex();
                   // Canvas.sortingOrder = TOP_SORTORDER - offset;


                    // if this is not the top panel, reorder and invoke the onchanged event
                    if (transform.GetSiblingIndex() != count - 1)
                    {
                        // Set the clicked panel to be on top
                        transform.SetAsLastSibling();

                        ///////////InvokeOnPanelsReordered();
                    }

                    break;
                }*/
            }

            //if (!clickedInAny)
            //    OnClickedOutsidePanels?.Invoke();
        }

        public static bool IsControlPanelSelected()
        {
            var mousePos = InputManager.Mouse.Position;
            Vector3 dragPos = MainPanel.RootRect.InverseTransformPoint(mousePos);
            bool inDragPos = MainPanel.RootRect.rect.Contains(dragPos);
            if (inDragPos)
                return true;
            dragPos = ElementListPanel.RootRect.InverseTransformPoint(mousePos);
            inDragPos = ElementListPanel.RootRect.rect.Contains(dragPos);
            if (inDragPos)
                return true;

            return false;
        }

        protected virtual void UpdateDraggers()
        {
            if (!MouseInTargetDisplay)
                return;

            var state = InputManager.Mouse.Button0;
            var mousePos = InputManager.Mouse.Position;

            // If the mouse hasn't changed, we don't need to do any more
            if (mousePos == _previousMousePosition && state == _previousMouseButtonState) 
                return;
            _previousMousePosition = mousePos;
            _previousMouseButtonState = state;

            if (!DraggerHandledThisFrame && MainPanel.IsRootActive)
            {
               /* var selected = MainPanel.SelectedElementPanel?.Dragger;
                if(selected is { IsActive: true } && selected.Panel.IsRootActive)
                {
                    selected.Update(state, mousePos);
                }*/

               //this is where we restrict panel selection for now
               foreach (var instance in _draggers.Where(instance =>
                            instance.IsActive &&
                            ((MainPanel.SelectedElementPanel != null &&
                              instance.Panel == MainPanel.SelectedElementPanel) ||
                             instance.Panel == MainPanel ||
                             instance.Panel == ElementListPanel)))
               {
                   if (!instance.Panel.IsRootActive) continue;

                   if (instance.Panel == MainPanel.SelectedElementPanel && IsControlPanelSelected() && !WasAnyDragging)
                   {
                       DraggerHandledThisFrame = true;
                       break;
                   }

                   instance.Update(state, mousePos);

                   if (DraggerHandledThisFrame)
                       break;
               }
               
            }

            if (WasAnyDragging && state.HasFlag(MouseState.ButtonState.Up))
            {
                foreach (var instance in _draggers)
                    instance.WasDragging = false;
                WasAnyDragging = false;
            }
        }

        public void AddDrag(string gameObjectName, string friendlyName)
        {
            if(_draggers.FirstOrDefault(a=> a.Panel.Name == friendlyName) != null)
                return;

            var element = new ElementPanel(gameObjectName, friendlyName);

            if (element.Initialize())
            {
                _draggers.Add(element.Dragger);
                ElementListPanel.AddElement(element);
            }
        }


        public DetachedPanel AddDetachedDrag(string name, string friendlyName, string shortName)
        {
            if (_draggers.FirstOrDefault(a => a.Panel.Name == friendlyName) != null)
                return null;

            var element = new DetachedPanel(name, friendlyName, shortName, PanelHolder);

            if (element.Initialize())
            {
                _draggers.Add(element.Dragger);
                ElementListPanel.AddElement(element);
                return element;
            }

            return null;
        }

        public void Dispose()
        {
            foreach (var panel in _draggers.Select(a=> a.Panel))
                panel.Dispose();
            _draggers.Clear();
        }

        public static void SelectPanel(IGenericPanel panel)
        {
            if(panel == MainPanel || panel == ElementListPanel)
                return;

            // clear all panels outline
            foreach (var drag in _draggers)
                drag.Panel.SelectPanelAsCurrentlyActive(false);
            
            // do not select inactive panel
            if (!panel.IsRootActive)
            {
                MainPanel.SelectedElementPanel = null;
                return;
            }

            // select panel
            var dragger = _draggers.FirstOrDefault(d => d.Panel == panel);
            _draggers.Remove(dragger);
            _draggers.Insert(0, dragger);
            panel.SelectPanelAsCurrentlyActive(true);
            MainPanel.SelectedElementPanel = panel as ElementPanel;
        }

        public static void SetPanelsActive(bool value)
        {
            foreach (var panel in GetAllPanels())
            {
                panel.SetActive(value);
            }
        }

        public static List<IGenericPanel> GetAllPanels()
        {
            return _draggers.Where(drag => drag.Panel != MainPanel && drag.Panel != ElementListPanel)
                .Select(a => a.Panel).ToList();
        }

    }
}
