using System;
using System.Globalization;
using UnityEngine;

namespace UIBuddy.UI.Classes;

public static class RectExtensions
{
    #region Handle Size

    internal static string RectSizeToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return string.Format(CultureInfo.InvariantCulture, "{0},{1}",rect.rect.width,
            rect.rect.height);
    }

    internal static void SetSizeFromString(this RectTransform panel, string stringAnchors)
    {
        if (string.IsNullOrEmpty(stringAnchors))
            throw new ArgumentNullException(nameof(stringAnchors));

        var split = stringAnchors.Split(',');

        if (split.Length != 2)
            throw new Exception($"stringAnchors split is unexpected length: {split.Length}");

        var width = float.Parse(split[0], CultureInfo.InvariantCulture);
        var height = float.Parse(split[1], CultureInfo.InvariantCulture);

        panel.sizeDelta = new Vector2(width, height);
    }

    #endregion

    #region Handle Position

    internal static string RectPositionToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return string.Format(CultureInfo.InvariantCulture, "{0},{1}", 
            rect.anchoredPosition.x, rect.anchoredPosition.y);
    }

    internal static void SetPositionFromString(this RectTransform rect, string stringPosition)
    {
        if (string.IsNullOrEmpty(stringPosition))
            throw new ArgumentNullException(nameof(stringPosition));

        var split = stringPosition.Split(',');

        if (split.Length != 2)
            throw new Exception($"stringPosition split is unexpected length: {split.Length}");

        var x = float.Parse(split[0], CultureInfo.InvariantCulture);
        var y = float.Parse(split[1], CultureInfo.InvariantCulture);
        rect.anchoredPosition = new Vector2(x, y);
    }

    #endregion

    #region Handle Rotation

    internal static string RectRotationToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", new object[]
        {
            rect.rotation.x, rect.rotation.y, rect.rotation.z, rect.rotation.w
        });
    }

    internal static void SetRotationFromString(this RectTransform rect, string stringRotation)
    {
        if (string.IsNullOrEmpty(stringRotation))
            throw new ArgumentNullException(nameof(stringRotation));

        var split = stringRotation.Split(',');

        if (split.Length != 4)
            throw new Exception($"stringPosition split is unexpected length: {split.Length}");

        var vector = new Quaternion
        {
            x = float.Parse(split[0], CultureInfo.InvariantCulture),
            y = float.Parse(split[1], CultureInfo.InvariantCulture),
            z = float.Parse(split[2], CultureInfo.InvariantCulture),
            w = float.Parse(split[3], CultureInfo.InvariantCulture)
        };
        rect.rotation = vector;
    }
    #endregion

    #region Handle Scale

    internal static string RectScaleToString(this RectTransform rect)
    {
        if (!rect)
            throw new ArgumentNullException(nameof(rect));

        return rect.localScale.x.ToString(CultureInfo.InvariantCulture);
    }

    internal static void SetScaleFromString(this RectTransform rect, string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(value);

        var floatValue = float.Parse(value, CultureInfo.InvariantCulture);
        rect.localScale = new Vector3(floatValue, floatValue, 1f);
    }

    #endregion

    internal static void SetPivot(this RectTransform rect, Vector2 pivot)
    {
        var size = rect.rect.size;
        var deltaPivot = rect.pivot - pivot;
        var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rect.pivot = pivot;
        rect.localPosition -= deltaPosition;
    }
}