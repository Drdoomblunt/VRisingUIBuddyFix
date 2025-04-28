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
using UnityEngine;

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
        public static Plugin Instance { get; private set; }

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Instance = this;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            var t = new Timer(24000) { AutoReset = false};
            t.Elapsed += T_Elapsed;
            //t.Start();
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
                _pm.AddDrag("BloodOrbParent", "Blood Orb HP");

                _pm.AddDrag("JournalParent(Clone)", "Journal (left top anchor)");
                _pm.AddDrag("TargetInfoPanel(Clone)", "Target Info");
                _pm.AddDrag("RootCanvasGroup");

                _pm.AddDrag("Buffs");
                _pm.AddDrag("Debuffs");
                _pm.AddDrag("BottomBar(Clone)", "Full Bottom Bar"); //bottom bar
                _pm.AddDrag("BottomBar(Clone)|Content|Background|Background", "Bottom Bar Fade"); //bottom bar fade
                _pm.AddDrag("BottomBar(Clone)|Content|Background|DarkFade", "Bottom Bar BG"); //bottom bar bg
                _pm.AddDrag("BottomBar(Clone)|Content|Background|ActionBar", "Action Bar"); //action bar
                _pm.AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry", "ACB1"); //ab1
                _pm.AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry (1)", "ACB2"); //ab2
                _pm.AddDrag("BottomBar(Clone)|Content|Background|AbilityBar", "Ability Bar"); //abilityBar bar
                _pm.AddDrag("AbilityBarEntry_Primary", "AB primary");

                _pm.AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|HUDAlertParent(Clone)|Container", "Right Alerts"); //right alerts
                _pm.AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|DangerTextParent(Clone)|Alpha|Background", "Bottom Danger (big right shift)"); //bottom danger text
                _pm.AddDrag("HUDChatParent|ChatWindow(Clone)|Content", "Chat Window"); //chat

                _pm.AddDrag("ClockParent3(Clone)|Content|Parent", "DayNight sphere"); //daytime circle
                _pm.AddDrag("BackgroundBig", "Clock+Minimap BG"); //clock/minimap background
                _pm.AddDrag("MiniMapParent(Clone)|Root|Panel", "Minimap"); //minimap

                // _pm.AddDrag("HUDClan"); //clan
                // _pm.AddDrag("HUDTutorial"); //tutorial
                // _pm.AddDrag("HUDRecipeTrackerParent"); //recipe tracker


                /*_pm.AddDrag("SLS logo");
                _pm.AddDrag("NewsPanelParent");
                _pm.AddDrag("SideBar");
                _pm.AddDrag("LinksParentNode");*/
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
