using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Panel;

public abstract class GenericPanelBase: IGenericPanel
{
    public bool IsActive => RootObject?.activeSelf ?? false;
    public GameObject RootObject { get; }
    public Vector2 ReferenceResolution { get; set; }
    public string Name { get; }
    public RectTransform RootRect { get; }
    public Canvas OwnerCanvas { get; private set; }

    protected GenericPanelBase(string gameObjectName)
    {
        Name = gameObjectName;
        RootObject = GameObject.Find(gameObjectName);
        if(RootObject == null)
            return;
        RootRect = RootObject.GetComponent<RectTransform>();
        OwnerCanvas = RootObject.GetComponentInParent<Canvas>();
        ReferenceResolution = RootObject.GetComponent<CanvasScaler>()?.referenceResolution ??
                              RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ??
                              Vector2.zero;
    }

    protected GenericPanelBase(GameObject parent, string panelInternalName)
    {
        RootObject = UIFactory.CreateUIObject(panelInternalName, parent);
        RootRect = RootObject.GetComponent<RectTransform>();
        OwnerCanvas = RootObject.GetComponentInParent<Canvas>();
        ReferenceResolution = RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ?? Vector2.zero;
    }

    protected abstract void ConstructUI();

    public float GetOwnerScaleFactor()
    {
        return OwnerCanvas.scaleFactor;
    }

    public virtual void EnsureValidPosition()
    {
        if (RootRect == null || ReferenceResolution == Vector2.zero)
            return;

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

    public virtual void SelectPanelAsCurrentlyActive(bool select)
    {
    }

    public virtual void SetActive(bool value)
    {
        RootObject.SetActive(value);
    }

    public virtual void SetActiveUnconditionally(bool value)
    {
        RootObject.SetActive(value);
    }

    public virtual void Dispose()
    {
        Object.Destroy(RootObject);
    }
}