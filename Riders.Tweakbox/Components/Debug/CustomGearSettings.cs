﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using DotNext.Buffers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;

namespace Riders.Tweakbox.Components.Debug;

public class CustomGearSettings : ComponentBase, IComponent
{
    public override string Name { get; set; } = "Custom Gear Settings";
    private CustomGearController _customGearController;
    private string[] _loadedGearNameBuffer = new string[0];
    private string[] _unloadedGearNameBuffer = new string[0];
    private CustomGearDataInternal _gearData = new CustomGearDataInternal();
    private Logger _log = new Logger();
    private NetplayController _netplayController = IoC.GetSingleton<NetplayController>();

    /// <inheritdoc />
    public bool IsAvailable() => !_netplayController.IsActive();

    public CustomGearSettings(CustomGearController customGearController)
    {
        _customGearController = customGearController;
    }

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            RenderMenu();
        }

        ImGui.End();
    }

    private unsafe void RenderMenu()
    {
        _customGearController.GetCustomGearCount(out int loadedGears, out int unloadedGears);

        if (ImGui.CollapsingHeaderTreeNodeFlags("Loaded Gears", 0))
        {
            using var loadedRental = new ArrayRental<string>(loadedGears);
            var span = loadedRental.Span;
            _customGearController.GetCustomGearNames(span, null);

            foreach (var item in span)
                RenderCustomGear(item);
        }

        if (ImGui.CollapsingHeaderTreeNodeFlags("Unloaded Gears", 0))
        {
            using var unloadedRental = new ArrayRental<string>(unloadedGears);
            var span = unloadedRental.Span;
            _customGearController.GetCustomGearNames(null, span);

            foreach (var item in span)
                RenderCustomGear(item);
        }
    }

    private void RenderCustomGear(string item)
    {
        if (_customGearController.TryGetGearData(item, out _gearData))
        {
            if (ImGui.TreeNodeStr(_gearData.GearName))
            {
                if (!string.IsNullOrEmpty(_gearData.AnimatedIconFolder))
                    ImGui.TextWrapped($"Animated Icon Folder: {_gearData.AnimatedIconFolder}");

                if (!string.IsNullOrEmpty(_gearData.AnimatedNameFolder))
                    ImGui.TextWrapped($"Animated Name Folder: {_gearData.AnimatedNameFolder}");

                if (!string.IsNullOrEmpty(_gearData.GearDataLocation))
                    ImGui.TextWrapped($"Gear Data Location: {_gearData.GearDataLocation}");

                if (!string.IsNullOrEmpty(_gearData.IconPath))
                    ImGui.TextWrapped($"Icon Path: {_gearData.IconPath}");

                if (!string.IsNullOrEmpty(_gearData.NamePath))
                    ImGui.TextWrapped($"Name Path: {_gearData.NamePath}");

                if (_gearData.IsGearLoaded)
                {
                    if (ImGui.Button("Try Unload Gear", Constants.Zero))
                    {
                        if (!_customGearController.UnloadGear(_gearData.GearName))
                            _log.WriteLine($"[{nameof(CustomGearSettings)}] Failed to Unload Gear");
                    }

#if DEBUG
                        if (ImGui.Button("Try Remove Gear", Constants.Zero))
                        {
                            if (!_customGearController.RemoveGear(_gearData.GearName, true))
                                _log.WriteLine($"[{nameof(CustomGearSettings)}] Failed to Remove Gear");
                        }
                        Tooltip.TextOnHover("Permanently removes gear until added again in code or via restart.");
#endif
                }
                else
                {
                    if (ImGui.Button("Try Load Gear", Constants.Zero))
                    {
                        var result = _customGearController.AddGear(Mapping.Mapper.Map<AddGearRequest>(_gearData));
                        if (result == null)
                            _log.WriteLine($"[{nameof(CustomGearSettings)}] Failed to Load Gear");
                    }
                }

                ImGui.TreePop();
            }
        }
    }
}
