using System;
using DearImguiSharp;
using EnumsNET;
using Riders.Tweakbox.Definitions;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Gameplay;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using ExtremeGearEnum = Sewer56.SonicRiders.Structures.Enums.ExtremeGear;

namespace Riders.Tweakbox.Components.GearEditor
{
    /// <summary>
    /// Provides the capability of editing gears for the mod.
    /// </summary>
    public unsafe class GearEditor : IComponent
    {
        public string Name { get; set; } = "Gear Editor";
        public GearEditorConfig CurrentConfig { get; private set; } = GearEditorConfig.FromGame();

        private IO _io;
        private ProfileSelector _profileSelector;

        public GearEditor(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.GearConfigFolder, IO.CompressLZ4(CurrentConfig.ToBytes()), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();
        }

        // Profile Selector Implementation
        private void LoadConfig(byte[] data)
        {
            var decompressed = IO.DecompressLZ4(data);
            var fileSpan = new Span<byte>(decompressed);
            CurrentConfig.FromBytes(fileSpan);
            CurrentConfig.Apply();
        }

        private string[] GetConfigFiles() => _io.GetGearConfigFiles();
        private byte[] GetCurrentConfigBytes() => IO.CompressLZ4(GearEditorConfig.FromGame().ToBytes());

        public void Disable() => CurrentConfig.GetDefault().Apply();
        public void Enable() => CurrentConfig?.Apply();

        /// <param name="compEnabled"></param>
        /// <inheritdoc />
        public void Render(ref bool compEnabled)
        {
            if (!compEnabled)
                return;

            if (ImGui.Begin(Name, ref compEnabled, 0))
            {
                _profileSelector.Render();
                EditGears();
                ImGui.End();
            }
        }

        /* Gear Editor */
        private void EditGears()
        {
            var gears = ExtremeGears.ExtremeGear;
            for (int x = 0; x <= (int)ExtremeGearEnum.Cannonball; x++)
            {
                var headerName = ((ExtremeGearEnum)x).GetName();
                if (ImGui.CollapsingHeaderTreeNodeFlags(headerName, 0))
                    EditGear(&gears[x]);
            }
        }

        private void EditGear(ExtremeGear* gear)
        {
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
                Reflection.MakeControl(&gear->Acceleration, nameof(ExtremeGear.Acceleration));
                Reflection.MakeControl(&gear->SpeedHandlingMultiplier, nameof(ExtremeGear.SpeedHandlingMultiplier));
                Reflection.MakeControl(&gear->Field_14, nameof(ExtremeGear.Field_14));
                Reflection.MakeControl(&gear->TurnLowSpeedMultiplier, nameof(ExtremeGear.TurnLowSpeedMultiplier));
                Reflection.MakeControl(&gear->TurnAcceleration, nameof(ExtremeGear.TurnAcceleration));
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
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Air Multipliers"))
            {
                Reflection.MakeControl(&gear->AirGainTrickMultiplier, nameof(ExtremeGear.AirGainTrickMultiplier));
                Reflection.MakeControl(&gear->AirGainShortcutMultiplier, nameof(ExtremeGear.AirGainShortcutMultiplier));
                Reflection.MakeControl(&gear->AirGainAutorotateMultiplier, nameof(ExtremeGear.AirGainAutorotateMultiplier));
                Reflection.MakeControl(&gear->JumpAirMultiplier, nameof(ExtremeGear.JumpAirMultiplier));
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
                Reflection.MakeControl(&gear->ExhaustTrail1Width, nameof(ExtremeGear.ExhaustTrail1Width));

                Reflection.MakeControl(&gear->ExhaustTrail1PositionOffset, nameof(ExtremeGear.ExhaustTrail1PositionOffset));
                Reflection.MakeControl(&gear->ExhaustTrail2PositionOffset, nameof(ExtremeGear.ExhaustTrail2PositionOffset));

                Reflection.MakeControl(&gear->ExhaustTrail1TrickWidth, nameof(ExtremeGear.ExhaustTrail1TrickWidth));
                Reflection.MakeControl(&gear->ExhaustTrail2TrickWidth, nameof(ExtremeGear.ExhaustTrail2TrickWidth));

                Reflection.MakeControl(&gear->ExhaustTrail1TrickOffset, nameof(ExtremeGear.ExhaustTrail1TrickOffset));
                Reflection.MakeControl(&gear->ExhaustTrail2TrickOffset, nameof(ExtremeGear.ExhaustTrail2TrickOffset));
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
                Reflection.MakeControl(&stats->SpeedGainedFromDriftDash, nameof(ExtremeGearLevelStats.SpeedGainedFromDriftDash));
                Reflection.MakeControl(&stats->BoostSpeed, nameof(ExtremeGearLevelStats.BoostSpeed));
                ImGui.TreePop();
            }
        }
    }
}
