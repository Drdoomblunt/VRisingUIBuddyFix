using FMOD;
using HarmonyLib;
using ProjectM.UI;
using System;
using TMPro;
using UIBuddy.UI;

namespace UIBuddy.Patches;

[HarmonyPatch]
public static class InitializationPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterHUDEntry), nameof(CharacterHUDEntry.Awake))]
    private static void AwakePostfix()
    {
        try
        {
            if (Plugin.IsInitialized) return;
            Plugin.Log.LogInfo("Creating Controls");
            Plugin.UIOnInitialize();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex.ToString());
        }
    }

    [HarmonyPatch(typeof(EscapeMenuView), "OnDestroy")]
    [HarmonyPrefix]
    private static void EscapeMenuViewOnDestroyPrefix()
    {
        if (!Plugin.IsInitialized) return;

        // User has left the server. Reset all ui as the next server might be a different one
        Plugin.Reset();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VersionString), nameof(VersionString.Start))]
    public static void VersionString_Start(VersionString __instance)
    {
        var tmp = __instance.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmp != null && UIFactory.Font == null)
        {
            UIFactory.Font = tmp.font;
            UIFactory.FontMaterial = tmp.fontMaterial;
        }
    }
}