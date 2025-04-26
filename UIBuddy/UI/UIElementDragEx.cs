using System;
using UIBuddy.Classes;
using UnityEngine;
using UIBuddy.UI.Classes;
using UIBuddy.UI.Panel;

namespace UIBuddy.UI;

public class UIElementDragEx: IUIElementDrag
{
    // Instance
    public bool AllowDrag => true;

    public event Action OnFinishDrag;

    // Common
    private Vector2 _initialMousePos;
    private Vector2 _initialValue;
    private readonly IGenericPanel _panel;

    // Dragging
    public RectTransform DraggableArea => Rect;

    public bool WasDragging { get; set; }

    public bool IsActive => Rect?.gameObject?.activeSelf ?? false;

    public RectTransform Rect { get; private set; }

    public bool IsPinned { get; set; }

    public UIElementDragEx(GameObject dragObject, IGenericPanel panel)
    {
        Rect = dragObject.GetComponent<RectTransform>();
        _panel = panel;
    }

    public void Update(MouseState.ButtonState state, Vector3 rawMousePos)
    {
        if(IsPinned || !AllowDrag) return;

        Vector3 dragPos = DraggableArea.InverseTransformPoint(rawMousePos);
        bool inDragPos = DraggableArea.rect.Contains(dragPos);

        if (state.HasFlag(MouseState.ButtonState.Clicked))
        {
            if (inDragPos)
            {
                //UIPanel.SetActive(true);
                PanelManager.DraggerHandledThisFrame = true;
            }

            if (inDragPos)
            {
                OnBeginDrag();
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

    public void OnBeginDrag()
    {
        PanelManager.WasAnyDragging = true;
        WasDragging = true;
        _initialMousePos = InputManager.Mouse.Position;
        _initialValue = Rect.anchoredPosition;
    }

    public void OnDrag()
    {
        var mousePos = (Vector2)InputManager.Mouse.Position;

        var diff = mousePos - _initialMousePos;

        Rect.anchoredPosition = _initialValue + diff / _panel.GetOwnerScaleFactor();

        _panel.EnsureValidPosition();
    }

    public void OnEndDrag()
    {
        WasDragging = false;

        OnFinishDrag?.Invoke();
    }

    #endregion

}