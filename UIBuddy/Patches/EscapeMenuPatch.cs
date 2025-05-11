using System.Collections;
using HarmonyLib;
using ProjectM.UI;
using UIBuddy.Classes;
using UIBuddy.Managers;
using UIBuddy.Utils;

namespace UIBuddy.Patches;

[HarmonyPatch]
public static class EscapeMenuPatch
{
    [HarmonyPatch(typeof(EscapeMenuView), "OnDestroy")]
    [HarmonyPrefix]
    private static void EscapeMenuViewOnDestroyPrefix()
    {
        if (!Plugin.IsInitialized) return;

        Plugin.Log.LogWarning("Game exit detected");
        PanelManager.CurrentScreenType = ScreenType.None;
    }

    [HarmonyPatch(typeof(HUDMenu), nameof(HUDMenu.OnEnable))]
    [HarmonyPostfix]
    static void OnEnablePostfix()
    {
        if (PanelManager.CurrentScreenType != ScreenType.CharacterHUD)
            return;
        PanelManager.CurrentScreenType = ScreenType.EscapeMenu;
        CoroutineUtility.StartCoroutine(DelayedReloadElements());
    }

    [HarmonyPatch(typeof(HUDMenu), nameof(HUDMenu.OnDisable))]
    [HarmonyPostfix]
    static void OnDisablePostfix()
    {
        if (PanelManager.CurrentScreenType != ScreenType.EscapeMenu)
            return;

        PanelManager.CurrentScreenType = ScreenType.CharacterHUD;
        CoroutineUtility.StartCoroutine(DelayedReloadElements());
    }

    private static IEnumerator DelayedReloadElements()
    {
        yield return null;
        yield return null;
        yield return null;

        PanelManager.ReloadElements();
    }

    [HarmonyPatch(typeof(EscapeMenuView), nameof(EscapeMenuView.OnEnable))]
    [HarmonyPostfix]
    private static void OnEnable()
    {
    }

    [HarmonyPatch(typeof(EscapeMenuView), nameof(EscapeMenuView.ResumeGame))]
    [HarmonyPostfix]
    private static void ResumeGame()
    {
    }
}