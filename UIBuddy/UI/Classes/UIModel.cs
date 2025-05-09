using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIBuddy.UI.Classes;

/// <summary>
/// An abstract UI object which does not exist as an actual UI Component, but which may be a reference to one.
/// </summary>
public abstract class UIModel
{
    protected abstract GameObject UIRoot { get; }

    public bool Enabled
    {
        get => UIRoot && UIRoot.activeInHierarchy;
        set
        {
            if (!UIRoot || Enabled == value)
                return;
            UIRoot.SetActive(value);
            OnToggleEnabled?.Invoke(value);
        }
    }

    public event Action<bool> OnToggleEnabled;

    public virtual void Toggle() => SetActive(!Enabled);

    protected virtual void SetActive(bool active)
    {
        if (UIRoot)
            UIRoot.SetActive(active);
    }

    protected virtual void Destroy()
    {
        if (UIRoot)
            Object.Destroy(UIRoot);
    }
}