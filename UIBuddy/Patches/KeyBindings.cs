using BepInEx.Unity.IL2CPP.Hook;
using HarmonyLib;

namespace UIBuddy.Patches;

static class Keybindings
{
#nullable disable
    private static Harmony _harmony;
    private static INativeDetour _detour;
#nullable enable

    public static void Initialize()
    {
        if (!Plugin.IsClient) return;

        _harmony = Harmony.CreateAndPatchAll(typeof(Keybindings));
    }

    public static void Uninitialize()
    {
        if (!Plugin.IsClient) return;

        _detour.Dispose();
        _harmony.UnpatchSelf();
    }
}