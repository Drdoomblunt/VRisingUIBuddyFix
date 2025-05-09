using BepInEx.Configuration;

namespace UIBuddy.Managers
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
            if(_cfgSelectPanelsWithMouse != null) return;

            _cfgSelectPanelsWithMouse = Plugin.Instance.Config.Bind(CATEGORY_SETTINGS, nameof(SelectPanelsWithMouse), false,
                "Enable selecting panels with mouse");

            _cfgIsModVisible = Plugin.Instance.Config.Bind(CATEGORY_SETTINGS, nameof(IsModVisible), true,
                "Are mod panels visible");
        }
    }
}
