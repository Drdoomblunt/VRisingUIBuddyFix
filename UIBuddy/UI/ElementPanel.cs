using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
using UIBuddy.UI.Panel;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    protected RectOutline Outline { get; set; }

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

        CoroutineUtility.StartCoroutine(SafeCreateContent());
        // Activate the UI
        CustomUIObject.SetActive(true);
    }

    private IEnumerator SafeCreateContent()
    {
        // Wait for a frame to ensure GameObject is fully initialized
        yield return null;

        try
        {
            if (CustomUIObject != null)
            {
                Outline = CustomUIObject.AddComponent<RectOutline>();
                Outline.OutlineColor = Theme.ElementOutlineColor;
                Outline.LineWidth = 2f; // Adjust as needed
                Outline.SetActive(false);

                // Create a layout container for the label to ensure proper horizontal display
                var labelContainer = UIFactory.CreateHorizontalGroup(
                    CustomUIObject,
                    "LabelContainer",
                    forceExpandWidth: true,
                    forceExpandHeight: false,
                    childControlWidth: true,
                    childControlHeight: true,
                    spacing: 5,
                    padding: new Vector4(5, 5, 5, 5));

                // Create the label
                var label = UIFactory.CreateLabel(labelContainer, "NameLabel", Name,
                    alignment: TextAlignmentOptions.Left,
                    fontSize: 16);

                // Ensure the label gets appropriate layout settings
                UIFactory.SetLayoutElement(label.GameObject,
                    minWidth: 50,
                    flexibleWidth: 1,
                    minHeight: 25,
                    preferredHeight: 25);

                var toggleRef = UIFactory.CreateToggle(CustomUIObject, "EnableCheck");
                toggleRef.OnValueChanged += value => { RootObject.SetActive(value); };
                toggleRef.Toggle.isOn = true; // Default value
                toggleRef.Text.text = "Enable this panel";
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error in SafeCreateContent for {Name}: {ex.Message}");
        }
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
        if (Outline == null) return;
        Outline.SetActive(select);
    }

    public void SetActive(bool value)
    {
        CustomUIObject.SetActive(value);
        if (!value)
            PanelManager.DeselectPanels();
    }

    public void Dispose()
    {
        Object.Destroy(CustomUIObject);
    }
}