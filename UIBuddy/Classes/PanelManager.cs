using System;
using System.Collections.Generic;
using UIBuddy.UI;
using UnityEngine;

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

        public PanelManager()
        {
            Instance = this;
            CreateRootCanvas();
            CreateMainPanel();
        }

        private void CreateMainPanel()
        {
            
        }

        private static void CreateRootCanvas()
        {
            CanvasRoot = new GameObject("UIBuddyCanvas");
            UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
            CanvasRoot.hideFlags |= HideFlags.HideAndDontSave;
            CanvasRoot.layer = 5;
            CanvasRoot.transform.position = new Vector3(0f, 0f, 1f);
            CanvasRoot.SetActive(false);
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
        }

        public void Dispose()
        {
            _draggers.Clear();
        }
    }
}
