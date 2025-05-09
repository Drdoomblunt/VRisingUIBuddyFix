using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BepInEx;
using ProjectM;
using UnityEngine;

namespace UIBuddy.Classes.KeyBinds;

public static class KeybindRegistrator
{
    private static readonly string KeybindingsPath = Path.Join(Paths.ConfigPath, "keybindings.json");

    private static readonly Dictionary<ButtonInputAction, Keybinding> KeybindingsByFlags = new();
    private static readonly Dictionary<int, Keybinding> KeybindingsByGuid = new();
    internal static readonly Dictionary<string, Keybinding> KeybindingsById = new();
    internal static Dictionary<string, KeybindingData> KeybindingValues = new();

    public static Keybinding Register(KeybindingDescription description)
    {
        if (KeybindingsById.ContainsKey(description.Id))
            throw new ArgumentException($"Keybinding with id {description.Id} already registered");

        var keybinding = new Keybinding(description);
        KeybindingsById.Add(description.Id, keybinding);
        KeybindingsByGuid.Add(keybinding.AssetGuid, keybinding);
        KeybindingsByFlags.Add(keybinding.InputFlag, keybinding);

        if (!KeybindingValues.ContainsKey(description.Id))
        {
            KeybindingValues.Add(description.Id, new()
            {
                Id = description.Id,
                Primary = description.DefaultKeybinding,
                Secondary = KeyCode.None,
            });
        }

        return keybinding;
    }

    public static void Unregister(Keybinding keybinding)
    {
        if (!KeybindingsById.ContainsKey(keybinding.Description.Id))
            throw new ArgumentException($"There was no keybinding with id {keybinding.Description.Id} registered");

        KeybindingsByFlags.Remove(keybinding.InputFlag);
        KeybindingsByGuid.Remove(keybinding.AssetGuid);
        KeybindingsById.Remove(keybinding.Description.Id);
    }

    internal static void Load()
    {
        try
        {
            if (File.Exists(KeybindingsPath))
            {
                var content = File.ReadAllText(KeybindingsPath);
                var deserialized = JsonSerializer.Deserialize<Dictionary<string, KeybindingData>>(content);
                if (deserialized != null)
                    KeybindingValues = deserialized;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Error loading keybindings, using defaults: ");
            Plugin.Log.LogError(ex);

            KeybindingValues = new();
        }
    }

    internal static void Save()
    {
        try
        {
            var serialized = JsonSerializer.Serialize(KeybindingValues);
            File.WriteAllText(KeybindingsPath, serialized);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Error saving custom keybindings: ");
            Plugin.Log.LogError(ex);
        }
    }

    public static void UnregisterAll()
    {
        KeybindingsById.Values.ToList().ForEach(Unregister);
    }

    public static void Update()
    {
        foreach (var kb in KeybindingsById.Values)
        {
            if (kb.IsPressed)
                kb.OnKeyPressed();
        }
    }

    public static void Rebind(Keybinding keybind, KeyCode code)
    {
        KeybindingValues[keybind.Description.Id].Primary = code;
        Save();
    }
}