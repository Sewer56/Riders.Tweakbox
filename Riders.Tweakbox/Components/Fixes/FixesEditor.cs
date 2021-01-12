using System;
using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Fixes
{
    public class FixesEditor : ComponentBase<FixesEditorConfig>, IComponent
    {
        public override string Name { get; set; } = "Various Fixes";

        private FixesController _controller = IoC.GetConstant<FixesController>();
        public FixesEditor(IO io) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles)
        {

        }

        public override void Disable() => _controller.Disable();
        public override void Enable() => _controller.Enable();

        // UI
        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                ProfileSelector.Render();
                EditFixes();
            }

            ImGui.End();
        }

        private unsafe void EditFixes()
        {
            // Fix item width for long labels.
            ImGui.PushItemWidth(ImGui.GetFontSize() * - 20);
            
            if (ImGui.TreeNodeStr("Startup"))
            {
                ImGui.Checkbox("Boot to Menu", ref Config.Data.BootToMenu);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Rendering Optimizations"))
            {
                ImGui.Checkbox("Fix D3D Device Flags", ref Config.Data.D3DDeviceFlags);
                Tooltip.TextOnHover("Applies on boot.");

                ImGui.Checkbox("Disable VSync ", ref Config.Data.DisableVSync);
                Tooltip.TextOnHover("Applies on boot.");

                if (ImGui.Checkbox("Frame Pacing Fix", ref Config.Data.FramePacing))
                    _controller.ResetSpeedup();

                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (Config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref Config.Data.FramePacingSpeedup);
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    ImGui.Text($"CPU Load {_controller.CpuUsage:00.00}%");
                    ImGui.Text($"Windows Timer Granularity: {_controller.TimerGranularity}ms");
                    Reflection.MakeControl(ref Config.Data.DisableYieldThreshold, "CPU Spin Disable Thread Yield Threshold");
                    Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                }

                ImGui.TreePop();
            }

            // Restore item width
            ImGui.PopItemWidth();
        }
    }
}
