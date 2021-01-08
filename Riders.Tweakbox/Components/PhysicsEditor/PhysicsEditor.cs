using System;
using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Components.PhysicsEditor
{
    public unsafe class PhysicsEditor : IComponent
    {
        public string Name { get; set; } = "Physics Editor";
        public PhysicsEditorConfig CurrentConfig { get; private set; } = PhysicsEditorConfig.FromGame();

        private bool _isEnabled;
        private IO _io;
        private ProfileSelector _profileSelector;

        public PhysicsEditor(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.PhysicsConfigFolder, IO.ConfigExtension, CurrentConfig.GetDefault().ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();
        }

        private byte[] GetCurrentConfigBytes() => CurrentConfig.GetCurrent().ToBytes();
        private string[] GetConfigFiles() => _io.GetPhysicsConfigFiles();

        private void LoadConfig(byte[] data)
        {
            var fileSpan = new Span<byte>(data);
            CurrentConfig.FromBytes(fileSpan);
            CurrentConfig.Apply();
        }

        public ref bool IsEnabled() => ref _isEnabled;
        public bool IsAvailable() => !IoC.Get<NetplayController>().IsConnected();

        public void Disable() => CurrentConfig.GetDefault().Apply();
        public void Enable() => CurrentConfig?.Apply();

        public void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                _profileSelector.Render();
                EditRunningPhysics();
            }

            ImGui.End();
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
