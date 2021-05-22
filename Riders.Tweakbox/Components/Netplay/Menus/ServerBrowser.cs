using System;
using System.Collections.Generic;
using DearImguiSharp;
using EnumsNET;
using Riders.Tweakbox.API.Application.Commands.v1;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiSortDirection;
using static DearImguiSharp.ImGuiTableColumnFlags;
using static DearImguiSharp.ImGuiTableFlags;
using Task = System.Threading.Tasks.Task;

namespace Riders.Tweakbox.Components.Netplay.Menus
{
    public class ServerBrowser : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Server Browser";

        /// <summary>
        /// All of the servers to be displayed.
        /// </summary>
        public List<GetServerResultEx> Results { get; set; } = new List<GetServerResultEx>();

        // Actions
        public Func<GetServerResultEx, Task> Connect;
        public Func<Task> Refresh;

        /// <inheritdoc />
        public ServerBrowser(Func<GetServerResultEx, Task> connect, Func<Task> refresh)
        {
            Connect = connect;
            Refresh = refresh;
        }

        private ImGuiTableFlags _tableFlags = ImGuiTableFlagsRowBg | ImGuiTableFlagsBorders 
                                              | ImGuiTableFlagsNoBordersInBody | ImGuiTableFlagsScrollY 
                                              | ImGuiTableFlagsSortable | ImGuiTableFlagsContextMenuInBody
                                              | ImGuiTableFlagsResizable;

        private ImVec2.__Internal _rowMinHeight = new ImVec2.__Internal() { x = 0, y = 0 };
        private ImGuiSelectableFlags _flags     = ImGuiSelectableFlags.ImGuiSelectableFlagsSpanAllColumns;
        private GetServerResultEx _currentSelection;
        
        private HorizontalCenterHelper _centerHelperButton = new HorizontalCenterHelper();
        private HorizontalCenterHelper _centerHelperPlayer = new HorizontalCenterHelper();
        private ImVec4 _passwordColor = Utilities.HexToFloat(0xF0C84FFF);

        /// <summary>
        /// Render all of the available results.
        /// </summary>
        public override unsafe void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                // Get client area
                var contentRegionWidth = ImGui.GetWindowContentRegionWidth();
                var results = Results;

                // Server List
                RenderServerList(contentRegionWidth, results);
                RenderRightColumn(contentRegionWidth, results);
            }

            ImGui.End();
        }

        private void RenderRightColumn(float contentRegionWidth, List<GetServerResultEx> results)
        {
            // Player Table
            ImGui.SameLine(0, 0);
            ImGui.BeginGroup();

            var availableWidth = contentRegionWidth * 0.25f;
            _centerHelperPlayer.Begin(availableWidth);
            var maxPlayers = _currentSelection?.Type.GetNumTeams() * _currentSelection?.Type.GetNumPlayersPerTeam();
            ImGui.TextUnformatted($"Players {_currentSelection?.Players.Count:00} / {maxPlayers:00}", null);
            _centerHelperPlayer.End();

            var playerTableSize = new ImVec2.__Internal() {x = availableWidth, y = -50};
            if (ImGui.__Internal.BeginTable("player_table", 2, (int) (_tableFlags & ~ImGuiTableFlagsSortable), playerTableSize, 0))
            {
                // Create Headers
                ImGui.TableSetupColumn("Name", 0, 0, 0);
                ImGui.TableSetupColumn("Ping", (int) ImGuiTableColumnFlagsWidthFixed, 0, 1);
                ImGui.TableSetupScrollFreeze(1, 1);

                // Show Headers
                ImGui.TableHeadersRow();

                // Render items
                if (_currentSelection != null)
                {
                    for (int x = 0; x < _currentSelection.Players.Count; x++)
                    {
                        // Setup
                        var item = _currentSelection.Players[x];
                        ImGui.PushIDInt(x);
                        ImGui.TableNextRow(0, 0);

                        // Name
                        if (ImGui.TableSetColumnIndex(0))
                            ImGui.TextUnformatted(item.Name, null);

                        // Ping
                        if (ImGui.TableSetColumnIndex(1))
                            ImGui.TextUnformatted(item.Latency.ToString(), null);

                        // Cleanup
                        ImGui.PopID();
                    }
                }

                ImGui.EndTable();
            }

            _centerHelperButton.Begin(availableWidth);
            if (ImGui.Button("Connect", Constants.ButtonSize))
                Connect?.Invoke(_currentSelection);

            _centerHelperButton.End();
            _centerHelperButton.Begin(availableWidth);
            if (ImGui.Button("Refresh", Constants.ButtonSize))
                Refresh?.Invoke();

            _centerHelperButton.End();
            ImGui.EndGroup();
        }

        private void RenderServerList(float contentRegionWidth, List<GetServerResultEx> results)
        {
            // Display a few columns:
            // Name, Type, Continent, Ping
            var serverTableSize = new ImVec2.__Internal() {x = contentRegionWidth * 0.75f};
            if (ImGui.__Internal.BeginTable("server_table", (int) TableColumn.Ping, (int) _tableFlags, serverTableSize, 0))
            {
                // Create Headers
                ImGui.TableSetupColumn("Name", 0, 0, (uint) TableColumn.Name);
                ImGui.TableSetupColumn("Type", (int) ImGuiTableColumnFlagsWidthFixed, 0, (uint) TableColumn.Type);
                ImGui.TableSetupColumn("Continent", (int) ImGuiTableColumnFlagsWidthFixed, 0, (uint) TableColumn.Region);
                ImGui.TableSetupColumn("Mods", (int) ImGuiTableColumnFlagsWidthFixed, 0, (uint) TableColumn.Mods);
                ImGui.TableSetupColumn("Ping", (int) ImGuiTableColumnFlagsWidthFixed, 0, (uint) TableColumn.Ping);
                ImGui.TableSetupScrollFreeze(1, 1);

                // Sort our data if sort specs have changed.
                using var sortSpecs = ImGui.TableGetSortSpecs();
                bool itemsNeedSort = sortSpecs?.SpecsDirty ?? false;

                if (itemsNeedSort && results.Count > 1)
                {
                    SortResults(sortSpecs, results);
                    sortSpecs.SpecsDirty = false;
                }

                // Show Headers
                ImGui.TableHeadersRow();

                // Render items
                for (int x = 0; x < results.Count; x++)
                {
                    // Setup
                    var item = results[x];
                    bool isSelected = item == _currentSelection;

                    ImGui.PushIDInt(x);
                    ImGui.TableNextRow(0, 0);

                    // Password Color Set
                    if (item.HasPassword)
                        ImGui.PushStyleColorVec4((int) ImGuiCol.ImGuiColText, _passwordColor);

                    // Name (Selectable)
                    ImGui.TableSetColumnIndex(0);
                    if (ImGui.__Internal.SelectableBool(item.Name, isSelected, (int) _flags, _rowMinHeight))
                        _currentSelection = item;

                    // Type
                    if (ImGui.TableSetColumnIndex(1))
                        ImGui.TextUnformatted(item.Type.AsString(), null);

                    // Continent
                    if (ImGui.TableSetColumnIndex(2))
                        ImGui.TextUnformatted(item.ContinentString, null);

                    // Password
                    if (ImGui.TableSetColumnIndex(3))
                        ImGui.TextUnformatted(item.Mods, null);

                    // Ping
                    if (ImGui.TableSetColumnIndex(4))
                        ImGui.TextUnformatted(item.Ping?.ToString() ?? "???", null);

                    // Password Color Unset
                    if (item.HasPassword)
                        ImGui.PopStyleColor(1);

                    // Cleanup
                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        }

        private void SortResults(ImGuiTableSortSpecs sortSpecs, List<GetServerResultEx> results)
        {
            var specs = sortSpecs.Specs;
            var index = specs.ColumnUserID;

            var compareModifier = specs.SortDirection == (int) ImGuiSortDirectionDescending ? -1 : 1;
            switch (index)
            {
                case (int) TableColumn.Name:
                    results.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal) * compareModifier);
                    break;

                case (int) TableColumn.Type:
                    results.Sort((a, b) => a.Type.CompareTo(b.Type) * compareModifier);
                    break;

                case (int) TableColumn.Region:
                    results.Sort((a, b) => String.Compare(a.ContinentString, b.ContinentString, StringComparison.Ordinal) * compareModifier);
                    break;

                case (int) TableColumn.Mods:
                    results.Sort((a, b) => String.Compare(a.Mods, b.Mods, StringComparison.Ordinal) * compareModifier);
                    break;

                case (int) TableColumn.Ping:
                    results.Sort((a, b) => Nullable.Compare(a.Ping, b.Ping) * compareModifier);
                    break;
            }
        }

        private enum TableColumn
        {
            Name = 1,
            Type,
            Region,
            Mods,
            Ping,
        }
    }
}
