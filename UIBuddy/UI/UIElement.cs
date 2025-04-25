using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI;

public abstract class UIElement
{
    private readonly GameObject _gameObject;
    public RectTransform Rect;
    public UIElementControlPanel ControlPanel;
    private readonly string _name;
    public bool CanDrag => true;
    public bool IsPinned => false;

    public Canvas OwnerCanvas { get; private set; }
    public CanvasScaler OwnerCanvasScaler { get; private set; }

    protected UIElement(string gameObjectName)
    {
        _name = gameObjectName;
        _gameObject = GameObject.Find(gameObjectName);
    }

    public bool Initialize()
    {
        if (_gameObject == null)
        {
            Plugin.Log.LogWarning($"Failed to initialize UIElement: {_name}");
            return false;
        }

        Rect = _gameObject.GetComponent<RectTransform>();
        OwnerCanvas = _gameObject.GetComponentInParent<Canvas>();
        OwnerCanvasScaler = _gameObject.GetComponentInParent<CanvasScaler>();

        ControlPanel = new UIElementControlPanel(_gameObject);
        return true;
    }

    public virtual void EnsureValidPosition()
    {
        // Prevent panel going outside screen bounds

        Vector2 pos = Rect.anchoredPosition;
        Vector2 dimensions = OwnerCanvasScaler.referenceResolution;

        float halfW = dimensions.x * 0.5f;
        float halfH = dimensions.y * 0.5f;

        float minPosX = -halfW + Rect.rect.width * 0.5f;
        float maxPosX = halfW - Rect.rect.width * 0.5f;
        float minPosY = -halfH + Rect.rect.height * 0.5f;
        float maxPosY = halfH - Rect.rect.height * 0.5f;

        pos.x = Math.Clamp(pos.x, minPosX, maxPosX);
        pos.y = Math.Clamp(pos.y, minPosY, maxPosY);
  
        Rect.anchoredPosition = pos;
    }
}