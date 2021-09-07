using System;
using System.IO;
using System.Runtime.CompilerServices;
using DearImguiSharp;
using EnumsNET;
using Reloaded.Memory;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using Player = Sewer56.SonicRiders.API.Player;
using Reflection = Sewer56.Imgui.Controls.Reflection;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;
using static Sewer56.Imgui.Misc.Constants;
using Sewer56.Imgui.Controls;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Components.Editors.Gear;

/// <summary>
/// Provides the capability of editing gears for the mod.
/// </summary>
public unsafe class GearEditor : ComponentBase<GearEditorConfig>, IComponent
{
    public override string Name { get; set; } = "Gear Editor";
    private NetplayController _netplayController = IoC.Get<NetplayController>();
    private CustomGearController _customGearController = IoC.GetSingleton<CustomGearController>();
    private CustomGearService _customGearService = IoC.GetSingleton<CustomGearService>();
    private CustomGearDataInternal _customGearData = new CustomGearDataInternal();
    private Logger _log = new Logger(LogCategory.Default);

    public GearEditor(IO io) : base(io, io.GearConfigFolder, io.GetGearConfigFiles)
    {

    }

    public bool IsAvailable() => !_netplayController.IsConnected();

    /// <inheritdoc />
    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            ProfileSelector.Render();
            EditGears();
        }

        ImGui.End();
    }

    /* Gear Editor */
    private void EditGears()
    {
        ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

        for (int x = 0; x < (int)Player.NumberOfGears; x++)
        {
            var headerName = _customGearController.GetGearName(x, out bool isCustom);

            if (ImGui.CollapsingHeaderTreeNodeFlags(headerName, 0))
            {
                ImGui.PushID_Int(x);
                bool hasCustomData = _customGearController.TryGetGearData(headerName, out _customGearData);
                var customData = hasCustomData ? _customGearData : null;
                EditGear(x, customData);
                ImGui.PopID();
            }
        }

        ImGui.PopItemWidth();
    }

    private void EditGear(int gearIndex, CustomGearDataInternal data)
    {
        var gear = (ExtremeGear*)Player.Gears.GetPointerToElement(gearIndex);
        if (data != null)
            ImGui.TextWrapped("This is a custom gear. Please note that any changes made here will be discarded should the custom gear data be enabled. e.g. Exiting/Entering Netplay. Please `Update Custom Gear` (if available) in Export menu for the data to persist.");

        if (ImGui.TreeNodeStr("Gear Flags"))
        {
            ImGui.Spacing();
            ImGui.TextWrapped("Type & Model:");
            Reflection.MakeControlEnum(&gear->GearType, nameof(ExtremeGear.GearType));
            Reflection.MakeControlEnum(&gear->GearModel, nameof(ExtremeGear.GearModel));

            ImGui.Spacing();
            ImGui.TextWrapped("Who Can Select:");
            Reflection.MakeControlEnum(&gear->WhoCanSelect, nameof(ExtremeGear.WhoCanSelect), 110);

            ImGui.Spacing();
            ImGui.TextWrapped("Special Flags:");
            Reflection.MakeControlEnum(&gear->SpecialFlags, nameof(ExtremeGear.SpecialFlags), 180);

            ImGui.Spacing();
            ImGui.TextWrapped("Extra Types:");
            Reflection.MakeControlEnum(&gear->ExtraTypes, nameof(ExtremeGear.ExtraTypes));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Speed & Acceleration"))
        {
            // Estimate cruise speeds.
            for (int x = 0; x <= (int)FormationTypes.Power; x++)
            {
                var estimateSpeed = Formula.GetGearSpeed(gear, FormationTypes.Speed, x, out float rawSpeed);
                ImGui.Text($"Estimate Speed Lv{x}: {estimateSpeed} (Speed Type)");
            }
            ImGui.Separator();

            Reflection.MakeControl(&gear->AdditiveSpeed, nameof(ExtremeGear.AdditiveSpeed), 0.025f, $"%f ({Formula.SpeedToSpeedometer(gear->AdditiveSpeed)})");
            Reflection.MakeControl(&gear->Acceleration, nameof(ExtremeGear.Acceleration), 0.05f, $"%f ({Formula.SpeedToSpeedometer(gear->Acceleration)})");
            Reflection.MakeControl(&gear->OffroadSpeed, nameof(ExtremeGear.OffroadSpeed));
            Reflection.MakeControl(&gear->TurnLowSpeedMultiplier, nameof(ExtremeGear.TurnLowSpeedMultiplier));
            Reflection.MakeControl(&gear->TurnAcceleration, nameof(ExtremeGear.TurnAcceleration), 0.05f, $"%f ({Formula.SpeedToSpeedometer(gear->TurnAcceleration)})");
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Turning & Drifting"))
        {
            Reflection.MakeControl(&gear->TurnMaxRadius, nameof(ExtremeGear.TurnMaxRadius));
            Reflection.MakeControl(&gear->DriftMaximumTurnRadius, nameof(ExtremeGear.DriftMaximumTurnRadius));
            Reflection.MakeControl(&gear->DriftMomentum, nameof(ExtremeGear.DriftMomentum));
            Reflection.MakeControl(&gear->DriftMinimumRadius, nameof(ExtremeGear.DriftMinimumRadius));
            Reflection.MakeControl(&gear->DriftAcceleration, nameof(ExtremeGear.DriftAcceleration));
            Reflection.MakeControl(&gear->DriftBoostFramesOffset, nameof(ExtremeGear.DriftBoostFramesOffset));
            Reflection.MakeControl(&gear->Weight, nameof(ExtremeGear.Weight));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Air Multipliers"))
        {
            float ToPercent(float multiplier) => (1 + multiplier) * 100f;

            Reflection.MakeControl(&gear->AirGainTrickMultiplier, nameof(ExtremeGear.AirGainTrickMultiplier), 0.01f, $"%f ({ToPercent(gear->AirGainTrickMultiplier)}%%)");
            Reflection.MakeControl(&gear->AirGainShortcutMultiplier, nameof(ExtremeGear.AirGainShortcutMultiplier), 0.01f, $"%f ({ToPercent(gear->AirGainShortcutMultiplier)}%%)");
            Reflection.MakeControl(&gear->AirGainAutorotateMultiplier, nameof(ExtremeGear.AirGainAutorotateMultiplier), 0.01f, $"%f ({ToPercent(gear->AirGainAutorotateMultiplier)}%%)");
            Reflection.MakeControl(&gear->JumpAirMultiplier, nameof(ExtremeGear.JumpAirMultiplier), 1f, $"%f ({gear->JumpAirMultiplier * 100}%%)");
            ImGui.TreePop();
        }

        EditGearLevelStats(&gear->GearStatsLevel1, 1);
        EditGearLevelStats(&gear->GearStatsLevel2, 2);
        EditGearLevelStats(&gear->GearStatsLevel3, 3);

        if (ImGui.TreeNodeStr("Main Menu Stats"))
        {
            Reflection.MakeControl(&gear->StatDashOffset, nameof(ExtremeGear.StatDashOffset));
            Reflection.MakeControl(&gear->StatLimitOffset, nameof(ExtremeGear.StatLimitOffset));
            Reflection.MakeControl(&gear->StatPowerOffset, nameof(ExtremeGear.StatPowerOffset));
            Reflection.MakeControl(&gear->StatCorneringOffset, nameof(ExtremeGear.StatCorneringOffset));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Exhaust Trail Settings"))
        {
            Reflection.MakeControl(&gear->ExhaustTrail1Width, nameof(ExtremeGear.ExhaustTrail1Width));
            Reflection.MakeControl(&gear->ExhaustTrail2Width, nameof(ExtremeGear.ExhaustTrail2Width));

            Reflection.MakeControl(&gear->ExhaustTrail1PositionOffset, nameof(ExtremeGear.ExhaustTrail1PositionOffset));
            Reflection.MakeControl(&gear->ExhaustTrail2PositionOffset, nameof(ExtremeGear.ExhaustTrail2PositionOffset));

            Reflection.MakeControl(&gear->ExhaustTrail1TrickWidth, nameof(ExtremeGear.ExhaustTrail1TrickWidth));
            Reflection.MakeControl(&gear->ExhaustTrail2TrickWidth, nameof(ExtremeGear.ExhaustTrail2TrickWidth));

            Reflection.MakeControl(&gear->ExhaustTrail1TrickOffset, nameof(ExtremeGear.ExhaustTrail1TrickOffset));
            Reflection.MakeControl(&gear->ExhaustTrail2TrickOffset, nameof(ExtremeGear.ExhaustTrail2TrickOffset));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Import/Export"))
        {
            // Custom Gear Stuff.
            if (data != null)
            {
                // Only if imported from file.
                if (!string.IsNullOrEmpty(data.GearDataLocation))
                {
                    if (ImGui.Button("Update Custom Gear Data", Zero))
                        _customGearService.UpdateRawGearData(gear, data);

                    Tooltip.TextOnHover("Updates the Custom Gear stats to the file stored on disk.");
                }
            }

            if (ImGui.Button("Export as Custom Gear", Zero))
            {
                if (data != null)
                    _customGearService.ExportToFolder(gear, data);
                else
                    _customGearService.ExportToFolder(gear, _customGearController.GetGearName(gearIndex, out _)); 
            }

            ImGui.TreePop();
        }
    }

    private void EditGearLevelStats(ExtremeGearLevelStats* stats, int level)
    {
        if (ImGui.TreeNodeStr($"Gear Stats Lv{level}"))
        {
            Reflection.MakeControl(&stats->MaxAir, nameof(ExtremeGearLevelStats.MaxAir));
            Reflection.MakeControl(&stats->PassiveAirDrain, nameof(ExtremeGearLevelStats.PassiveAirDrain));
            Reflection.MakeControl(&stats->DriftAirCost, nameof(ExtremeGearLevelStats.DriftAirCost));
            Reflection.MakeControl(&stats->BoostCost, nameof(ExtremeGearLevelStats.BoostCost));
            Reflection.MakeControl(&stats->TornadoCost, nameof(ExtremeGearLevelStats.TornadoCost));
            Reflection.MakeControl(&stats->SpeedGainedFromDriftDash, nameof(ExtremeGearLevelStats.SpeedGainedFromDriftDash), 0.01f, $"%f ({Formula.SpeedToSpeedometer(stats->SpeedGainedFromDriftDash)})");
            Reflection.MakeControl(&stats->BoostSpeed, nameof(ExtremeGearLevelStats.BoostSpeed), 0.01f, $"%f ({Formula.SpeedToSpeedometer(stats->BoostSpeed)})");
            ImGui.TreePop();
        }
    }
}
