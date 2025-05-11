using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using System;
using TMPro;
using UIBuddy.Classes;
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
            //if (Plugin.IsInitialized) return;
            //Plugin.Log.LogInfo("Creating Controls");
            Plugin.UIOnInitialize(ScreenType.CharacterHUD);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex.ToString());
        }
    }

    [HarmonyPatch(typeof(GameDataManager), "OnUpdate")]
    [HarmonyPostfix]
    private static void GameDataManagerOnUpdatePostfix(GameDataManager __instance)
    {
        try
        {
            if (!__instance.GameDataInitialized) return;
            Plugin.GameDataOnInitialize(__instance.World);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex.ToString());
        }
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