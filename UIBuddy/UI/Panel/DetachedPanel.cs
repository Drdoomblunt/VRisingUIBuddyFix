using System;
using System.Collections;
using TMPro;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Panel;

public class DetachedPanel: ElementPanel
{
    private GameObject TargetObject { get; }
    private RectTransform TargetRect { get; }
    private const float TOLERANCE = 0.001f;

    private Vector2 _oldPosition = Vector2.zero;
    private Vector3 _oldScale = Vector3.one;
    private float _oldRotation = 0f;
    private readonly string _shortName;

    protected bool ParamHasInitialPosition;
    protected bool ParamPositionValidation = true;


    public DetachedPanel(string gameObjectName, string friendlyName, string shortName, GameObject parent)
        : base(parent, friendlyName)
    {
        _shortName = shortName;
        IsDetached = true;

        TargetObject = gameObjectName.Contains('|') ? FindInHierarchy(gameObjectName) : GameObject.Find(gameObjectName);
        if (TargetObject == null)
            return;
        TargetRect = TargetObject.GetComponent<RectTransform>();
        
        RootRect.anchoredPosition = TargetRect.anchoredPosition;
        RootRect.anchorMin = TargetRect.anchorMin;
        RootRect.anchorMax = TargetRect.anchorMax;
        RootRect.pivot = TargetRect.pivot;

        RootRect.sizeDelta = new Vector2(50f, 50f);

        EnsureValidPositionEx();
    }

    public override void Update()
    {
        if(!IsRootActive) return;

        // Force size to remain 50x50
        if (Math.Abs(RootRect.sizeDelta.x - 50f) > TOLERANCE ||
            Math.Abs(RootRect.sizeDelta.y - 50f) > TOLERANCE)
        {
            RootRect.sizeDelta = new Vector2(50f, 50f);
        }

        var shouldSave = false;
        if (Math.Abs(Transform.localEulerAngles.z - _oldRotation) > TOLERANCE)
        {
            TargetRect.localEulerAngles = RootRect.localEulerAngles;
            shouldSave = true;
        }

        if (Math.Abs(Transform.localScale.x - _oldScale.x) > TOLERANCE)
        {
            TargetRect.localScale = RootRect.localScale;
            shouldSave = true;
        }

        if (Math.Abs(Transform.localPosition.x - _oldPosition.x) > TOLERANCE ||
            Math.Abs(Transform.localPosition.y - _oldPosition.y) > TOLERANCE)
        {
            TargetRect.anchoredPosition = RootRect.anchoredPosition;
            if(ParamPositionValidation)
                EnsureValidPositionEx();
            shouldSave = true; 
        }

        _oldPosition = TargetRect.anchoredPosition;
        _oldScale = TargetRect.localScale;
        _oldRotation = TargetRect.localEulerAngles.z;

        if(shouldSave)
            Save();
    }

    private void EnsureValidPositionEx()
    {
        if (RootRect == null || ReferenceResolution == Vector2.zero)
            return;

        // Get current canvas scale factor to adjust position constraints
        float canvasScale = GetOwnerScaleFactor();
        if (canvasScale <= 0)
            canvasScale = 1f;

        // Get screen dimensions
        Vector2 screenSize = ReferenceResolution;

        // Calculate minimum visible percentage
        float minVisiblePercentage = 0.1f; // 10% of the panel must be visible

        // Get panel dimensions
        Vector2 panelSize = RootRect.rect.size;

        // Calculate minimum visible amounts in pixels
        float minVisibleWidth = panelSize.x * minVisiblePercentage;
        float minVisibleHeight = panelSize.y * minVisiblePercentage;

        // Calculate screen boundaries
        float halfScreenWidth = screenSize.x * 0.5f;
        float halfScreenHeight = screenSize.y * 0.5f;

        // Get current anchored position
        Vector2 currentPosition = RootRect.anchoredPosition;

        // Calculate anchor-based offset
        Vector2 anchorOffset = new Vector2(
            RootRect.anchorMin.x + (RootRect.anchorMax.x - RootRect.anchorMin.x) * RootRect.pivot.x,
            RootRect.anchorMin.y + (RootRect.anchorMax.y - RootRect.anchorMin.y) * RootRect.pivot.y
        ) * screenSize - new Vector2(halfScreenWidth, halfScreenHeight);

        // Calculate adjusted boundaries based on pivot and anchors
        float minX = -halfScreenWidth + panelSize.x * RootRect.pivot.x - anchorOffset.x;
        float maxX = halfScreenWidth - panelSize.x * (1 - RootRect.pivot.x) - anchorOffset.x;
        float minY = -halfScreenHeight + panelSize.y * RootRect.pivot.y - anchorOffset.y;
        float maxY = halfScreenHeight - panelSize.y * (1 - RootRect.pivot.y) - anchorOffset.y;

        // Adjust for minimum visibility
        minX = Mathf.Min(minX, halfScreenWidth - minVisibleWidth - anchorOffset.x);
        maxX = Mathf.Max(maxX, -halfScreenWidth + minVisibleWidth - anchorOffset.x);
        minY = Mathf.Min(minY, halfScreenHeight - minVisibleHeight - anchorOffset.y);
        maxY = Mathf.Max(maxY, -halfScreenHeight + minVisibleHeight - anchorOffset.y);

        // Clamp position to keep panel on screen with minimum visibility
        Vector2 adjustedPosition = new Vector2(
            Mathf.Clamp(currentPosition.x, minX, maxX),
            Mathf.Clamp(currentPosition.y, minY, maxY)
        );

        // Apply position if it changed
        if (adjustedPosition != currentPosition)
        {
            RootRect.anchoredPosition = adjustedPosition;
        }

        // Double-check visibility with world space corners
        Vector3[] corners = new Vector3[4];
        RootRect.GetWorldCorners(corners);

        // Calculate screen edges in world space
        Vector3[] screenCorners = new Vector3[4];
        if (OwnerCanvas != null && OwnerCanvas.transform.parent != null)
        {
            // Convert from world space to local canvas space
            for (int i = 0; i < 4; i++)
            {
                corners[i] = OwnerCanvas.transform.parent.InverseTransformPoint(corners[i]);
            }

            // Get screen edges in same coordinate space
            Canvas rootCanvas = OwnerCanvas.rootCanvas;
            RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
            canvasRect.GetWorldCorners(screenCorners);
            for (int i = 0; i < 4; i++)
            {
                screenCorners[i] = OwnerCanvas.transform.parent.InverseTransformPoint(screenCorners[i]);
            }
        }

        // Calculate screen bounds
        float screenLeft = screenCorners[0].x;
        float screenRight = screenCorners[2].x;
        float screenBottom = screenCorners[0].y;
        float screenTop = screenCorners[1].y;

        // Find the bounds of the panel
        float minPanelX = float.MaxValue, minPanelY = float.MaxValue;
        float maxPanelX = float.MinValue, maxPanelY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            minPanelX = Mathf.Min(minPanelX, corner.x);
            minPanelY = Mathf.Min(minPanelY, corner.y);
            maxPanelX = Mathf.Max(maxPanelX, corner.x);
            maxPanelY = Mathf.Max(maxPanelY, corner.y);
        }

        // Emergency visibility correction
        Vector2 finalAdjustment = Vector2.zero;

        if (maxPanelX < screenLeft + minVisibleWidth)
            finalAdjustment.x += (screenLeft + minVisibleWidth) - maxPanelX;

        if (minPanelX > screenRight - minVisibleWidth)
            finalAdjustment.x -= minPanelX - (screenRight - minVisibleWidth);

        if (maxPanelY < screenBottom + minVisibleHeight)
            finalAdjustment.y += (screenBottom + minVisibleHeight) - maxPanelY;

        if (minPanelY > screenTop - minVisibleHeight)
            finalAdjustment.y -= minPanelY - (screenTop - minVisibleHeight);

        // Apply final adjustment if needed
        if (finalAdjustment != Vector2.zero)
        {
            RootRect.anchoredPosition += finalAdjustment;
        }
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
            OriginalScaleFactor = OwnerCanvasScaler.scaleFactor;
        Transform = RootObject.GetComponent<Transform>();
        if(OwnerCanvasScaler == null)
            OriginalScaleFactor = Transform.localScale.x;


        if(!base.Initialize())
            return false;

        return true;
    }

    protected override void ConstructUI()
    {
        RootObject.SetActive(false);
        // Get or add RectTransform
        CustomUIObject = UIFactory.CreateUIObject($"DetachedPanel_{Name}", RootObject);
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
                    Outline.UpdateRect(RootRect);
                }

                if (!string.IsNullOrEmpty(_shortName))
                {
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
                        var label = UIFactory.CreateLabel(headerContainer, $"NameLabel_{_shortName}", _shortName,
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
                    }
                }

                // Activate the UI
                if(ConfigManager.IsModVisible)
                    RootObject.SetActive(true);
                Outline?.SetActive(false);

                if (LoadConfigValues())
                    TargetRect.anchoredPosition = RootRect.anchoredPosition;

                RootRect.sizeDelta = new Vector2(50f, 50f);
                CustomUIRect.sizeDelta = new Vector2(50f, 50f);
            }
        }
        catch (Exception ex)
        {
            if (Plugin.Log != null)
                Plugin.Log.LogError($"Error in SafeCreateContent for {Name}: {ex.Message}");
        }
    }

    public void SetParameters(bool? positionValidation = null, Vector2? initialPosition = null)
    {
        if (positionValidation == false)
            ParamPositionValidation = false;

        if (initialPosition != null)
        {
            ParamHasInitialPosition = true;
            RootRect.anchoredPosition = (Vector2)initialPosition;
        }
    }

    public override void SetActive(bool value)
    {
        CustomUIObject.SetActive(value);
        if (!value && PanelManager.MainPanel.SelectedElementPanel == this)
            PanelManager.MainPanel.SelectedElementPanel = null;
    }

    public override void SetRootActive(bool value)
    {
        RootObject.SetActive(value);
        TargetObject.SetActive(value);
        Save();
    }
}