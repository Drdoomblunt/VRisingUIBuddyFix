using System;
using System.Globalization;
using UnityEngine;

namespace UIBuddy.UI.Classes;

public static class RectExtensions
{
    // Window Anchors helpers
    internal static string RectAnchorsToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException("rect");

        return string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[]
        {
            rect.rect.width,
            rect.rect.height
        });
    }

    internal static void SetAnchorsFromString(this RectTransform panel, string stringAnchors)
    {
        if (string.IsNullOrEmpty(stringAnchors))
            throw new ArgumentNullException("stringAnchors");

        string[] split = stringAnchors.Split(',');

        if (split.Length != 2)
            throw new Exception($"stringAnchors split is unexpected length: {split.Length}");

        var width = float.Parse(split[0], CultureInfo.InvariantCulture);
        var height = float.Parse(split[1], CultureInfo.InvariantCulture);

        panel.sizeDelta = new Vector2(width, height);
    }

    internal static string RectPositionToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException("rect");

        return string.Format(CultureInfo.InvariantCulture, "{0},{1}", new object[]
        {
            rect.anchoredPosition.x, rect.anchoredPosition.y
        });
    }

    internal static string RectRotationToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", new object[]
        {
            rect.rotation.x, rect.rotation.y, rect.rotation.z, rect.rotation.w
        });
    }

    internal static string RectScaleToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return rect.localScale.x.ToString(CultureInfo.InvariantCulture);
    }

    internal static void SetPositionFromString(this RectTransform rect, string stringPosition)
    {
        if (string.IsNullOrEmpty(stringPosition))
            throw new ArgumentNullException(nameof(stringPosition));

        string[] split = stringPosition.Split(',');

        if (split.Length != 2)
            throw new Exception($"stringPosition split is unexpected length: {split.Length}");

        Vector2 vector = rect.anchoredPosition;
        vector.x = float.Parse(split[0], CultureInfo.InvariantCulture);
        vector.y = float.Parse(split[1], CultureInfo.InvariantCulture);
        rect.anchoredPosition = vector;
    }

    internal static void SetRotationFromString(this RectTransform rect, string stringRotation)
    {
        if (string.IsNullOrEmpty(stringRotation))
            throw new ArgumentNullException(nameof(stringRotation));

        string[] split = stringRotation.Split(',');

        if (split.Length != 4)
            throw new Exception($"stringPosition split is unexpected length: {split.Length}");

        var vector = rect.rotation;
        vector.x = float.Parse(split[0], CultureInfo.InvariantCulture);
        vector.y = float.Parse(split[1], CultureInfo.InvariantCulture);
        vector.z = float.Parse(split[2], CultureInfo.InvariantCulture);
        vector.w = float.Parse(split[3], CultureInfo.InvariantCulture);
        rect.rotation = vector;
    }

    internal static void SetScaleFromString(this RectTransform rect, string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(value);

        var floatValue = float.Parse(value, CultureInfo.InvariantCulture);
        rect.localScale = new Vector3(floatValue, floatValue, 1f);
    }

    internal static void SetPivot(this RectTransform rect, Vector2 pivot)
    {
        Vector2 size = rect.rect.size;
        Vector2 deltaPivot = rect.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rect.pivot = pivot;
        rect.localPosition -= deltaPosition;
    }
}