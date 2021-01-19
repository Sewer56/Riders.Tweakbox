using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditor : ComponentBase<TweaksEditorConfig>, IComponent
    {
        public override string Name { get; set; } = "Various Fixes";

        private FramePacingController _pacingController = IoC.Get<FramePacingController>();
        public TweaksEditor(IO io) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles)
        {

        }

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
            ImGui.PushItemWidth(ImGui.GetFontSize() * - 12);
            
            if (ImGui.CollapsingHeaderTreeNodeFlags("Startup", 0))
            {
                ImGui.Checkbox("Boot to Menu", ref Config.Data.BootToMenu);
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Misc", 0))
            {
                ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref Config.Data.AutoQTE);
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Graphics", 0))
            {
                ImGui.Text("Live Settings");
                Reflection.MakeControl(ref Config.Data.Blur, "Blur");
                Reflection.MakeControl(ref Config.Data.Borderless, "Borderless Windowed");
                Reflection.MakeControl(ref Config.Data.WidescreenHack, "Widescreen Hack");
                Tooltip.TextOnHover("Basic widescreen hack that centers the game content to the screen. Work in progress on adding more HUD elements.");

                if (ImGui.ButtonEx("Apply", Constants.ButtonSize, 0))
                    Config.Apply();

                Tooltip.TextOnHover("Changing resolution mid-game is currently not supported.\n" +
                                    "Need to find every single texture, buffer etc. that needs to be recreated before calling Reset.\n" +
                                    "I need help of a graphics programmer experienced with DX9 for this one.");

                ImGui.Separator();
                ImGui.Text("Startup Settings");

                Reflection.MakeControl(ref Config.Data.ResolutionX, "Resolution X");
                Reflection.MakeControl(ref Config.Data.ResolutionY, "Resolution Y");
                Reflection.MakeControl(ref Config.Data.Fullscreen, "Fullscreen");
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Rendering Optimizations", 0))
            {
                ImGui.PushItemWidth(ImGui.GetFontSize() * -20);
                ImGui.Checkbox("Fix D3D Device Flags", ref Config.Data.D3DDeviceFlags);
                Tooltip.TextOnHover("Applies on boot.");

                ImGui.Checkbox("Disable VSync ", ref Config.Data.DisableVSync);
                Tooltip.TextOnHover("Applies on boot.");

                if (ImGui.Checkbox("Frame Pacing Fix", ref Config.Data.FramePacing))
                    _pacingController.ResetSpeedup();

                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (Config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref Config.Data.FramePacingSpeedup);
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    ImGui.Text($"CPU Load {_pacingController.CpuUsage:00.00}%");
                    ImGui.Text($"Windows Timer Granularity: {_pacingController.TimerGranularity}ms");
                    Reflection.MakeControl(ref Config.Data.DisableYieldThreshold, "CPU Spin Disable Thread Yield Threshold");
                    Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                }

                ImGui.PopItemWidth();
            }

            // Restore item width
            ImGui.PopItemWidth();
        }
    }
}
