using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Refs;

/// <summary>
/// A simple helper class to handle a button's OnClick more effectively, along with some helpers.
/// </summary>
public class ButtonRef
{
    /// <summary>
    /// Invoked when the Button is clicked.
    /// </summary>
    public Action OnClick;

    /// <summary>
    /// The actual Button component this object is a reference to.
    /// </summary>
    public Button Component { get; }

    /// <summary>
    /// The Text component on the button.
    /// </summary>
    public TextMeshProUGUI ButtonText { get; }

    /// <summary>
    /// The GameObject this Button is attached to.
    /// </summary>
    public GameObject GameObject => Component.gameObject;

    /// <summary>
    /// The RectTransform for this Button.
    /// </summary>
    public RectTransform Transform { get; }

    /// <summary>
    /// Helper for <c>Button.enabled</c>.
    /// </summary>
    public bool Enabled
    {
        get => Component.enabled;
        set => Component.enabled = value;
    }

    public ButtonRef(Button button)
    {
        Component = button;
        ButtonText = button.GetComponentInChildren<TextMeshProUGUI>();
        Transform = button.GetComponent<RectTransform>();

        button.onClick.AddListener(new Action(() => { OnClick?.Invoke(); }));
    }

    public void DisableWithTimer(int interval)
    {
        Component.interactable = false;

        var timer = new System.Timers.Timer(interval);
        timer.Elapsed += (sender, e) =>
        {
            Component.interactable = true;
            timer.Stop();
            timer.Dispose();
        };
        timer.AutoReset = false;
        timer.Enabled = true;
        timer.Start();
    }
}