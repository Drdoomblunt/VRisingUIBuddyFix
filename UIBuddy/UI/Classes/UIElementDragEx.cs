using System;
using UnityEngine;
using UIBuddy.UI.Panel;
using UIBuddy.Managers;

namespace UIBuddy.UI.Classes;

public class UIElementDragEx: IUIElementDrag
{
    // Instance
    public bool AllowDrag => true;

    public Action OnFinishDrag;
    public Action OnBeginDrag;

    // Common
    private Vector2 _initialMousePos;
    private Vector2 _initialValue;

    public IGenericPanel Panel { get; }

    // Dragging
    private RectTransform DraggableArea { get; }

    public bool WasDragging { get; set; }

    public bool IsActive => Rect?.gameObject?.activeSelf ?? false;

    private RectTransform Rect { get; set; }

    public bool IsPinned { get; set; }

    public UIElementDragEx(GameObject dragObject, IGenericPanel panel)
    {
        DraggableArea = dragObject.GetComponent<RectTransform>();
        Rect = panel.RootObject.GetComponent<RectTransform>();
        Panel = panel;
    }

    public void Update(MouseState.ButtonState state, Vector3 rawMousePos)
    {
        if(IsPinned || !AllowDrag || !Panel.IsRootActive) 
            return;

        Vector3 dragPos = DraggableArea.InverseTransformPoint(rawMousePos);
        bool inDragPos = DraggableArea.rect.Contains(dragPos);

        if (state.HasFlag(MouseState.ButtonState.Clicked))
        {
            if (inDragPos)
            {
                PanelManager.DraggerHandledThisFrame = true;
                OnBeginDragging();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Down))
        {
            if (WasDragging)
            {
                PanelManager.DraggerHandledThisFrame = true;
            }

            if (WasDragging)
            {
                OnDrag();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Released))
        {
            if (WasDragging)
            {
                OnEndDrag();
            }
        }
    }

    #region DRAGGING

    private void OnBeginDragging()
    {
        PanelManager.WasAnyDragging = true;
        WasDragging = true;
        _initialMousePos = InputManager.Mouse.Position;
        _initialValue = Rect.anchoredPosition;
        OnBeginDrag?.Invoke();
    }

    private void OnDrag()
    {
        var mousePos = (Vector2)InputManager.Mouse.Position;

        var diff = mousePos - _initialMousePos;

        Rect.anchoredPosition = _initialValue + diff / Panel.GetOwnerScaleFactor();

        Panel.EnsureValidPosition();
    }

    private void OnEndDrag()
    {
        WasDragging = false;

        OnFinishDrag?.Invoke();
    }

    #endregion

}