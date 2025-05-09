using System;
using System.Collections;
using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using StunShared.UI;
using UIBuddy.KeyBinds;
using UIBuddy.Managers;
using UIBuddy.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace UIBuddy.Patches
{
    [HarmonyPatch]
    internal static class InputSystemPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InputActionSystem), nameof(InputActionSystem.OnUpdate))]
        private static void OnUpdate()
        {
            KeybindRegistrator.Update();
        }


        [HarmonyPatch(typeof(RebindingMenu), nameof(RebindingMenu.Start))]
        [HarmonyPostfix]
        static void StartPostfix(RebindingMenu __instance)
        {
            if (__instance._BindingTypeToDisplay != ControllerType.KeyboardAndMouse)
            {
                return;
            }

            __instance.AddHeader(LocalizationManager.BindMenuSectionKey);

            foreach (var keybind in KeybindRegistrator.KeybindingsById.Values)
            {
                var settingsEntryBinding =
                    UIHelper.InstantiatePrefabUnderAnchor(__instance.ControlsInputEntryPrefab, __instance.ContentNode);

                settingsEntryBinding.Initialize(
                    ControllerType.KeyboardAndMouse,
                    keybind.InputFlag,
                    AnalogInputAction.None,
                    true,
                    false,
                    true,
                    onClick: (Il2CppSystem.Action<SettingsEntry_Binding, bool, ButtonInputAction, AnalogInputAction,
                        bool>)__instance.OnEntryButtonClicked,
                    onClear: (Il2CppSystem.Action<SettingsEntry_Binding, ButtonInputAction>)__instance.OnEntryCleared,
                    true
                );

                settingsEntryBinding.SetInputInfo(keybind.Description.NameKey, keybind.Description.DescriptionKey);
                settingsEntryBinding.SetPrimary(keybind.Primary.ToString());
                settingsEntryBinding.SetSecondary(keybind.Secondary.ToString());

                settingsEntryBinding.PrimaryButton.onClick.AddListener((UnityAction)(() =>
                    RefreshKeybind(__instance, settingsEntryBinding, keybind).Start()));
                settingsEntryBinding.SecondaryButton.gameObject.SetActive(false);

                SettingsEntryBase settingsEntryBase = settingsEntryBinding;
                __instance.EntriesSelectionGroup.AddEntry(ref settingsEntryBase);
            }
        }

        static IEnumerator RefreshKeybind(RebindingMenu __instance, SettingsEntry_Binding binding, Keybinding keybind)
        {
            KeyCode newKey = KeyCode.None;
            __instance.OnEntryButtonClicked(binding, true, ButtonInputAction.None, AnalogInputAction.None, true);

            while (true)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        if (key == KeyCode.Escape)
                        {
                            Plugin.Log.LogWarning($"[{binding.name}] Rebind cancelled");

                            __instance.OnRebindingCancel(false, false);

                            yield break;
                        }

                        if (key == KeyCode.Backspace)
                        {
                            Plugin.Log.LogWarning($"[{binding.name}] Rebind cancelled");

                            __instance.OnEntryCleared(binding, ButtonInputAction.None);
                            KeybindRegistrator.Rebind(keybind, KeyCode.None);
                            binding.SetPrimary(keybind.Primary.ToString());

                            yield break;
                        }

                        newKey = key;
                        break;
                    }
                }

                if (newKey != KeyCode.None)
                    break;

                yield return null;
            }

            KeybindRegistrator.Rebind(keybind, newKey);
            __instance.OnRebindingComplete(false);
            binding.SetPrimary(keybind.Primary.ToString());

            Plugin.Log.LogWarning($"[{binding.name}] rebound to {newKey}");
        }
    }
}
