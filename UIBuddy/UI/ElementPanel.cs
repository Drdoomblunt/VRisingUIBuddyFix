using System;
using HarmonyLib;
using UIBuddy.UI.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI;

public class ElementPanel: IGenericPanel
{
    public RectTransform Rect;
    public readonly string Name;
    public bool CanDrag => true;
    public bool IsPinned => false;

    public Canvas OwnerCanvas { get; private set; }
    public CanvasScaler OwnerCanvasScaler { get; private set; }
    public Transform Transform { get; set; }
    public GameObject RootObject { get; }
    public Vector2 ReferenceResolution { get; set; }

    public RectTransform CustomUIRect { get; set; }
    public GameObject CustomUIObject { get; set; }
    public UIElementDragEx Dragger { get; protected set; }
    protected Outline Outline { get; set; }

    // Track the original scale to allow proper reset
    private float _originalScaleFactor;

    public ElementPanel(string gameObjectName)
    {
        Name = gameObjectName;
        RootObject = GameObject.Find(gameObjectName);
    }

    public bool Initialize()
    {
        if (RootObject == null)
        {
            Plugin.Log.LogWarning($"Failed to initialize UIElement: {Name}");
            return false;
        }

        Rect = RootObject.GetComponent<RectTransform>();
        OwnerCanvas = RootObject.GetComponentInParent<Canvas>();
        OwnerCanvasScaler = RootObject.GetComponent<CanvasScaler>();

        ReferenceResolution = OwnerCanvasScaler?.referenceResolution ??
                              RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ?? 
                              Vector2.one;

        if (OwnerCanvasScaler != null)
            _originalScaleFactor = OwnerCanvasScaler.scaleFactor;
        Transform = RootObject.GetComponent<Transform>();
        if(OwnerCanvasScaler == null)
            _originalScaleFactor = Transform.localScale.x;

        ConstructUI();

        Dragger = new UIElementDragEx(RootObject, this);

        return true;
    }

    protected virtual void ConstructUI()
    {
        // Get or add RectTransform
        CustomUIObject = UIFactory.CreateUIObject($"MarkPanel_{Name}", RootObject);
        CustomUIRect = CustomUIObject.GetComponent<RectTransform>();

        // Set anchors manually using individual values instead of Vector2
        CustomUIRect.anchorMin = new Vector2(0, 0);
        CustomUIRect.anchorMax = new Vector2(1, 1);
        CustomUIRect.anchoredPosition = new Vector2(0, 0);
        CustomUIRect.sizeDelta = new Vector2(0, 0);

        // Add background image
        var bgImage = CustomUIObject.AddComponent<Image>();
        bgImage.type = Image.Type.Sliced;
        bgImage.color = Theme.PanelBackground;

        //var titleBar = UIFactory.CreateUIObject("ContentHolder", CustomUIObject);
        //UIFactory.CreateLabel(titleBar, "NameLabel", Name, fontSize: 20);

        Outline = CustomUIObject.AddComponent<Outline>();
        Outline.effectColor = Theme.ElementOutlineColor;
        Outline.enabled = false;

        // Activate the UI
        CustomUIObject.SetActive(true);
    }



    private void OnScaleChanged(float value)
    {
        ApplyScale(value);
    }

    public void ApplyScale(float value)
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

                    Plugin.Log.LogInfo($"Scale updated to {value} for {Name}");
                }
                else
                {
                    Plugin.Log.LogWarning($"Could not find scaleFactor field for {Name}");
                }
            }
            else
            {
                Transform.localScale = new Vector3(value, value, 1f);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error applying scale to {Name}: {ex.Message}");
        }
    }

    public void ResetScale()
    {
        ApplyScale(_originalScaleFactor);
    }

    public float GetOwnerScaleFactor()
    {
        return OwnerCanvas.scaleFactor;
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

    public void SelectPanel(bool select)
    {
        Outline.enabled = select;
    }
}