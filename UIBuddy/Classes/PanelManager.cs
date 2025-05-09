using System;
using System.Collections.Generic;
using System.Linq;
using UIBuddy.UI;
using UIBuddy.UI.Classes;
using UIBuddy.UI.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Classes
{
    public class PanelManager: IDisposable
    {
        private Vector3 _previousMousePosition;
        private MouseState.ButtonState _previousMouseButtonState;
        private static readonly List<IUIElementDrag> DraggersList = new();
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
        private static bool _focusHandledThisFrame;

        protected virtual bool ShouldUpdateFocus =>
            MouseInTargetDisplay &&
            InputManager.Mouse.Button0 == MouseState.ButtonState.Down &&
            !WasAnyDragging;

        public PanelManager()
        {
            Instance = this;
            CreateRootCanvas();
            CreateMainPanel();
            CreateElementListPanel();
        }

        private static void CreateElementListPanel()
        {
            ElementListPanel = new ElementListPanel(PanelHolder);
            ElementListPanel.Initialize();
            DraggersList.Add(ElementListPanel.Dragger);
            Plugin.Log.LogInfo("Elements list panel created and initialized successfully");
        }

        private static void CreateMainPanel()
        {
            try
            {
                // Create the main control panel
                MainPanel = new MainControlPanel(PanelHolder);
                MainPanel.Initialize();
                DraggersList.Add(MainPanel.Dragger);
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
            var rect = PanelHolder.GetComponent<RectTransform>();
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

            if (ConfigManager.SelectPanelsWithMouse && ShouldUpdateFocus)
                UpdateFocus();

            foreach (var panel in DraggersList.Select(a=> a.Panel))
            {
                panel.Update();
            }

            if (!DraggerHandledThisFrame)
                UpdateDraggers();

            DraggerHandledThisFrame = false;
            _focusHandledThisFrame = false;
        }

        protected virtual void UpdateFocus()
        {

            // If another UIBase has already handled a user's click for focus, don't update it for this UIBase.
            if (!_focusHandledThisFrame)
            {
                var mousePos = InputManager.Mouse.Position;
   
                var panelsThroughClick = DraggersList.Where(a =>
                        a.Panel.IsRootActive &&
                        a.Panel.RootRect.rect.Contains(a.Panel.RootRect.InverseTransformPoint(mousePos)))
                    .Select(a => a.Panel).ToList();

                if (panelsThroughClick.Count > 1)
                {
                    var filteredPanels = panelsThroughClick.Where(panel => MainPanel.SelectedElementPanel != panel)
                        .ToArray();
                    var panel = filteredPanels.FirstOrDefault();
                    SelectPanel(panel);
                    var dragger = DraggersList.FirstOrDefault(d => d.Panel == panel);
                    DraggersList.Remove(dragger);
                    DraggersList.Add(dragger);
                }
                else
                {
                    var panel = panelsThroughClick.FirstOrDefault();
                    if (panel != null)
                    {
                        SelectPanel(panel);
                        //if(panel is not ElementPanel)
                        //    panel.RootObject.transform.SetAsLastSibling();
                    }
                }
                _focusHandledThisFrame = true;
            }

            //if (!clickedInAny)
            //    OnClickedOutsidePanels?.Invoke();
        }

        private static bool IsControlPanelSelected()
        {
            var mousePos = InputManager.Mouse.Position;
            var dragPos = MainPanel.RootRect.InverseTransformPoint(mousePos);
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
               foreach (var instance in DraggersList.Where(instance =>
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
                foreach (var instance in DraggersList)
                    instance.WasDragging = false;
                WasAnyDragging = false;
            }
        }

        public static void AddDrag(string gameObjectName, string friendlyName)
        {
            if(DraggersList.FirstOrDefault(a=> a.Panel.Name == friendlyName) != null)
                return;

            var element = new ElementPanel(gameObjectName, friendlyName);

            if (element.Initialize())
            {
                DraggersList.Add(element.Dragger);
                ElementListPanel.AddElement(element);
            }
        }


        public static DetachedPanel AddDetachedDrag(string name, string friendlyName, string shortName)
        {
            if (DraggersList.FirstOrDefault(a => a.Panel.Name == friendlyName) != null)
                return null;

            var element = new DetachedPanel(name, friendlyName, shortName, PanelHolder);

            if (element.Initialize())
            {
                DraggersList.Add(element.Dragger);
                ElementListPanel.AddElement(element);
                return element;
            }

            return null;
        }

        public void Dispose()
        {
            foreach (var panel in DraggersList.Select(a=> a.Panel))
                panel.Dispose();
            DraggersList.Clear();
        }

        public static void SelectPanel(IGenericPanel panel)
        {
            if(panel == MainPanel || panel == ElementListPanel)
                return;

            // clear all panels outline
            foreach (var drag in DraggersList)
                drag.Panel.SelectPanelAsCurrentlyActive(false);
            
            // do not select inactive panel
            if (!panel.IsRootActive)
            {
                MainPanel.SelectedElementPanel = null;
                return;
            }

            // select panel
            var dragger = DraggersList.FirstOrDefault(d => d.Panel == panel);
            DraggersList.Remove(dragger);
            DraggersList.Insert(0, dragger);
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

        private static List<IGenericPanel> GetAllPanels()
        {
            return DraggersList.Where(drag => drag.Panel != MainPanel && drag.Panel != ElementListPanel)
                .Select(a => a.Panel).ToList();
        }
    }
}
