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
    public Transform Transform { get; set; }
    public Vector2 ReferenceResolution { get; set; }

    // Track the original scale to allow proper reset
    private float _originalScaleFactor;

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
        OwnerCanvasScaler = _gameObject.GetComponent<CanvasScaler>();
        if (OwnerCanvasScaler == null)
            ReferenceResolution = _gameObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ?? Vector2.one;

        if (OwnerCanvasScaler != null)
            _originalScaleFactor = OwnerCanvasScaler.scaleFactor;
        Transform = _gameObject.GetComponent<Transform>();
        if(OwnerCanvasScaler == null)
            _originalScaleFactor = Transform.localScale.x;

        ControlPanel = new UIElementControlPanel(_gameObject, _originalScaleFactor != 0f ? _originalScaleFactor : 1f);
        ControlPanel.ScaleChanged += OnScaleChanged;
        return true;
    }


    private void OnScaleChanged(float value)
    {
        ApplyScale(value);
    }

    protected void ApplyScale(float value)
    {
        if (OwnerCanvasScaler == null && Transform == null)
            return;

        try
        {
            if (OwnerCanvasScaler != null)
            {

                // Use reflection to set the scaleFactor property
                var scaleFactorField = AccessTools.Field(typeof(CanvasScaler), "m_ScaleFactor");
                if (scaleFactorField != null)
                {
                    scaleFactorField.SetValue(OwnerCanvasScaler, value);

                    // Force a canvas update
                    if (OwnerCanvas != null)
                    {
                        OwnerCanvas.scaleFactor = value;
                        Canvas.ForceUpdateCanvases();
                    }

                    Plugin.Log.LogInfo($"Scale updated to {value} for {_name}");
                }
                else
                {
                    Plugin.Log.LogWarning($"Could not find scaleFactor field for {_name}");
                }
            }
            else
            {
                Transform.localScale = new Vector3(value, value, 1f);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error applying scale to {_name}: {ex.Message}");
        }
    }

    public void ResetScale()
    {
        ApplyScale(_originalScaleFactor);
    }

    public virtual void EnsureValidPosition()
    {
        // Prevent panel going outside screen bounds

        Vector2 pos = Rect.anchoredPosition;
        Vector2 dimensions = ReferenceResolution;

        var x = Rect.anchorMax.x;
        var y = Rect.anchorMax.y;
        var mx = Rect.anchorMin.x;
        var my = Rect.anchorMin.y;

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