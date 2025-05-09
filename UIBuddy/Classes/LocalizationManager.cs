using FMOD;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Stunlock.Core;
using Stunlock.Localization;

namespace UIBuddy.Classes
{
    internal static class LocalizationManager
    {
        private static string _bindMenuSection;
        public static LocalizationKey BindMenuSectionKey { get; private set; }

        private static Dictionary<AssetGuid, string> _guids = new();

        public static void  Initialize()
        {
            _bindMenuSection = MyPluginInfo.PLUGIN_NAME;
            BindMenuSectionKey = AddKey(_bindMenuSection);
        }

        public static LocalizationKey AddKey(string key)
        {
            var guid = AssetGuid.FromGuid(Guid.NewGuid());
            _guids[guid] = key;
            return new LocalizationKey(guid);
        }

        public static bool HasKey(LocalizationKey key)
        {
            return _guids.ContainsKey(key.GetGuid());
        }

        public static bool HasKey(AssetGuid guid)
        {
            return _guids.ContainsKey(guid);
        }

        public static string GetKey(AssetGuid guid)
        {
            return _guids[guid];
        }

        public static string GetKey(LocalizationKey key)
        {
            return _guids[key.GetGuid()];
        }
    }
}
