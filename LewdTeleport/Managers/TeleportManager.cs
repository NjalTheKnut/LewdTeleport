using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using LewdTeleport.Plugin;

namespace LewdTeleport.Managers {
    public static unsafe class TeleportManager {
        public static bool Teleport(TeleportInfo info) {
            if (LewdTeleportMain.ClientState.LocalPlayer == null)
                return false;
            var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, 5);
            if (status != 0) {
                var msg = GetLogMessage(status);
                if (!string.IsNullOrEmpty(msg))
                    LewdTeleportMain.LogChat(msg, true);
                return false;
            }

            if (LewdTeleportMain.ClientState.LocalPlayer.CurrentWorld.Id != LewdTeleportMain.ClientState.LocalPlayer.HomeWorld.Id) {
                if (AetheryteManager.IsHousingAetheryte(info.AetheryteId, info.Plot, info.Ward, info.SubIndex)) {
                    LewdTeleportMain.LogChat($"Unable to Teleport to {AetheryteManager.GetAetheryteName(info)} while visiting other Worlds.", true);
                    return false;
                }
            }

            return Telepo.Instance()->Teleport(info.AetheryteId, info.SubIndex);
        }

        public static bool Teleport(TeleportAlias alias) {
            if (LewdTeleportMain.ClientState.LocalPlayer == null)
                return false;
            var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, 5);
            if (status != 0) {
                var msg = GetLogMessage(status);
                if(!string.IsNullOrEmpty(msg))
                    LewdTeleportMain.LogChat(msg, true);
                return false;
            }

            if (LewdTeleportMain.ClientState.LocalPlayer.CurrentWorld.Id != LewdTeleportMain.ClientState.LocalPlayer.HomeWorld.Id) {
                if (AetheryteManager.IsHousingAetheryte(alias.AetheryteId, alias.Plot, alias.Ward, alias.SubIndex)) {
                    LewdTeleportMain.LogChat($"Unable to Teleport to {alias.Aetheryte} while visiting other Worlds.", true);
                    return false;
                }
            }
            return Telepo.Instance()->Teleport(alias.AetheryteId, alias.SubIndex);
        }

        private static string GetLogMessage(uint id) {
            var sheet = LewdTeleportMain.Data.GetExcelSheet<LogMessage>();
            if (sheet == null) return string.Empty;
            var row = sheet.GetRow(id);
            return row == null ? string.Empty : row.Text.ToString();
        }
    }
}