using System;
using System.Collections;
using HarmonyLib;
using TMPro;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
using UIBuddy.UI.Classes;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIBuddy.UI.Panel;

public class ElementPanel: GenericPanelBase
{
    public bool CanDrag => true;

    public CanvasScaler OwnerCanvasScaler { get; private set; }
    public Transform Transform { get; set; }

    private RectTransform CustomUIRect { get; set; }
    private GameObject CustomUIObject { get; set; }
    private RectOutline Outline { get; set; }

    // Track the original scale to allow proper reset
    private float _originalScaleFactor;
    private ToggleRef _toggleRef;

    public ElementPanel(string gameObjectName, string friendlyName)
        : base(gameObjectName, friendlyName)
    {
    }

    public override bool Initialize()
    {
        if (RootObject == null)
        {
            Plugin.Log.LogWarning($"Failed to initialize UIElement: {Name}");
            return false;
        }
        ConstructDrag(RootObject);

        OwnerCanvasScaler = RootObject.GetComponent<CanvasScaler>();

        if (OwnerCanvasScaler != null)
            _originalScaleFactor = OwnerCanvasScaler.scaleFactor;
        Transform = RootObject.GetComponent<Transform>();
        if(OwnerCanvasScaler == null)
            _originalScaleFactor = Transform.localScale.x;


        if(!base.Initialize())
            return false;

        return true;
    }

    protected override void ConstructUI()
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

        CoroutineUtility.StartCoroutine(SafeCreateContent());

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
                if (Outline != null)
                {
                    Outline.OutlineColor = Theme.ElementOutlineColor;
                    Outline.LineWidth = 2f; // Adjust as needed
                    Outline.SetActive(false);
                }

                // Create a header container at the top of the panel
                var headerContainer = UIFactory.CreateUIObject($"HeaderContainer_{Name}", CustomUIObject);
                if (headerContainer != null)
                {
                    var headerRect = headerContainer.GetComponent<RectTransform>();
                    if (headerRect != null)
                    {
                        headerRect.anchorMin = new Vector2(0, 1);
                        headerRect.anchorMax = new Vector2(1, 1);
                        headerRect.pivot = new Vector2(0.5f, 1);
                        headerRect.sizeDelta = new Vector2(0, 30); // Fixed height
                        headerRect.anchoredPosition = Vector2.zero;
                    }

                    // Create the label with maximum width
                    var label = UIFactory.CreateLabel(headerContainer, $"NameLabel_{Name}", Name,
                        alignment: TextAlignmentOptions.Left,
                        fontSize: 12);

                    if (label != null && label.GameObject != null)
                    {
                        // Make the label take most of the width
                        var labelRect = label.GameObject.GetComponent<RectTransform>();
                        if (labelRect != null)
                        {
                            labelRect.anchorMin = new Vector2(0, 0);
                            labelRect.anchorMax = new Vector2(0.9f, 1);
                            labelRect.pivot = new Vector2(0, 0.5f);
                            labelRect.anchoredPosition = new Vector2(10, 0);
                            labelRect.sizeDelta = Vector2.zero;
                        }
                    }

                    // Create toggle in top-right corner using UIFactory
                    var toggleContainer = UIFactory.CreateUIObject($"ToggleContainer_{Name}", headerContainer);
                    if (toggleContainer != null)
                    {
                        var toggleContainerRect = toggleContainer.GetComponent<RectTransform>();
                        if (toggleContainerRect != null)
                        {
                            toggleContainerRect.anchorMin = new Vector2(0.9f, 0);
                            toggleContainerRect.anchorMax = new Vector2(1, 1);
                            toggleContainerRect.pivot = new Vector2(1, 0.5f);
                            toggleContainerRect.anchoredPosition = new Vector2(-10, 0);
                            toggleContainerRect.sizeDelta = Vector2.zero;
                        }

                        // Use UIFactory to create the toggle
                        _toggleRef = UIFactory.CreateToggle(toggleContainer, $"EnableToggle_{Name}");
                        if (_toggleRef != null)
                        {
                            // Hide the text
                            if (_toggleRef.Text != null)
                            {
                                _toggleRef.Text.text = "";
                            }

                            // Set the value change handler
                            _toggleRef.OnValueChanged += value =>
                            {
                                if (RootObject != null)
                                {
                                    RootObject.SetActive(value);
                                    //on hide we should update all related controls
                                    if (!value && PanelManager.MainPanel.SelectedElementPanel == this)
                                    {
                                        PanelManager.MainPanel.SelectedElementPanel = null;
                                        SelectPanelAsCurrentlyActive(false);
                                    }

                                    PanelManager.ElementListPanel.UpdateElement(this);
                                }
                            };

                            if (_toggleRef.Toggle != null)
                            {
                                _toggleRef.Toggle.isOn = true; // Default value
                            }
                        }
                    }
                }
                // Activate the UI
                CustomUIObject.SetActive(true);

                LoadConfigValues();
            }
        }
        catch (Exception ex)
        {
            if (Plugin.Log != null)
                Plugin.Log.LogError($"Error in SafeCreateContent for {Name}: {ex.Message}");
        }
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
            Save();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error applying scale to {Name}: {ex.Message}");
        }
    }

    public void ApplyRotation(float value)
    {
        try
        {
            var eulerAngles = Transform.localEulerAngles;
            Transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, value);
            Save();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Error applying rotation to {Name}: {ex.Message}");
        }
    }

    public void ResetScale()
    {
        ApplyScale(_originalScaleFactor);
    }

    public override void SelectPanelAsCurrentlyActive(bool select)
    {
        if (Outline == null || !RootObject.activeSelf) return;
        Outline.SetActive(select);
    }

    public override void SetActive(bool value)
    {
        if(!RootObject.activeSelf) return;
        CustomUIObject.SetActive(value);
        if(!value && PanelManager.MainPanel.SelectedElementPanel == this)
            PanelManager.MainPanel.SelectedElementPanel = null;
    }

    public override void SetRootActive(bool value)
    {
        //base.SetRootActive(value);
        if(_toggleRef == null) //while loading
            RootObject.SetActive(value);
        else 
            _toggleRef.Toggle.isOn = value;
        Save();
    }

    public override void Dispose()
    {
        Object.Destroy(CustomUIObject);
    }
}