using System;
using System.Linq;
using ImGuiNET;
using LewdTeleport.Managers;
using LewdTeleport.Plugin;

namespace LewdTeleport.Gui {
    public static class ConfigWindow {
        private static bool m_Visible;
        public static bool Enabled {
            get => m_Visible;
            set {
                if (value) AetheryteManager.UpdateAvailableAetherytes();
                m_AetheryteFilter = string.Empty;
                m_Visible = value;
            }
        }

        private static TeleportAlias m_DummyAlias = new();
        private static string m_AetheryteFilter = string.Empty;

        public static void Draw() {
            if(!m_Visible) return;
            try {
                if (!ImGui.Begin("Lewd Teleport Config", ref m_Visible)) return;

                if(ImGui.BeginTabBar("##tpConfigTabs")) {
                    if (ImGui.BeginTabItem("General")) {
                        DrawGeneralTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Alias")) {
                        DrawAliasTab();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            } finally {
                ImGui.End();
            }
        }

        private static void DrawGeneralTab() {
            if (ImGui.Checkbox("Show Chat Messages", ref LewdTeleportMain.Config.ChatMessage))
                LewdTeleportMain.Config.Save();
            ImGui.SameLine();
            if (ImGui.Checkbox("Show Error Messages", ref LewdTeleportMain.Config.ChatError)) 
                LewdTeleportMain.Config.Save();
            if (ImGui.Checkbox("Use English Aetheryte Names", ref LewdTeleportMain.Config.UseEnglish))
                AetheryteManager.Load();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("This does not apply to Estate Names (Appartment, Shared Estate etc.)");

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Allow Partial Match:");
            ImGui.SameLine();
            if (ImGui.Checkbox("Aetheryte", ref LewdTeleportMain.Config.AllowPartialName))
                LewdTeleportMain.Config.Save();
            ImGui.SameLine();
            if (ImGui.Checkbox("Alias", ref LewdTeleportMain.Config.AllowPartialAlias))
                LewdTeleportMain.Config.Save();

            ImGui.Separator();
            if(ImGui.Checkbox("Grand Company Ticket Teleport", ref LewdTeleportMain.Config.EnableGrandCompany))
                LewdTeleportMain.Config.Save();
            if (LewdTeleportMain.Config.EnableGrandCompany) {
                ImGui.SameLine();
                ImGui.TextUnformatted(" /ltp");
                ImGui.SetNextItemWidth(80);
                ImGui.SameLine();
                ImGui.InputText("##GcAlias", ref LewdTeleportMain.Config.GrandCompanyAlias, 512);
            }

            if (ImGui.Checkbox("Eternity Ring Teleport", ref LewdTeleportMain.Config.EnableEternityRing))
                LewdTeleportMain.Config.Save();
            if (LewdTeleportMain.Config.EnableEternityRing) {
                ImGui.SameLine();
                ImGui.TextUnformatted(" /ltp");
                ImGui.SetNextItemWidth(80);
                ImGui.SameLine();
                ImGui.InputText("##RingAlias", ref LewdTeleportMain.Config.EternityRingAlias, 512);
            }
        }
        
        private static void DrawAliasTab() {
            if (!ImGui.BeginTable("##tpAliasTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.PadOuterX)) return;
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Alias", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Aetheryte", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##aliasBtns", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var list = LewdTeleportMain.Config.AliasList.ToList();
            for (var i = -1; i < list.Count; i++) {
                var alias = i < 0 ? m_DummyAlias : list[i];
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                ImGui.SetCursorPosX(0);
                if (ImGui.InputText($"##alias{i}Input", ref alias.Alias, 512, ImGuiInputTextFlags.EnterReturnsTrue)) {
                    if (i == -1) {
                        LewdTeleportMain.Config.AliasList.Insert(0, alias);
                        m_DummyAlias = new TeleportAlias();
                    }
                    LewdTeleportMain.Config.Save();
                }
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(alias.Aetheryte);
                ImGui.TableNextColumn();
                if (ImGui.BeginCombo($"##alias{i}AetheryteCombo", string.Empty, ImGuiComboFlags.NoPreview)) {
                    if (AetheryteManager.AvailableAetherytes.Count == 0) {
                        ImGui.TextUnformatted("No Aetherytes Available");
                        if (ImGui.Selectable("Click here to Update"))
                            AetheryteManager.UpdateAvailableAetherytes();
                    } else {
                        ImGui.InputText("Search##aetheryteFilter", ref m_AetheryteFilter, 512);
                        foreach (var info in AetheryteManager.AvailableAetherytes) {
                            var name = AetheryteManager.GetAetheryteName(info);
                            if (!string.IsNullOrEmpty(m_AetheryteFilter) && !name.Contains(m_AetheryteFilter, StringComparison.OrdinalIgnoreCase))
                                continue;

                            var selected = alias.Aetheryte.Equals(name, StringComparison.OrdinalIgnoreCase);
                            if (ImGui.Selectable(name, selected)) {
                                alias.Update(info);
                                if (i == -1) {
                                    LewdTeleportMain.Config.AliasList.Insert(0, alias);
                                    m_DummyAlias = new TeleportAlias();
                                }

                                LewdTeleportMain.Config.Save();
                            }

                            if (selected) ImGui.SetItemDefaultFocus();
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                if (i != -1 && ImGui.Button($" X ##alias{i}delete")) {
                    LewdTeleportMain.Config.AliasList.Remove(alias);
                    LewdTeleportMain.Config.Save();
                }
            }
            ImGui.EndTable();
        }
    }
}