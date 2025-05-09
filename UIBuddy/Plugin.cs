using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
using UIBuddy.Classes.KeyBinds;
using UIBuddy.Patches;
using UIBuddy.UI;
using Unity.Entities;
using UnityEngine;

namespace UIBuddy
{
    [BepInProcess("VRising.exe")]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private static PanelManager _pm;
        internal new static ManualLogSource Log;
        private static Harmony _harmony;
        public static bool IsInitialized { get; set; }
        private static CoreUpdateBehavior _updateBehavior;
        public static Plugin Instance { get; private set; }
        public static World VWorld { get; private set; }
        public static bool IsClient { get; private set; }

        public override void Load()
        {
            IsClient = Application.productName != "VRisingServer";
            if (!IsClient)
            {
                Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}[{MyPluginInfo.PLUGIN_VERSION}] is a client mod! ({Application.productName})");
                return;
            }

            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Instance = this;

            LocalizationManager.Initialize();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            KeybindingPatch.Initialize();
            KeybindingManager.LoadBindings();

            _updateBehavior = new CoreUpdateBehavior();
            _updateBehavior.Setup();

            ClassInjector.RegisterTypeInIl2Cpp<RectOutline>();

            //var t = new Timer(24000) { AutoReset = false};
            //t.Elapsed += (s,e)=> UIOnInitialize(); 
            //t.Start();
        }

        public static void UIOnInitialize()
        {
            try
            {
                if (IsInitialized) return;
                IsInitialized = true;

                Log.LogInfo("Initializing UIBuddy...");
                EnsureThemeInitialized();
                ConfigManager.Initialize();

                _pm = new PanelManager();
                PanelManager.ReloadElements();

                Log.LogInfo("UIBuddy initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Error initializing UIBuddy: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        private static void EnsureThemeInitialized()
        {
            // Force Theme class initialization early
            var tempOpacity = Theme.Opacity;
            Log.LogInfo($"Theme initialized with opacity: {tempOpacity}");
        }

        public static void Reset()
        {
            _pm?.Dispose();
        }

        public override bool Unload()
        {
            _updateBehavior?.Dispose();
            _harmony.UnpatchSelf();
            KeybindingPatch.Deinitialize();
            KeybindingManager.UnloadBindings();
            return base.Unload();
        }

        public static void GameDataOnInitialize(World world)
        {
            VWorld = world;
        }
    }
}
