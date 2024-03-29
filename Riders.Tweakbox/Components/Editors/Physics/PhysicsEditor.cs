﻿using System;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.API.Player;
using Reflection = Sewer56.Imgui.Controls.Reflection;
using Riders.Tweakbox.Interfaces.Internal;

namespace Riders.Tweakbox.Components.Editors.Physics;

public unsafe class PhysicsEditor : ComponentBase<PhysicsEditorConfig>, IComponent
{
    public override string Name { get; set; } = "Physics Editor";
    private NetplayController _netplayController = IoC.Get<NetplayController>();

    public PhysicsEditor(IO io) : base(io, io.PhysicsConfigFolder, io.GetPhysicsConfigFiles)
    {

    }

    public bool IsAvailable() => !_netplayController.IsConnected();

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            int id = 0;
            ProfileSelector.Render();
            ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

            if (ImGui.CollapsingHeaderTreeNodeFlags("Running Physics", 0))
                EditRunningPhysics();

            if (ImGui.CollapsingHeaderTreeNodeFlags("Character Type Stats", 0))
            {
                EditTypeStatsType(FormationTypes.Speed);
                EditTypeStatsType(FormationTypes.Fly);
                EditTypeStatsType(FormationTypes.Power);
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Turbulence Properties", 0))
            {
                if (ImGui.TreeNodeStr("Regular Turbulence"))
                {
                    ImGui.PushID_Int(id++);
                    EditTurbulenceType(TurbulenceType.NoTrick, 0);
                    EditTurbulenceType(TurbulenceType.TrickOne, 0);
                    EditTurbulenceType(TurbulenceType.TrickTwo, 0);
                    EditTurbulenceType(TurbulenceType.TrickThree, 0);
                    EditTurbulenceType(TurbulenceType.TrickRainbowTopPath, 0);
                    ImGui.TreePop();
                    ImGui.PopID();
                }

                if (ImGui.TreeNodeStr("Babylon Garden Turbulence"))
                {
                    ImGui.PushID_Int(id++);
                    var offset = (int)TurbulenceType.TrickRainbowTopPath + 1;
                    EditTurbulenceType(TurbulenceType.NoTrick, offset);
                    EditTurbulenceType(TurbulenceType.TrickOne, offset);
                    EditTurbulenceType(TurbulenceType.TrickTwo, offset);
                    EditTurbulenceType(TurbulenceType.TrickThree, offset);
                    EditTurbulenceType(TurbulenceType.TrickRainbowTopPath, offset);
                    ImGui.TreePop();
                    ImGui.PopID();
                }

                if (ImGui.TreeNodeStr("Sky Road Turbulence"))
                {
                    ImGui.PushID_Int(id++);
                    var offset = ((int)TurbulenceType.TrickRainbowTopPath + 1) * 2;
                    EditTurbulenceType(TurbulenceType.NoTrick, offset);
                    EditTurbulenceType(TurbulenceType.TrickOne, offset);
                    EditTurbulenceType(TurbulenceType.TrickTwo, offset);
                    EditTurbulenceType(TurbulenceType.TrickThree, offset);
                    EditTurbulenceType(TurbulenceType.TrickRainbowTopPath, offset);
                    ImGui.TreePop();
                    ImGui.PopID();
                }
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Dash Panel Behaviour", 0))
            {
                ImGui.PushID_Int(id++);
                ref var panelProps = ref Static.PanelProperties;
                Reflection.MakeControlEnum(ref panelProps.Mode, "Dash Panel Mode");
                switch (panelProps.Mode)
                {
                    case DashPanelMode.Fixed:
                        Reflection.MakeControl(ref panelProps.FixedSpeed, "Dash Panel Speed", 0.025f, $"%f ({Formula.SpeedToSpeedometer(panelProps.FixedSpeed)})");
                        break;
                    case DashPanelMode.Additive:
                        Reflection.MakeControl(ref panelProps.AdditiveSpeed, "Additive Speed", 0.025f, $"%f ({Formula.SpeedToSpeedometer(panelProps.AdditiveSpeed)})");
                        break;
                    case DashPanelMode.Multiplicative:
                        Reflection.MakeControl(ref panelProps.MultiplicativeSpeed, "Multiplicative Speed", 0.001f, $"%f ({panelProps.MultiplicativeSpeed * 100}%%)");
                        break;
                    case DashPanelMode.MultiplyOrFixed:
                        Reflection.MakeControl(ref panelProps.MultiplicativeSpeed, "Multiplicative Speed", 0.001f, $"%f ({panelProps.MultiplicativeSpeed * 100}%%)");
                        Reflection.MakeControl(ref panelProps.MultiplicativeMinSpeed, "Minimum Speed", 0.001f, $"%f ({Formula.SpeedToSpeedometer(panelProps.MultiplicativeMinSpeed)})");
                        break;

                    case DashPanelMode.Vanilla:
                    default: break;
                }

                ImGui.PopID();
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Speed Shoes Behaviour", 0))
            {
                ImGui.PushID_Int(id++);
                ref var shoeProps = ref Static.SpeedShoeProperties;
                Reflection.MakeControlEnum(ref shoeProps.Mode, "Speed Shoes Mode");
                switch (shoeProps.Mode)
                {
                    case SpeedShoesMode.Fixed:
                        Reflection.MakeControl(ref shoeProps.FixedSpeed, "Speed Shoe Speed", 0.025f, $"%f ({Formula.SpeedToSpeedometer(shoeProps.FixedSpeed)})");
                        break;
                    case SpeedShoesMode.Additive:
                        Reflection.MakeControl(ref shoeProps.AdditiveSpeed, "Additive Speed", 0.025f, $"%f ({Formula.SpeedToSpeedometer(shoeProps.AdditiveSpeed)})");
                        break;
                    case SpeedShoesMode.Multiplicative:
                        Reflection.MakeControl(ref shoeProps.MultiplicativeSpeed, "Multiplicative Speed", 0.001f, $"%f ({shoeProps.MultiplicativeSpeed * 100}%%)");
                        break;
                    case SpeedShoesMode.MultiplyOrFixed:
                        Reflection.MakeControl(ref shoeProps.MultiplicativeSpeed, "Multiplicative Speed", 0.001f, $"%f ({shoeProps.MultiplicativeSpeed * 100}%%)");
                        Reflection.MakeControl(ref shoeProps.MultiplicativeMinSpeed, "Minimum Speed", 0.001f, $"%f ({Formula.SpeedToSpeedometer(shoeProps.MultiplicativeMinSpeed)})");
                        break;

                    case SpeedShoesMode.Vanilla:
                    default: break;
                }
                ImGui.PopID();
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Deceleration Properties", 0))
            {
                ref var props = ref Static.DecelProperties;
                Reflection.MakeControlEnum(ref props.Mode, "Deceleration Mode");
                switch (props.Mode)
                {
                    case DecelMode.Default: break;
                    case DecelMode.Linear:
                        Reflection.MakeControl(ref props.LinearSpeedCapOverride, "Speed Cap Override", 0.001f, $"%f ({Formula.SpeedToSpeedometer(props.LinearSpeedCapOverride)})");
                        Reflection.MakeControl(ref props.EnableMaxSpeedOverCap, "Enable Max Speed Over Cap");
                        if (props.EnableMaxSpeedOverCap)
                            Reflection.MakeControl(ref props.LinearMaxSpeedOverCap, "Max Speed Over Cap", 0.001f, $"%f ({Formula.SpeedToSpeedometer(props.LinearMaxSpeedOverCap)})");

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ImGui.PopItemWidth();
        }

        ImGui.End();
    }

    private void EditTurbulenceType(TurbulenceType type, int startingIndex)
    {
        if (ImGui.TreeNodeStr(type.ToString()))
        {
            var turbulenceProperties = &Player.TurbulenceProperties.Pointer[(int)type + startingIndex];
            Reflection.MakeControl(&turbulenceProperties->MaxSpeed, "Max Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->MaxSpeed)})");
            Reflection.MakeControl(&turbulenceProperties->SpeedLossAboveMaxSpeed, "Speed Loss Above Max Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->SpeedLossAboveMaxSpeed)})");
            Reflection.MakeControl(&turbulenceProperties->MinSpeed, "Min Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->MinSpeed)})");
            Reflection.MakeControl(&turbulenceProperties->SpeedGainBelowMinSpeed, "Speed Gain Below Min Speed", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->SpeedGainBelowMinSpeed)})");

            Reflection.MakeControl(&turbulenceProperties->TrickSpeed, "Speed on Trick Land", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->TrickSpeed)})");
            Tooltip.TextOnHover($"This is the speed set when entering this turbulence type.\ne.g. for {TurbulenceType.TrickOne} this is the speed set when landing first trick.");

            Reflection.MakeControl(&turbulenceProperties->AccelOnCurve, "Speed on Curves", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->AccelOnCurve)})");
            Tooltip.TextOnHover("Amount of speed gained when going from the edge to the center of the turbulence. Noticeable on turns.");

            Reflection.MakeControl(&turbulenceProperties->ApparentlyMaxSpeed, "Max Speed (Unused)", 0.01f, $"%f ({Formula.SpeedToSpeedometer(turbulenceProperties->ApparentlyMaxSpeed)})");
            Tooltip.TextOnHover("Appears to be unused. This was the original Max Speed variable in earlier versions of the game.");
            ImGui.TreePop();
        }
    }

    private void EditTypeStatsType(FormationTypes type)
    {
        if (ImGui.TreeNodeStr(type.ToString()))
        {
            var typeStats = &Player.TypeStats.Pointer[(int)type];
            for (int x = 0; x < 3; x++)
            {
                var levelStats = CharacterTypeStats.GetLevelStats(typeStats, x);
                EditTypeStatsLevel(x, levelStats);
            }

            ImGui.TreePop();
        }
    }

    private void EditTypeStatsLevel(int level, CharacterTypeLevelStats* characterTypeStats)
    {
        if (ImGui.TreeNodeStr($"Level {level}"))
        {
            Reflection.MakeControl(&characterTypeStats->SpeedCap1, nameof(CharacterTypeLevelStats.SpeedCap1), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->SpeedCap1)})");
            Reflection.MakeControl(&characterTypeStats->SpeedCap2, nameof(CharacterTypeLevelStats.SpeedCap2), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->SpeedCap2)})");
            Reflection.MakeControl(&characterTypeStats->SpeedCap3, nameof(CharacterTypeLevelStats.SpeedCap3), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->SpeedCap3)})");
            Reflection.MakeControl(&characterTypeStats->AccelToSpeedCap1, nameof(CharacterTypeLevelStats.AccelToSpeedCap1), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->AccelToSpeedCap1)})");
            Reflection.MakeControl(&characterTypeStats->AccelToSpeedCap2, nameof(CharacterTypeLevelStats.AccelToSpeedCap2), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->AccelToSpeedCap2)})");
            Reflection.MakeControl(&characterTypeStats->AccelToSpeedCap3, nameof(CharacterTypeLevelStats.AccelToSpeedCap3), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->AccelToSpeedCap3)})");
            Reflection.MakeControl(&characterTypeStats->Offroad, nameof(CharacterTypeLevelStats.Offroad), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->Offroad)})");
            ImGui.TreePop();
        }
    }

    private void EditRunningPhysics()
    {
        if (ImGui.TreeNodeStr("Speed"))
        {
            ImGui.Spacing();
            Reflection.MakeControl(&Player.RunPhysics->MinimumSpeed, nameof(RunningPhysics.MinimumSpeed));
            Reflection.MakeControl(&Player.RunPhysics2->GearOneMaxSpeed, nameof(RunningPhysics2.GearOneMaxSpeed));
            Reflection.MakeControl(&Player.RunPhysics2->GearTwoMaxSpeed, nameof(RunningPhysics2.GearTwoMaxSpeed));
            Reflection.MakeControl(&Player.RunPhysics2->GearThreeMaxSpeed, nameof(RunningPhysics2.GearThreeMaxSpeed));

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Acceleration"))
        {
            ImGui.Spacing();
            Reflection.MakeControl(&Player.RunPhysics->BackwardsWalkAccel, nameof(RunningPhysics.BackwardsWalkAccel));
            Reflection.MakeControl(&Player.RunPhysics->SlidingBreakAccel, nameof(RunningPhysics.SlidingBreakAccel));
            Reflection.MakeControl(&Player.RunPhysics2->GearOneAcceleration, nameof(RunningPhysics2.GearOneAcceleration));
            Reflection.MakeControl(&Player.RunPhysics2->GearTwoAcceleration, nameof(RunningPhysics2.GearTwoAcceleration));
            Reflection.MakeControl(&Player.RunPhysics2->GearThreeAcceleration, nameof(RunningPhysics2.GearThreeAcceleration));

            ImGui.TreePop();
        }

        // Misc
        if (ImGui.TreeNodeStr("Miscellaneous"))
        {
            ImGui.Spacing();
            Reflection.MakeControl(&Player.RunPhysics->Inertia, nameof(RunningPhysics.Inertia));

            ImGui.TreePop();
        }

        // Unknown Struct Components
        if (ImGui.TreeNodeStr("Unknown"))
        {
            ImGui.Spacing();
            if (ImGui.TreeNodeStr("Struct A"))
            {
                Reflection.MakeControl(&Player.RunPhysics->Field_00, nameof(RunningPhysics.Field_00));
                Reflection.MakeControl(&Player.RunPhysics->Field_10, nameof(RunningPhysics.Field_10));
                Reflection.MakeControl(&Player.RunPhysics->Field_18, nameof(RunningPhysics.Field_18));
                Reflection.MakeControl(&Player.RunPhysics->Field_1C, nameof(RunningPhysics.Field_1C));

                ImGui.TreePop();
            }

            ImGui.Spacing();
            if (ImGui.TreeNodeStr("Struct B"))
            {
                Reflection.MakeControl(&Player.RunPhysics2->Field_18, nameof(RunningPhysics2.Field_18));
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }
    }
}
