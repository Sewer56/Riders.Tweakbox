using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditor : ComponentBase<TweaksEditorConfig>, IComponent
    {
        public override string Name { get; set; } = "Various Fixes";

        private FixesController _controller = IoC.GetConstant<FixesController>();
        public TweaksEditor(IO io) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles)
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

            if (ImGui.TreeNodeStr("Misc"))
            {
                ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref Config.Data.AutoQTE);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Graphics"))
            {
                ImGui.Text("Live Settings");
                Reflection.MakeControl(ref Config.Data.Blur, "Blur");
                Reflection.MakeControl(ref Config.Data.Borderless, "Borderless Windowed");
                Reflection.MakeControl(ref Config.Data.WidescreenHack, "Widescreen Hack");
                Tooltip.TextOnHover("Basic widescreen hack that centers the game content to the screen. Work in progress on adding more HUD elements.");

                ImGui.Separator();
                ImGui.Text("Startup Settings");

                Reflection.MakeControl(ref Config.Data.ResolutionX, "Resolution X");
                Reflection.MakeControl(ref Config.Data.ResolutionY, "Resolution Y");
                Reflection.MakeControl(ref Config.Data.Fullscreen, "Fullscreen");

                ImGui.Separator();

                if (ImGui.ButtonEx("Apply", Constants.ButtonSize, 0))
                    Config.Apply();

                Tooltip.TextOnHover("Changing resolution mid-game is currently not supported.\n" +
                                    "Need to find every single texture, buffer etc. that needs to be recreated before calling Reset.\n" +
                                    "I need help of a graphics programmer experienced with DX9 for this one.");

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
