using System;
using System.Linq;
using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using LewdTeleport.Plugin;

namespace LewdTeleport.Managers {
    public static class CommandManager {
        public static void Load() {
            LewdTeleportMain.Commands.AddHandler("/ltp", new CommandInfo(CommandHandler) {
                ShowInHelp = true,
                HelpMessage = "/ltp <aetheryte name> - Teleport to aetheryte (use just '/ltp' to open the Config)"
            });
            LewdTeleportMain.Commands.AddHandler("/lctp", new CommandInfo(CommandHandlerMaps) {
                ShowInHelp = true,
                HelpMessage = "/lctp <map name> - Teleport to map"
            });
        }

        public static void UnLoad() {
            LewdTeleportMain.Commands.RemoveHandler("/ltp");
            LewdTeleportMain.Commands.RemoveHandler("/lctp");
        }

        private static void CommandHandlerMaps(string cmd, string arg) {
            if (string.IsNullOrEmpty(arg)) {
                LewdTeleportMain.OnOpenConfigUi();
                return;
            }

            if (LewdTeleportMain.ClientState.LocalPlayer == null)
                return;

            AetheryteManager.UpdateAvailableAetherytes();

            arg = CleanArgument(arg);

            if (!AetheryteManager.TryFindAetheryteByMapName(arg, LewdTeleportMain.Config.AllowPartialName, out var info)) {
                LewdTeleportMain.LogChat($"No attuned Aetheryte found for '{arg}'.", true);
                return;
            }

            LewdTeleportMain.LogChat($"Teleporting to {AetheryteManager.GetAetheryteName(info)}.");
            TeleportManager.Teleport(info);
        }

        private static void CommandHandler(string cmd, string arg) {
            arg = CleanArgument(arg);

            if (string.IsNullOrEmpty(arg)) {
                LewdTeleportMain.OnOpenConfigUi();
                return;
            }

            if(LewdTeleportMain.ClientState.LocalPlayer == null)
                return;

            AetheryteManager.UpdateAvailableAetherytes();
            
            if (LewdTeleportMain.Config.EnableGrandCompany &&
                arg.Equals(LewdTeleportMain.Config.GrandCompanyAlias, StringComparison.OrdinalIgnoreCase)) {
                unsafe {
	                var gc = PlayerState.Instance()->GrandCompany;
                    if (gc == 0) return;
                    uint gcTicket = gc switch {
                        1 => 21069, //Maelstrom
                        2 => 21070, //Order of the Twin Adder
                        3 => 21071, //Immortal Flames
                        _ => 0
                    };
                    if (gcTicket == 0)
                        return;

                    var cnt = InventoryManager.Instance()->GetInventoryItemCount(gcTicket);
                    if (cnt < 1) {
                        LewdTeleportMain.LogChat("You do not have the required GC Tickets.", true);
                        return;
                    }

                    var gcName = TryGetGrandCompanyName(gc, out var name) ? name : $"GrandCompany{gc}";
                    LewdTeleportMain.LogChat($"Teleporting to {gcName}.");
                    ActionManager.Instance()->UseAction(ActionType.Item, gcTicket, 0xE000_0000, 65535);
                    return;
                }
            }

            if (LewdTeleportMain.Config.EnableEternityRing 
                && arg.Equals(LewdTeleportMain.Config.EternityRingAlias, StringComparison.OrdinalIgnoreCase)) {
                unsafe {
                    var cnt = InventoryManager.Instance()->GetInventoryItemCount(8575);
                    if (cnt < 1) {
                        LewdTeleportMain.LogChat("You do not have the Eternity Ring.", true);
                        return;
                    }
                    LewdTeleportMain.LogChat("Teleporting to Partner.");
                    ActionManager.Instance()->UseAction(ActionType.Item, 8575, 0xE000_0000, ushort.MaxValue);
                    return;
                }
            }

            if (TryFindAliasByName(arg, LewdTeleportMain.Config.AllowPartialAlias, out var alias)) {
                if (alias.Aetheryte.Equals("NO_DATA", StringComparison.OrdinalIgnoreCase)) {
                    LewdTeleportMain.LogChat($"Invalid Alias: {alias.Alias} -> {alias.Aetheryte}", true);
                    return;
                }

                LewdTeleportMain.LogChat($"Teleporting to {AetheryteManager.GetAetheryteName(alias)}.");
                TeleportManager.Teleport(alias);
                return;
            }

            if (!AetheryteManager.TryFindAetheryteByName(arg, LewdTeleportMain.Config.AllowPartialName, out var info)) {
                LewdTeleportMain.LogChat($"No attuned Aetheryte found for '{arg}'.", true);
                return;
            }

            LewdTeleportMain.LogChat($"Teleporting to {AetheryteManager.GetAetheryteName(info)}.");
            TeleportManager.Teleport(info);
        }

        private static string CleanArgument(string arg) {
            //remove autotranslate arrows and double spaces
            arg = arg.Replace("\xe040", "").Replace("\xe041", "");
            arg = arg.Replace("  ", " ");
            return arg.Trim();
        }

        private static bool TryGetGrandCompanyName(byte id, out string name) {
            name = string.Empty;
            if (id == 0) return false;
            var sheet = LewdTeleportMain.Data.GetExcelSheet<GrandCompany>();
            var row = sheet?.GetRow(id);
            if (row == null) return false;
            name = row.Name.ToString();
            return !string.IsNullOrEmpty(name);
        }

        private static bool TryFindAliasByName(string name, bool matchPartial, out TeleportAlias alias) {
            //TODO Support multiple matches, maybe by checking which of the matches can be used and only return that
            alias = new TeleportAlias();
            foreach (var teleportAlias in LewdTeleportMain.Config.AliasList) {
                var result = matchPartial && teleportAlias.Alias.Contains(name, StringComparison.OrdinalIgnoreCase);
                if (!result && !teleportAlias.Alias.Equals(name, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!AetheryteManager.AvailableAetherytes.Any(i => teleportAlias.Equals(i)))
                    continue;
                alias = teleportAlias;
                return true;
            }
            return false;
        }
    }
}