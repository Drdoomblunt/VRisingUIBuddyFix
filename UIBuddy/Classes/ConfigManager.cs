using BepInEx.Configuration;

namespace UIBuddy.Classes
{
    public static class ConfigManager
    {
        private static ConfigEntry<bool> _cfgIsModVisible;
        private static ConfigEntry<bool> _cfgSelectPanelsWithMouse;
        private const string CATEGORY_SETTINGS = "Settings";

        public static bool SelectPanelsWithMouse
        {
            get => _cfgSelectPanelsWithMouse.Value;
            set => _cfgSelectPanelsWithMouse.Value = value;
        }

        public static bool IsModVisible
        {
            get => _cfgIsModVisible.Value;
            set => _cfgIsModVisible.Value = value;
        }

        public static void Initialize()
        {
            _cfgSelectPanelsWithMouse = Plugin.Instance.Config.Bind<bool>(CATEGORY_SETTINGS, nameof(SelectPanelsWithMouse), false,
                "Enable selecting panels with mouse");

            _cfgIsModVisible = Plugin.Instance.Config.Bind<bool>(CATEGORY_SETTINGS, nameof(SelectPanelsWithMouse), true,
                "Are mod panels visible");
        }
    }
}
