using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.Classes.Behaviors;

public class RectOutline : MonoBehaviour
{
    public Color OutlineColor = Color.yellow;
    public float LineWidth = 1f;
    private RectTransform _rect;
    private readonly GameObject[] _lines = new GameObject[4]; // Top, Right, Bottom, Left

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        CreateOutlineLines();
    }

    public void UpdateRect(RectTransform rect)
    {
        _rect = rect;
    }

    private void CreateOutlineLines()
    {
        // Create line objects if they don't exist
        for (int i = 0; i < 4; i++)
        {
            if (_lines[i] == null)
            {
                _lines[i] = new GameObject($"Outline_Line_{i}");
                _lines[i].transform.SetParent(transform, false);
                _lines[i].AddComponent<RectTransform>();

                var image = _lines[i].AddComponent<Image>();
                image.color = OutlineColor;
            }
        }

        UpdateLines();
    }

    private void UpdateLines()
    {
        if (_rect == null) return;

        // Get rect dimensions
        Rect rect = _rect.rect;
        float width = rect.width;
        float height = rect.height;

        // Top line
        ConfigureLine(_lines[0].GetComponent<RectTransform>(),
            new Vector2(0, height - LineWidth / 2),
            new Vector2(width, LineWidth));

        // Right line
        ConfigureLine(_lines[1].GetComponent<RectTransform>(),
            new Vector2(width - LineWidth / 2, 0),
            new Vector2(LineWidth, height));

        // Bottom line
        ConfigureLine(_lines[2].GetComponent<RectTransform>(),
            new Vector2(0, 0),
            new Vector2(width, LineWidth));

        // Left line
        ConfigureLine(_lines[3].GetComponent<RectTransform>(),
            new Vector2(0, 0),
            new Vector2(LineWidth, height));
    }

    private void ConfigureLine(RectTransform lineRect, Vector2 position, Vector2 size)
    {
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.zero;
        lineRect.pivot = Vector2.zero;
        lineRect.anchoredPosition = position;
        lineRect.sizeDelta = size;

        Image image = lineRect.GetComponent<Image>();
        if (image != null)
        {
            image.color = OutlineColor;
        }
    }

    public void SetColor(Color color)
    {
        OutlineColor = color;
        foreach (var line in _lines)
        {
            if (line != null)
            {
                var image = line.GetComponent<Image>();
                if (image != null)
                {
                    image.color = color;
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        foreach (var line in _lines)
        {
            if (line != null)
            {
                line.SetActive(active);
            }
        }
    }
}