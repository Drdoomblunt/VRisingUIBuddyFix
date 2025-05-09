using Stunlock.Localization;
using UIBuddy.Managers;
using UnityEngine;

namespace UIBuddy.KeyBinds;

/// <summary>
/// Represents a keybinding option.
/// </summary>
public struct KeybindingDescription
{
    /// <summary>
    /// The ID of the keybinding. This should be unique across all keybindings.
    /// </summary>
    public string Id;

    /// <summary>
    /// The title of the category in which this keybinding should appear.
    /// </summary>
    public string Category;

    /// <summary>
    /// The name of the keybinding setting, as shown in the settings menu.
    /// </summary>
    public string Name;

    public string Description;

    public LocalizationKey DescriptionKey { get; set; }

    public LocalizationKey NameKey { get; set; }

    /// <summary>
    /// The default keycode for this keybinding. Use KeyCode.NONE if you do not
    /// want the keybinding to be bound by default. The secondary keybinding will
    /// always be set to none when the user has not explicitly set it.
    /// </summary>
    public KeyCode DefaultKeybinding;

    public KeybindingDescription(string id, string category, string name, string desc, KeyCode binding)
    {
        Id = id;
        Category = category;
        Name = name;
        Description = desc;
        DefaultKeybinding = binding;

        NameKey = LocalizationManager.AddKey(Name);
        DescriptionKey = LocalizationManager.AddKey(Description);
    }
}