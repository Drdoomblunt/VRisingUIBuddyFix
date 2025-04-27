using System.Reflection;
using System.Timers;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
using UIBuddy.UI;

namespace UIBuddy
{
    [BepInProcess("VRising.exe")]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private static PanelManager _pm;
        internal new static ManualLogSource Log;
        private static Harmony Harmony;
        public static bool IsInitialized { get; set; }
        private static CoreUpdateBehavior _updateBehavior;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            var t = new Timer(24000) { AutoReset = false};
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            UIOnInitialize();
        }

        public static void UIOnInitialize()
        {
            try
            {
                if (Plugin.IsInitialized) return;
                IsInitialized = true;

                Log.LogInfo("Initializing UIBuddy...");
                EnsureThemeInitialized();
                ClassInjector.RegisterTypeInIl2Cpp<RectOutline>();
                _updateBehavior = new CoreUpdateBehavior();
                _updateBehavior.Setup();

                _pm = new PanelManager();
                _pm.AddDrag("SLS logo");
                _pm.AddDrag("NewsPanelParent");
                _pm.AddDrag("SideBar");
                _pm.AddDrag("LinksParentNode");
                // _pm.AddDrag(null);

                Log.LogInfo("UIBuddy initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Error initializing UIBuddy: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        public static void EnsureThemeInitialized()
        {
            // Force Theme class initialization early
            var tempOpacity = Theme.Opacity;
            Plugin.Log.LogInfo($"Theme initialized with opacity: {tempOpacity}");
        }

        public static void Reset()
        {
            _updateBehavior?.Dispose();
            _pm?.Dispose();
        }

        public override bool Unload()
        {
            _updateBehavior?.Dispose();
            Harmony.UnpatchSelf();
            return base.Unload();
        }
    }
}
