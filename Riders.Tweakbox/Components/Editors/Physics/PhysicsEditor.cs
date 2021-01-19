using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

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
                if (ImGui.CollapsingHeaderTreeNodeFlags("Running Physics", 0))
                    EditRunningPhysics();
            }

            ImGui.End();
        }

        private void EditRunningPhysics()
        {
            ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

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

            ImGui.PopItemWidth();
        }
    }
}
