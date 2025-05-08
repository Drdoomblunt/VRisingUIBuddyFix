using HarmonyLib;
using ProjectM;
using UIBuddy.Classes;

namespace UIBuddy.Patches
{
    [HarmonyPatch]
    internal static class InputSystemPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InputActionSystem), nameof(InputActionSystem.OnUpdate))]
        private static void OnUpdate()
        {
            foreach (var kb in KeybindManager._keybindingsById.Values)
            {
                if (kb.IsPressed)
                    kb.OnKeyPressed();
            }
        }
    }
}
