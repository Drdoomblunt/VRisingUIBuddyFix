using UnityEngine;

namespace UIBuddy.Classes.KeyBinds;

internal class KeybindingData
{
    public string Id { get; set; }
    public KeyCode Primary { get; set; }
    public KeyCode Secondary { get; set; }
}