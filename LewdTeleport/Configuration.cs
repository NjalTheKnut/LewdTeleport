using System.Collections.Generic;
using Dalamud.Configuration;
using LewdTeleport.Managers;
using LewdTeleport.Plugin;

namespace LewdTeleport {
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 2;

        public bool UseEnglish = false;

        public bool EnableNSFW = false;
        public bool ForceNSFW = false;

        public bool AllowPartialName = true;
        public bool AllowPartialAlias = false;

        public bool ChatMessage = true;
        public bool ChatError = true;

        public bool EnableGrandCompany = false;
        public string GrandCompanyAlias = "gc";

        public bool EnableEternityRing = false;
        public string EternityRingAlias = "ring";
        
        public List<TeleportAlias> AliasList = new();

        #region Helper

        public void Save() {
            LewdTeleportMain.PluginInterface.SavePluginConfig(this);
        }

        public static Configuration Load() {
            var config = LewdTeleportMain.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            foreach (var alias in config.AliasList)
                alias.Aetheryte = AetheryteManager.GetAetheryteName(alias);
            return config;
        }
        
        #endregion
    }
}