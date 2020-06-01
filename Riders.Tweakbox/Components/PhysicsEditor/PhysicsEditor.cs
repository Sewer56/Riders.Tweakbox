using System;
using System.Collections.Generic;
using System.Text;
using DearImguiSharp;
using Reloaded.Memory.Kernel32;
using Reloaded.Memory.Sources;
using Riders.Tweakbox.Definitions;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Components.PhysicsEditor
{
    public unsafe class PhysicsEditor : IComponent
    {
        public string Name { get; set; } = "Physics Editor";
        public PhysicsEditorConfig CurrentConfig { get; private set; } = PhysicsEditorConfig.FromGame();

        private IO _io;
        private ProfileSelector _profileSelector;

        public PhysicsEditor(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.PhysicsConfigFolder, CurrentConfig.ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();

            // Unprotect Modified Memory.
            Memory.CurrentProcess.ChangePermission((IntPtr) Physics.RunningPhysics1, sizeof(RunningPhysics), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
        }

        private byte[] GetCurrentConfigBytes() => PhysicsEditorConfig.FromGame().ToBytes();
        private string[] GetConfigFiles() => _io.GetPhysicsConfigFiles();

        private void LoadConfig(byte[] data)
        {
            var fileSpan = new Span<byte>(data);
            CurrentConfig.FromBytes(fileSpan);
            CurrentConfig.Apply();
        }

        public void Disable() => CurrentConfig.GetDefault().Apply();
        public void Enable() => CurrentConfig?.Apply();

        public void Render(ref bool compEnabled)
        {
            if (!compEnabled)
                return;

            if (ImGui.Begin(Name, ref compEnabled, 0))
            {
                _profileSelector.Render();
                EditRunningPhysics();
                ImGui.End();
            }
        }

        private void EditRunningPhysics()
        {
            if (ImGui.TreeNodeStr("Speed"))
            {
                ImGui.Spacing();
                Reflection.MakeControl(&Physics.RunningPhysics1->MinimumSpeed, nameof(RunningPhysics.MinimumSpeed));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearOneMaxSpeed, nameof(RunningPhysics2.GearOneMaxSpeed));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearTwoMaxSpeed, nameof(RunningPhysics2.GearTwoMaxSpeed));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearThreeMaxSpeed, nameof(RunningPhysics2.GearThreeMaxSpeed));

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Acceleration"))
            {
                ImGui.Spacing();
                Reflection.MakeControl(&Physics.RunningPhysics1->BackwardsWalkAccel, nameof(RunningPhysics.BackwardsWalkAccel));
                Reflection.MakeControl(&Physics.RunningPhysics1->SlidingBreakAccel, nameof(RunningPhysics.SlidingBreakAccel));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearOneAcceleration, nameof(RunningPhysics2.GearOneAcceleration));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearTwoAcceleration, nameof(RunningPhysics2.GearTwoAcceleration));
                Reflection.MakeControl(&Physics.RunningPhysics2->GearThreeAcceleration, nameof(RunningPhysics2.GearThreeAcceleration));

                ImGui.TreePop();
            }

            // Misc
            if (ImGui.TreeNodeStr("Miscellaneous"))
            {
                ImGui.Spacing();
                Reflection.MakeControl(&Physics.RunningPhysics1->Inertia, nameof(RunningPhysics.Inertia));

                ImGui.TreePop();
            }

            // Unknown Struct Components
            if (ImGui.TreeNodeStr("Unknown"))
            {
                ImGui.Spacing();
                if (ImGui.TreeNodeStr("Struct A"))
                {
                    Reflection.MakeControl(&Physics.RunningPhysics1->Field_00, nameof(RunningPhysics.Field_00));
                    Reflection.MakeControl(&Physics.RunningPhysics1->Field_10, nameof(RunningPhysics.Field_10));
                    Reflection.MakeControl(&Physics.RunningPhysics1->Field_18, nameof(RunningPhysics.Field_18));
                    Reflection.MakeControl(&Physics.RunningPhysics1->Field_1C, nameof(RunningPhysics.Field_1C));

                    ImGui.TreePop();
                }

                ImGui.Spacing();
                if (ImGui.TreeNodeStr("Struct B"))
                {
                    Reflection.MakeControl(&Physics.RunningPhysics2->Field_18, nameof(RunningPhysics2.Field_18));
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }
        }
    }
}
