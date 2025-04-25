using System;
using System.Collections.Generic;
using UIBuddy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Classes
{
    public class PanelManager: IDisposable
    {
        private Vector3 _previousMousePosition;
        private MouseState.ButtonState _previousMouseButtonState;
        private readonly List<UIElementDrag> _draggers = new();
        public static bool WasAnyDragging;
        public static bool DraggerHandledThisFrame;
        protected virtual bool MouseInTargetDisplay => true;
        public static PanelManager Instance { get; private set; }
        public static GameObject CanvasRoot { get; private set; }

        // Main control panel
        public UILayer MainPanel { get; private set; }

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
                MainPanel = new UILayer();

                // Initialize it
                if (MainPanel.Initialize())
                {
                    // Add to draggers
                    _draggers.Add(MainPanel);

                    Plugin.Log.LogInfo("Main panel created and initialized successfully");
                }
                else
                {
                    Plugin.Log.LogError("Failed to initialize the main panel");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error creating main panel: {ex.Message}");
            }
        }

        private static void CreateRootCanvas()
        {
            CanvasRoot = new GameObject("UIBuddyCanvas");
            UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
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

            CanvasRoot.SetActive(false);
            CanvasRoot.SetActive(true);
        }

        public static CanvasScaler Scaler { get; set; }

        public static Canvas Canvas { get; set; }

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
                if (!instance.Rect.gameObject.activeSelf)
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
            var element = new UIElementDrag(gameObjectName);

            if(element.Initialize())
                _draggers.Add(element);

            // Set this element as the selected element in the main panel
            if (MainPanel != null && !string.IsNullOrEmpty(gameObjectName)
                && MainPanel.SelectedUIElement == null)
            {
                MainPanel.SelectedUIElement = element;
            }
        }

        public void Dispose()
        {
            _draggers.Clear();
        }
    }
}
