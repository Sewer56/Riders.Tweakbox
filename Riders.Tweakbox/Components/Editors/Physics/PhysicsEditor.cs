using DearImguiSharp;
using EnumsNET;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.API.Player;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Editors.Physics
{
    public unsafe class PhysicsEditor : ComponentBase<PhysicsEditorConfig>, IComponent
    {
        public override string Name { get; set; } = "Physics Editor";

        public PhysicsEditor(IO io) : base(io, io.PhysicsConfigFolder, io.GetPhysicsConfigFiles)
        {
            
        }

        public bool IsAvailable() => !IoC.Get<NetplayController>().IsConnected();
        public override void Disable() => Config.GetDefault().Apply();
        public override void Enable() => Config?.Apply();

        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();
                ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

                if (ImGui.CollapsingHeaderTreeNodeFlags("Running Physics", 0))
                    EditRunningPhysics();

                if (ImGui.CollapsingHeaderTreeNodeFlags("Character Type Stats", 0))
                {
                    foreach (var type in Enums.GetValues<FormationTypes>())
                        EditTypeStatsType(type);
                }

                ImGui.PopItemWidth();
            }

            ImGui.End();
        }

        private void EditTypeStatsType(FormationTypes type)
        {
            if (ImGui.TreeNodeStr(type.ToString()))
            {
                var typeStats = &Player.TypeStats.Pointer[(int) type];
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
                Reflection.MakeControl(&characterTypeStats->AdditiveSpeed, nameof(CharacterTypeLevelStats.AdditiveSpeed), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->AdditiveSpeed)})");
                Reflection.MakeControl(&characterTypeStats->LowSpeedAccel, nameof(CharacterTypeLevelStats.LowSpeedAccel), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->LowSpeedAccel)})");
                Reflection.MakeControl(&characterTypeStats->HighSpeedAccel, nameof(CharacterTypeLevelStats.HighSpeedAccel), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->HighSpeedAccel)})");
                Reflection.MakeControl(&characterTypeStats->OffRoadCruisingResistance, nameof(CharacterTypeLevelStats.OffRoadCruisingResistance), 1.0f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->OffRoadCruisingResistance)})");
                Reflection.MakeControl(&characterTypeStats->Field_14, nameof(CharacterTypeLevelStats.Field_14), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->Field_14)})");
                Reflection.MakeControl(&characterTypeStats->Field_18, nameof(CharacterTypeLevelStats.Field_18), 0.05f, $"%f ({Formula.SpeedToSpeedometer(characterTypeStats->Field_18)})");

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
}
