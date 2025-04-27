using System;
using System.Collections.Generic;
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
        private static readonly List<IUIElementDrag> _draggers = new();
        public static bool WasAnyDragging;
        public static bool DraggerHandledThisFrame;
        protected virtual bool MouseInTargetDisplay => true;

        public static PanelManager Instance { get; private set; }
        public static GameObject CanvasRoot { get; private set; }
        public static CanvasScaler Scaler { get; set; }
        public static Canvas Canvas { get; set; }
        public static GameObject PanelHolder { get; private set; }
        // Main control panel
        public MainControlPanel MainPanel { get; private set; }

        public PanelManager()
        {
            Instance = this;
            CreateRootCanvas();
            CreateMainPanel();
        }

        private void CreateMainPanel()
        {
            try
            {
                // Create the main control panel
                MainPanel = new MainControlPanel(PanelHolder);
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
            CanvasRoot = new GameObject("UIBuddyCanvas");
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
        }


        public void Update()
        {
            if (!DraggerHandledThisFrame)
                UpdateDraggers();

            DraggerHandledThisFrame = false;
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

            foreach (var instance in _draggers)
            {
                if (!instance.IsActive)
                    continue;

                instance.Update(state, mousePos);

                if (DraggerHandledThisFrame)
                    break;
            }

            if (WasAnyDragging && state.HasFlag(MouseState.ButtonState.Up))
            {
                foreach (var instance in _draggers)
                    instance.WasDragging = false;
                WasAnyDragging = false;
            }
        }

        public void AddDrag(string gameObjectName)
        {
            var element = new ElementPanel(gameObjectName);

            if(element.Initialize())
                _draggers.Add(element.Dragger);

            // Set this element as the selected element in the main panel
            if (MainPanel != null && !string.IsNullOrEmpty(gameObjectName)
                && MainPanel.SelectedElementPanel == null)
            {
                MainPanel.SelectedElementPanel = element;
            }
        }

        public void Dispose()
        {
            _draggers.Clear();
        }

        public static void SelectPanel(IGenericPanel panel)
        {
            foreach (var drag in _draggers)
                drag.Panel.SelectPanel(false);
            panel.SelectPanel(true);
        }
    }
}
