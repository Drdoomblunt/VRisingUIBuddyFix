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

        private static readonly Dictionary<AssetGuid, string> Guids = new();

        public static void  Initialize()
        {
            _bindMenuSection = MyPluginInfo.PLUGIN_NAME;
            BindMenuSectionKey = AddKey(_bindMenuSection);
        }

        public static LocalizationKey AddKey(string key)
        {
            var guid = AssetGuid.FromGuid(Guid.NewGuid());
            Guids[guid] = key;
            return new LocalizationKey(guid);
        }

        public static bool HasKey(LocalizationKey key)
        {
            return Guids.ContainsKey(key.GetGuid());
        }

        public static bool HasKey(AssetGuid guid)
        {
            return Guids.ContainsKey(guid);
        }

        public static string GetKey(AssetGuid guid)
        {
            return Guids[guid];
        }

        public static string GetKey(LocalizationKey key)
        {
            return Guids[key.GetGuid()];
        }
    }
}
