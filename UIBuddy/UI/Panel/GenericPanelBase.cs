using System;
using ProjectM.UI;
using UIBuddy.Managers;
using UIBuddy.UI.Classes;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIBuddy.UI.Panel;

public abstract class GenericPanelBase: IGenericPanel
{
    protected bool IsDetached { get; set; }
    public bool IsRootActive => RootObject?.activeSelf ?? false;
    public GameObject RootObject { get; }
    public Vector2 ReferenceResolution { get; set; }
    public string Name { get; }
    public RectTransform RootRect { get; }
    protected Canvas OwnerCanvas { get; private set; }
    public UIElementDragEx Dragger { get; protected set; }
    protected bool ApplyingSaveData { get; set; } = true;
    public bool IsPinned => false;

    /// <summary>
    /// Custom parent for panel in case we can't use direct parent for drag purpose
    /// </summary>
    protected readonly GameObject CustomPanelParentObject;

    protected GenericPanelBase(string gameObjectName, string friendlyName, string panelParentGameObjectName)
    {
        Name = friendlyName ?? gameObjectName;
        RootObject = gameObjectName.Contains('|') ? FindInHierarchy(gameObjectName) : GameObject.Find(gameObjectName);
        if (RootObject == null)
            return;

        if (PanelManager.ButtonBarFixList.Contains(friendlyName))
        {
            //var fitter = RootObject.GetComponent<ContentSizeFitter>();
            var parent = RootObject.GetComponent<ActionBarParent>();
            //Object.Destroy(fitter);
            Object.Destroy(parent);
        }

        RootRect = RootObject.GetComponent<RectTransform>();
        if (RootRect.rect.width < 50f || RootRect.rect.height < 50f)
            RootRect.sizeDelta = new Vector2(50f, 50f);

        OwnerCanvas = RootObject.GetComponentInParent<Canvas>();
        ReferenceResolution = RootObject.GetComponent<CanvasScaler>()?.referenceResolution ??
                              RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ??
                              Vector2.zero;

        if (!string.IsNullOrEmpty(panelParentGameObjectName))
            CustomPanelParentObject = panelParentGameObjectName.Contains('|')
                ? FindInHierarchy(panelParentGameObjectName)
                : GameObject.Find(panelParentGameObjectName);
    }

    protected GenericPanelBase(GameObject parent, string panelInternalName)
    {
        Name = panelInternalName;
        RootObject = UIFactory.CreateUIObject(panelInternalName, parent);
        RootRect = RootObject.GetComponent<RectTransform>();
        OwnerCanvas = RootObject.GetComponentInParent<Canvas>();
        ReferenceResolution = RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ?? Vector2.zero;
    }

    protected void ConstructDrag(GameObject dragObject, bool bringToFront = false)
    {
        Dragger = new UIElementDragEx(dragObject, this);
        Dragger.OnFinishDrag += () => OnFinishedDrag();
        Dragger.OnBeginDrag += () =>
        {
            if (bringToFront) RootRect.SetAsLastSibling();
        };
    }

    public virtual bool Initialize()
    {
        if(RootObject == null)
            return false;

        ConstructUI();

        return true;
    }


    protected abstract void ConstructUI();

    public float GetOwnerScaleFactor()
    {
        return OwnerCanvas.scaleFactor;
    }

    #region EnsureValidPosition

    public virtual void EnsureValidPosition()
    {
        if (RootRect == null || ReferenceResolution == Vector2.zero)
            return;

        return; //TODO

        // Get current canvas scale factor to adjust position constraints
        float canvasScale = GetOwnerScaleFactor();
        if (canvasScale <= 0)
            canvasScale = 1f;

        // Get screen dimensions from reference resolution
        Vector2 screenSize = ReferenceResolution;

        // Calculate panel's corners in screen space
        Vector3[] corners = new Vector3[4];
        RootRect.GetWorldCorners(corners);

        // Convert world space corners to local canvas space
        if (OwnerCanvas != null && OwnerCanvas.transform.parent != null)
        {
            for (int i = 0; i < 4; i++)
            {
                corners[i] = OwnerCanvas.transform.parent.InverseTransformPoint(corners[i]);
            }
        }

        // Find the bounds of the panel from its corners
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            minX = Mathf.Min(minX, corner.x);
            minY = Mathf.Min(minY, corner.y);
            maxX = Mathf.Max(maxX, corner.x);
            maxY = Mathf.Max(maxY, corner.y);
        }

        // Calculate panel dimensions
        float panelWidth = maxX - minX;
        float panelHeight = maxY - minY;

        // Calculate screen boundaries
        float halfScreenWidth = screenSize.x * 0.5f;
        float halfScreenHeight = screenSize.y * 0.5f;

        // Calculate the offsets based on anchors and pivot
        Vector2 anchorOffset = new Vector2(
            RootRect.anchorMin.x + (RootRect.anchorMax.x - RootRect.anchorMin.x) * RootRect.pivot.x,
            RootRect.anchorMin.y + (RootRect.anchorMax.y - RootRect.anchorMin.y) * RootRect.pivot.y
        );

        // Calculate screen space limits considering anchors
        float minPosX = -halfScreenWidth + panelWidth * RootRect.pivot.x;
        float maxPosX = halfScreenWidth - panelWidth * (1 - RootRect.pivot.x);
        float minPosY = -halfScreenHeight + panelHeight * RootRect.pivot.y;
        float maxPosY = halfScreenHeight - panelHeight * (1 - RootRect.pivot.y);

        // Get current position
        Vector2 position = RootRect.anchoredPosition;

        // Calculate anchor-based screen position
        Vector2 anchorScreenPos = new Vector2(
            (anchorOffset.x * screenSize.x) - halfScreenWidth,
            (anchorOffset.y * screenSize.y) - halfScreenHeight
        );

        // Apply offset based on anchors
        minPosX -= anchorScreenPos.x;
        maxPosX -= anchorScreenPos.x;
        minPosY -= anchorScreenPos.y;
        maxPosY -= anchorScreenPos.y;

        // Clamp position to keep panel on screen
        position.x = Mathf.Clamp(position.x, minPosX, maxPosX);
        position.y = Mathf.Clamp(position.y, minPosY, maxPosY);

        // Apply adjusted position
        RootRect.anchoredPosition = position;

        // If the panel has extremely unusual anchors or size, this additional safety check ensures 
        // at least 10% of the panel remains visible on each edge
        float safetyMargin = 0.1f; // 10% visibility minimum

        if (panelWidth > 0 && panelHeight > 0)
        {
            Vector3[] newCorners = new Vector3[4];
            RootRect.GetWorldCorners(newCorners);

            // Convert to canvas space
            if (OwnerCanvas != null && OwnerCanvas.transform.parent != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    newCorners[i] = OwnerCanvas.transform.parent.InverseTransformPoint(newCorners[i]);
                }
            }

            // Find new bounds
            float newMinX = float.MaxValue, newMinY = float.MaxValue;
            float newMaxX = float.MinValue, newMaxY = float.MinValue;

            foreach (Vector3 corner in newCorners)
            {
                newMinX = Mathf.Min(newMinX, corner.x);
                newMinY = Mathf.Min(newMinY, corner.y);
                newMaxX = Mathf.Max(newMaxX, corner.x);
                newMaxY = Mathf.Max(newMaxY, corner.y);
            }

            // Calculate minimum visible amounts
            float minVisibleWidth = panelWidth * safetyMargin;
            float minVisibleHeight = panelHeight * safetyMargin;

            // Adjust position if needed
            Vector2 finalAdjustment = Vector2.zero;

            if (newMinX < -halfScreenWidth)
                finalAdjustment.x += (-halfScreenWidth - newMinX) + minVisibleWidth;

            if (newMaxX > halfScreenWidth)
                finalAdjustment.x -= (newMaxX - halfScreenWidth) + minVisibleWidth;

            if (newMinY < -halfScreenHeight)
                finalAdjustment.y += (-halfScreenHeight - newMinY) + minVisibleHeight;

            if (newMaxY > halfScreenHeight)
                finalAdjustment.y -= (newMaxY - halfScreenHeight) + minVisibleHeight;

            // Apply final safety adjustment if needed
            if (finalAdjustment != Vector2.zero)
            {
                RootRect.anchoredPosition += finalAdjustment;
            }
        }
    }

    #endregion

    public virtual void ShowPanelOutline(bool select)
    {
    }

    public virtual void SetActive(bool value)
    {
        RootObject?.SetActive(value);
    }

    public virtual void SetRootActive(bool value)
    {
        RootObject.SetActive(value);
    }

    public virtual void Dispose()
    {
        Object.Destroy(RootObject);
        Dragger?.Dispose();
    }

    protected virtual void OnFinishedDrag()
    {
        Save();
    }

    public virtual void Update()
    {

    }

    #region Save/Load settings

    private string PanelConfigKey => $"{Name}".Replace("'", "").Replace("\"", "").Replace(" ","_");

    protected virtual void Save()
    {
        if (ApplyingSaveData) return;

        SetSaveDataToConfigValue(RootRect);
    }

    protected virtual bool LoadConfigValues()
    {
        ApplyingSaveData = true;
        // apply panel save data or revert to default
        try
        {
            return ApplyLoadedData(RootRect);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Exception loading panel save data: {ex}");
            EnsureValidPosition();
            return false;
        }
        finally
        {
            ApplyingSaveData = false;
        }

    }

    protected void SetSaveDataToConfigValue(RectTransform rectTransform)
    {
        Plugin.Instance.Config.Bind("Panels", PanelConfigKey, "", "Serialized panel data").Value = ToSaveData(rectTransform);
    }

    private string ToSaveData(RectTransform rectTransform)
    {
        try
        {
            return string.Join("|", new string[]
            {
                IsRootActive.ToString(),
                rectTransform.RectSizeToString(),
                rectTransform.RectPositionToString(),
                rectTransform.RectRotationToString(),
                IsPinned.ToString(),
                rectTransform.RectScaleToString(),
            });
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Exception generating Panel save data: {ex}");
            return "";
        }
    }

    protected bool ApplyLoadedData(RectTransform rectTransform)
    {
        var data = Plugin.Instance.Config.Bind("Panels", PanelConfigKey, "", "Serialized panel data").Value;
        return ApplyLoadedData(data, rectTransform);
    }

    private bool ApplyLoadedData(string data, RectTransform rectTransform)
    {
        if (string.IsNullOrEmpty(data))
            return false;
        string[] split = data.Split('|');

        try
        {
            if(bool.TryParse(split[0], out var isActive))
                RootObject.SetActive(isActive);
            //rectTransform.SetAnchorsFromString(split[1]);
            rectTransform.SetPositionFromString(split[2]);
            rectTransform.SetRotationFromString(split[3]);
            // IsPinned 4
            rectTransform.SetScaleFromString(split[5]);

            if (IsDetached)
            {
                RootRect.sizeDelta = new Vector2(50f, 50f);
            }

            EnsureValidPosition();
        }
        catch
        {
            Plugin.Log.LogWarning("Invalid or corrupt panel save data! Restoring to default.");
            //SetDefaultSizeAndPosition();
            EnsureValidPosition();
            SetSaveDataToConfigValue(rectTransform);
            return false;
        }

        return true;
    }

    #endregion

    protected static GameObject FindInHierarchy(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        string[] segments = path.Split('|');
        if (segments.Length == 0)
            return null;

        // Start with the root object
        GameObject current = GameObject.Find(segments[0]);
        if (current == null)
            return null;

        // Navigate through the hierarchy path
        for (int i = 1; i < segments.Length; i++)
        {
            Transform child = current.transform.Find(segments[i]);
            if (child == null)
                return null;

            current = child.gameObject;
        }

        return current;
    }
}