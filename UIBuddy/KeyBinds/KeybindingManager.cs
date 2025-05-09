using System;
using UIBuddy.Managers;
using UnityEngine;

namespace UIBuddy.KeyBinds;

internal static class KeybindingManager
{
    public static void LoadBindings()
    {
        try
        {
            KeybindRegistrator.Load();

            var showMainKb = KeybindRegistrator.Register(new KeybindingDescription(
            
                id: $"{MyPluginInfo.PLUGIN_GUID}_SHOW",
                name: "Show/Hide UIBuddy",
                desc: "Show/Hide UIBuddy",
                category: MyPluginInfo.PLUGIN_NAME,
                binding: KeyCode.PageDown
            ));
            if (showMainKb != null)
                showMainKb.KeyPressed += ShowMainKb_KeyPressed;

            Plugin.Log.LogInfo("Key bindings loaded successfully!");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Key bindings load error: {ex}");
        }
    }

    public static void UnloadBindings()
    {
        KeybindRegistrator.UnregisterAll();
    }

    private static void ShowMainKb_KeyPressed()
    {
        if (!Plugin.IsInitialized) return;
        PanelManager.MainPanel.ToggleMainPanel();
    }
}