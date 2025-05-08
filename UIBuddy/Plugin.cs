using System.Reflection;
using System.Timers;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using ProjectM.EOS;
using UIBuddy.Classes;
using UIBuddy.Classes.Behaviors;
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
        private static Harmony Harmony;
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

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Keybindings.Initialize();

            var t = new Timer(24000) { AutoReset = false};
            t.Elapsed += (s,e)=> UIOnInitialize(); 
            //t.Start();
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

                var kb = KeybindManager.Register(new KeybindingDescription
                {
                    Id = $"{MyPluginInfo.PLUGIN_GUID}_SHOW",
                    Name = "Show/Hide UIBuddy",
                    Category = MyPluginInfo.PLUGIN_NAME,
                    DefaultKeybinding = KeyCode.PageDown
                });
                kb.KeyPressed += Kb_KeyPressed;

                _pm = new PanelManager();
                Instance.ReloadElements();

                Log.LogInfo("UIBuddy initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Error initializing UIBuddy: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }

        private static void Kb_KeyPressed()
        {
            PanelManager.MainPanel.EnableMainPanel(!PanelManager.MainPanel.IsRootActive);
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
            Keybindings.Uninitialize();
            return base.Unload();
        }

        public void ReloadElements()
        {
            _pm.AddDrag("BloodOrbParent", "Blood Orb HP");
            _pm.AddDetachedDrag("BottomBar(Clone)|Content|TooltipParent|BloodPoolTooltip", "Blood Orb Tooltip Anchor", "BOTA");

            _pm.AddDrag("JournalParent(Clone)", "Journal (left top anchor)");
            _pm.AddDrag("TargetInfoPanel(Clone)", "Target Info");
            _pm.AddDrag("Buffs", "Buffs");
            _pm.AddDrag("Debuffs", "Debuffs");
            _pm.AddDrag("BottomBar(Clone)", "Full Bottom Bar"); //bottom bar
            _pm.AddDrag("BottomBar(Clone)|Content|Background|Background", "Bottom Bar Fade"); //bottom bar fade
            _pm.AddDrag("BottomBar(Clone)|Content|Background|DarkFade", "Bottom Bar BG"); //bottom bar bg
            _pm.AddDetachedDrag("BottomBar(Clone)|Content|Background|ActionBar", "Action Bar", "ACBAR"); //action bar
            _pm.AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry", "ACB1"); //ab1
            _pm.AddDrag("BottomBar(Clone)|Content|Background|ActionBar|ActionBarEntry (1)", "ACB2"); //ab2
            _pm.AddDetachedDrag("BottomBar(Clone)|Content|Background|AbilityBar", "Ability Bar", "ABBAR"); //abilityBar bar
            _pm.AddDrag("AbilityBarEntry_Primary", "AB primary");

            _pm.AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|HUDAlertParent(Clone)|Container", "Right Alerts"); //right alerts
            _pm.AddDrag("HUDCanvas(Clone)|Canvas|HUDOther|DangerTextParent(Clone)|Alpha|Background", "Bottom Danger (big right shift!)"); //bottom danger text
            var chatPanel = _pm.AddDetachedDrag("HUDChatParent|ChatWindow(Clone)|Content", "Chat Window", "CHAT"); //chat
            if (chatPanel != null)
                chatPanel.SetParameters(positionValidation: false, initialPosition: new Vector2(500,500));

            _pm.AddDrag("ClockParent3(Clone)|Content|Parent", "DayNight sphere"); //daytime circle
            _pm.AddDrag("BackgroundBig", "Clock+Minimap BG"); //clock/minimap background
            _pm.AddDrag("MiniMapParent(Clone)|Root|Panel", "Minimap"); //minimap

            _pm.AddDrag("Version_HUD", "Clan Members");
            // _pm.AddDrag("HUDClan"); //clan
            // _pm.AddDrag("HUDTutorial"); //tutorial
            // _pm.AddDrag("HUDRecipeTrackerParent"); //recipe tracker


            /*_pm.AddDrag("SLS logo");
            _pm.AddDrag("NewsPanelParent");
            _pm.AddDrag("SideBar");
            _pm.AddDrag("LinksParentNode");*/
            // _pm.AddDrag(null);
        }

        public static void GameDataOnInitialize(World world)
        {
            VWorld = world;
        }

    }
}
