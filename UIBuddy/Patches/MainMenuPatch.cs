using HarmonyLib;
using ProjectM.UI;
using System;
using UIBuddy.Classes;
using UIBuddy.Managers;

namespace UIBuddy.Patches
{
    [HarmonyPatch]
    internal static class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenuNewView), nameof(MainMenuNewView.SetConsoleReady))] // reset bools and object states when exiting world
        [HarmonyPostfix]
        static void SetConsoleReadyPostfix()
        {
            try
            {
                Plugin.UIOnInitialize(ScreenType.MainMenu);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"MainMenuNewView.SetConsoleReady error: {ex}");
            }
        }

        [HarmonyPatch(typeof(MainMenuNewView), nameof(MainMenuNewView.OnDestroy))] // reset bools and object states when exiting world
        [HarmonyPostfix]
        static void OnDestroy()
        {
            try
            {
                PanelManager.CurrentScreenType = ScreenType.None;
                PanelManager.ReloadElements();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"MainMenuNewView.SetConsoleReady error: {ex}");
            }
        }
    }
}
