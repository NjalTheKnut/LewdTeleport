using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using LewdTeleport.Gui;
using LewdTeleport.Managers;
using System;
using System.Security.Cryptography;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;

namespace LewdTeleport.Plugin {
    public sealed class LewdTeleportMain : IDalamudPlugin {
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService] public static IDataManager Data { get; set; } = null!;
        [PluginService] public static IClientState ClientState { get; set; } = null!;
        [PluginService] public static ICommandManager Commands { get; set; } = null!;
        [PluginService] public static IChatGui Chat { get; set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; set; } = null!;
        public static Configuration Config { get; set; } = new();

        public LewdTeleportMain() {
            AetheryteManager.Load();
            CommandManager.Load();
            
            Config = Configuration.Load();
            if (Config.UseEnglish)
                AetheryteManager.Load();

            IpcManager.Register(PluginInterface);

            PluginInterface.UiBuilder.Draw += OnDraw;
            PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
            ClientState.TerritoryChanged += OnTeleport;
        }

        private void OnTeleport(ushort obj)
        {
            Chat.Print("/wstrip -c");
            Chat.Print("/wstrip - C");
            Chat.Print("/wait 1");
            Chat.Print("/examineself motion < wait.1.5 >");
            Chat.Print("/moodle apply self preset \"ENF\"");
            Chat.Print("/surprised motion < wait.1.5 >");
            Chat.Print("/sad motion < wait.1 >");
            Chat.Print("/snap motion < wait.1.5 >");
            Chat.Print("/wdress");
            Chat.Print("/wait 0.5");
            Chat.Print("/examineself motion < wait.2 >");
            Chat.Print("/upset motion < wait.1.5 >");
            Chat.Print("/ppose stand 6");
            Chat.Print("/porch stop");
        }

        public static void LogChat(string message, bool error = false) {
            switch (error) {
                case true when Config.ChatError:
                    Chat.PrintError(message);
                    break;
                case false when Config.ChatMessage:
                    Chat.Print(message);
                    break;
            }
        }

        private static void OnDraw() {
            ConfigWindow.Draw();
        }

        public static void OnOpenConfigUi() {
            ConfigWindow.Enabled = !ConfigWindow.Enabled;
        }

        public void Dispose() {
            Config.Save();
            IpcManager.Unregister();
            CommandManager.UnLoad();
            PluginInterface.UiBuilder.Draw -= OnDraw;
            PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        }
    }
}