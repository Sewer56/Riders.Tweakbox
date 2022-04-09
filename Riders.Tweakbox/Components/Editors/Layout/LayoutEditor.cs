using System;
using System.IO;
using System.Numerics;
using DearImguiSharp;
using EnumsNET;
using Microsoft.Win32;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.ObjectLayoutController;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Parser.Layout.Structs;
using Sewer56.SonicRiders.Structures.Enums;
using static DearImguiSharp.ImGuiTableFlags;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;
namespace Riders.Tweakbox.Components.Editors.Layout;

public unsafe class LayoutEditor : ComponentBase, IComponent
{
    /// <inheritdoc />
    public override string Name { get; set; } = "Layout Editor";

    private const string _layoutExtension = ".layout";
    private SetObject* _currentObject;
    private ImVec2.__Internal _rowMinHeight = new ImVec2.__Internal() { x = 0, y = 0 };
    private int _currentIndex;
    private bool _freezePlayerToItem;
    private bool _freezeItemToPlayer;
    private bool _safeMode;
    private bool _autoRenderRegion = true;
    private MiscPatchController _patchController;
    private ObjectLayoutController _layoutController;
    private IO _io;
    private Logger _log = new Logger(LogCategory.Default);

    private bool _scrollTable;
    private NetplayController _netplayController = IoC.Get<NetplayController>();
    private int _currentAutosaveFrames = 0;
    private Autosaver _autoSaver;

    public LayoutEditor(MiscPatchController patchController, ObjectLayoutController layoutController, IO io)
    {
        _io = io;
        _patchController = patchController;
        _layoutController = layoutController;
        _autoSaver = new Autosaver(_io.AutosaveObjectLayoutFolder, TimeSpan.FromMinutes(5), TimeSpan.FromHours(1), TimeSpan.FromHours(24), $"*{_layoutExtension}");
    }

    public bool IsAvailable() => !_netplayController.IsConnected();

    /// <inheritdoc />
    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            var contentRegionWidth = ImGui.GetWindowContentRegionWidth();

            if (_layoutController.LoadedLayouts.Count > 0)
            {
                ImGui.TextWrapped($"Total Object Tasks: {_layoutController.CountTotalTasks():0000}");
                ImGui.TextWrapped($"Current Item: {_currentIndex:000}/{_layoutController.CountTotalObjects():000}");
                RenderInternal(contentRegionWidth);
            }
            else
            {
                _currentIndex = 0;
                _currentObject = null;
            }

            ImGui.End();
        }
    }

    private void RenderInternal(float contentRegionWidth)
    {
        ImGui.BeginGroup();

        // Display a few columns:
        // Name, Type, Continent, Ping
        const int tableWidth = 200;
        float remainingWidth = (contentRegionWidth - tableWidth);

        var serverTableSize = new ImVec2.__Internal() { x = tableWidth, y = -40 };
        const int tableFlags = (int)(ImGuiTableFlagsRowBg | ImGuiTableFlagsBorders | ImGuiTableFlagsNoBordersInBody | ImGuiTableFlagsScrollY | ImGuiTableFlagsContextMenuInBody);

        if (ImGui.__Internal.BeginTable("item_table", 1, tableFlags, serverTableSize, 0))
        {
            if (_scrollTable)
            {
                ImGui.SetScrollY_Float(ImGui.GetTextLineHeightWithSpacing() * _currentIndex);
                _scrollTable = false;
            }

            // Create Headers
            // TODO: Re-add Type when they become relevant.
            ImGui.TableSetupColumn("Name", 0, 0, 0);
            ImGui.TableSetupScrollFreeze(1, 1);

            // Show Headers
            ImGui.TableHeadersRow();

            // Render items
            int totalIndex = 0;
            foreach (var loaded in _layoutController.LoadedLayouts)
            {
                for (int x = 0; x < loaded.LayoutFile.Header->ObjectCount; x++)
                {
                    // Setup
                    var item = &loaded.LayoutFile.Objects.Pointer[x];
                    bool isSelected = item == _currentObject;

                    ImGui.PushID_Int(totalIndex);
                    ImGui.TableNextRow(0, 0);
                    int columnIndex = 0;

                    // Name (Selectable)
                    ImGui.TableSetColumnIndex(columnIndex++);
                    if (ImGui.__Internal.SelectableBool($"{Enums.AsStringUnsafe(item->Type)} ({(int)item->Type})", isSelected, (int)0, _rowMinHeight))
                    {
                        _currentObject = item;
                        _currentIndex = totalIndex;
                    }

                    // Cleanup
                    ImGui.PopID();
                    totalIndex++;
                }
            }

            ImGui.EndTable();
        }

        // Autosave
        if (_autoSaver.Update(out bool compressionPerformed))
        {
            if (compressionPerformed)
                _log.WriteLine($"[Layout Editor] Old Autosaves Compressed");
            
            var fileName = GetAutosaveFileName();
            var filePath = Path.Combine(_io.AutosaveObjectLayoutFolder, fileName);
            _log.WriteLine($"Autosaving: {filePath}");

            var data = _layoutController.Export();
            File.WriteAllBytesAsync(filePath, data);
        }

        // Debug Information
        var tablePtr = *State.CurrentStageObjectLayout;
        ImGui.TextWrapped($"Layout Data Address: {(int)tablePtr:X}");

        // Render Current Item
        if (_currentObject == (void*)0)
        {
            ImGui.EndGroup();
            return;
        }

        // Render Task Address
        if (_layoutController.TryFindObjectTask(_currentObject, out var task))
        {
            ImGui.TextWrapped($"Item Task Address: {(int)task.Pointer:X}");
        }

        ImGui.EndGroup();
        RenderObject(_currentObject, remainingWidth);
    }

    private void RenderObject(SetObject* item, float availableWidth)
    {
        const float spacing = 20;
        const float renderRegionOffset = -240;
        ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

        ImGui.SameLine(0, spacing);
        ImGui.BeginGroup();

        ImGui.TextWrapped("Until stage restart, changes are visual only. Collision etc. may not be updated.");

        availableWidth -= spacing;
        Reflection.MakeControlEnum(&item->Type, "Object Id");
        Reflection.MakeControl((ushort*)&item->Type, "Object Id (Manual)");
        Tooltip.TextOnHover("In case your object isn't in the list <3");

        Reflection.MakeControl(&item->MaxPlayerCount, "Max Player Count");
        Tooltip.TextOnHover("The maximum player count until the object is no longer included.\n" +
                            "e.g. 1 = 1 Player Only\n" +
                            "2 = 1 & 2 Player Only");

        ImGui.PushItemWidth(renderRegionOffset);
        if (Reflection.MakeControl(&item->PortalChar, "Map Portal (Render Region)"))
            _layoutController.SetPortalChar(item, item->PortalChar);

        Tooltip.TextOnHover("The ASCII character used to denote the \"portal\" the object belongs to.\n" +
                            "Portals are bounding box regions. If the object is outside the portal, it is not rendered.");

        ImGui.SameLine(0, Constants.Spacing);
        ImGui.Checkbox("Auto", ref _autoRenderRegion);
        if (_autoRenderRegion)
            CopyNearestRenderRegion();

        ImGui.PopItemWidth();

        ImGui.Text("Availability");
        Reflection.MakeControlEnum(&item->Visibility, "Visibility", 100, availableWidth);

        if (Reflection.MakeControl(&item->Attribute, "Attribute"))
            _layoutController.SetAttribute(item, (ushort)item->Attribute);

        Tooltip.TextOnHover("What this does depends on the type of object. For item boxes, this is the item inside.");

        if (Reflection.MakeControl(&item->Position, "Position") && !_safeMode)
                _layoutController.MoveObject(item, item->Position);
        
        if (Reflection.MakeControl(&item->Rotation, "Rotation") && !_safeMode)
            _layoutController.RotateObject(item, item->Rotation);

        Reflection.MakeControl(&item->Scale, "Scale");

        ImGui.Text("Tools");

        // Row 1
        if (ImGui.Button("Clone Object", Constants.Zero))
        {
            _currentObject = _layoutController.LoadNewObject(item);
            _currentIndex = _layoutController.CountTotalObjects() - 1;
            _scrollTable = true;
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Invalidate (Delete) Item", Constants.Zero))
            _layoutController.InvalidateItem(item);

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Copy Portal (Render Region) of Nearest Item", Constants.Zero))
            CopyNearestRenderRegion();

        // Row 2
        if (ImGui.Button("Teleport to Item", Constants.Zero))
            TeleportPlayer(item->Position, item->Rotation.DegreesToRadians());

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Teleport to Nearest Item", Constants.Zero))
        {
            var nearest = _layoutController.FindNearestItem(item, Player.Players[0].Position, false, out float distance, out var nearestIndex);
            if (nearest != (void*)0)
            {
                _currentObject = nearest;
                _currentIndex = nearestIndex;
                _scrollTable = true;
                TeleportPlayer(_currentObject->Position, _currentObject->Rotation.DegreesToRadians());
            }
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Teleport to Nearest Active Item", Constants.Zero))
        {
            var nearest = _layoutController.FindNearestItem(item, Player.Players[0].Position, true, out float distance, out var nearestIndex);
            if (nearest != (void*)0)
            {
                _currentObject = nearest;
                _currentIndex = nearestIndex;
                _scrollTable = true;
                TeleportPlayer(_currentObject->Position, _currentObject->Rotation.DegreesToRadians());
            }
        }

        // Row 3
        if (ImGui.Button("Teleport Item to Player", Constants.Zero))
        {
            item->Position = Player.Players[0].Position;
            _layoutController.MoveObject(item, item->Position);
        }

        ImGui.SameLine(0, Constants.Spacing);
        ImGui.Checkbox("Player Follow Item", ref _freezePlayerToItem);
        if (_freezePlayerToItem)
            TeleportPlayer(item->Position, item->Rotation.DegreesToRadians());

        ImGui.SameLine(0, Constants.Spacing);
        ImGui.Checkbox("Item Follow Player", ref _freezeItemToPlayer);
        if (_freezeItemToPlayer)
        {
            item->Position = Player.Players[0].Position;
            item->Rotation = Player.Players[0].Rotation.RadiansToDegrees();
            if (!_safeMode)
            {
                _layoutController.MoveObject(item, item->Position);
                _layoutController.RotateObject(item, item->Rotation);
            }
        }

        if (ImGui.Button("Export to File", Constants.Zero))
        {
            bool bigEndian = false;
            Shell.AddDialog("Export Object Layout", (ref bool opened) =>
            {
                ImGui.Checkbox("Big Endian (GameCube)", ref bigEndian);
                if (ImGui.Button("Save", Constants.Zero))
                {
                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Title = "Save to Path";
                    saveFileDialog.FileName = "00000.bin";
                    var result = saveFileDialog.ShowDialog();
                    if (result == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                    {
                        var data = _layoutController.Export(bigEndian);
                        File.WriteAllBytes(saveFileDialog.FileName, data);
                    }

                    opened = false;
                }
            });
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Import from File & Restart", Constants.Zero))
        {
            var saveFileDialog = new OpenFileDialog();
            saveFileDialog.Title = "Save to Path";
            saveFileDialog.FileName = "00000.bin";
            var result = saveFileDialog.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                _layoutController.ImportAndRestart(File.ReadAllBytes(saveFileDialog.FileName));
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Fast Restart", Constants.Zero))
            _layoutController.FastRestart();

        ImGui.SameLine(0, Constants.Spacing);
        ImGui.Checkbox("Safe Mode", ref _safeMode);
        Tooltip.TextOnHover("Prevents from writing to object data when moving objects.\n" +
                            "Use this to avoid potential crashes, at expense of no real time render position update.");

        ImGui.EndGroup();
        ImGui.PopItemWidth();

        void CopyNearestRenderRegion()
        {
            var nearest = _layoutController.FindNearestItem(item, item->Position, false, out float distance, out _);
            if (nearest != (void*)0)
            {
                item->PortalChar = nearest->PortalChar;
                _layoutController.SetPortalChar(item, item->PortalChar);
            }
        }
    }

    private void TeleportPlayer(Vector3 position, Vector3 rotation)
    {
        ref var player = ref Player.Players[0];
        player.Position = position;
        player.PositionAlt = position;
        player.Rotation = rotation;
        player.Speed = 0;
        player.VSpeed = 0;
        player.Acceleration = 0;
        player.FallingMode = FallingMode.Ground;
        player.LastMovementFlags = 0;
        player.MovementFlags = 0;
    }

    private string GetAutosaveFileName()
    {
        var stage = *State.Level;
        var stageName = stage.GetName();
        stageName ??= "Unknown";

        var currentTime = DateTime.UtcNow.ToString(@"yyyy.MM.dd.HH.mm.ss");
        return $"{stageName}_{currentTime}{_layoutExtension}";
    }
}
