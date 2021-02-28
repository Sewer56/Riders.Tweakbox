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
        private PatchController _patchController = IoC.Get<PatchController>();

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
                ImGui.Checkbox("Boot to Menu", ref Config.Data.BootToMenu.Value);
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Misc", 0))
            {
                ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref Config.Data.AutoQTE.Value);
                ImGui.Checkbox("Force Single Player Stage Data", ref Config.Data.SinglePlayerStageData.Value);
                Tooltip.TextOnHover("Forces the game to load Single Player stage assets and Single Player Object Layout.");

                if (ImGui.Checkbox("Force Single Player Models", ref Config.Data.SinglePlayerModels.Value))
                    _patchController.AlwaysLoadSinglePlayerCharacterModels.Set(Config.Data.SinglePlayerModels.Value);

                Tooltip.TextOnHover("Forces the game to load high quality single player models for all characters.");
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Graphics", 0))
            {
                ImGui.Text("Startup Settings");

                Reflection.MakeControl(ref Config.Data.ResolutionX.Value, "Resolution X");
                Reflection.MakeControl(ref Config.Data.ResolutionY.Value, "Resolution Y");
                Reflection.MakeControl(ref Config.Data.Fullscreen.Value, "Fullscreen");

                ImGui.Text("For these settings to apply, please *save* the default configuration above and restart the game.");

                ImGui.Separator();
                ImGui.Text("Live Settings");
                Reflection.MakeControl(ref Config.Data.Blur.Value, "Blur");
                Reflection.MakeControl(ref Config.Data.Borderless.Value, "Borderless Windowed");
                Reflection.MakeControl(ref Config.Data.WidescreenHack.Value, "Centered Widescreen Hack");
                Tooltip.TextOnHover("Basic widescreen hack that centers the game content to the screen.\n" +
                                    "Do not combine/use with other widescreen hacks.");

                if (ImGui.Button("Apply", Constants.ButtonSize))
                    Config.Apply();

                Tooltip.TextOnHover("Changing resolution mid-game is currently not supported.\n" +
                                    "Need to find every single texture, buffer etc. that needs to be recreated before calling Reset.\n" +
                                    "I need help of a graphics programmer experienced with DX9 for this one.");
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Rendering Optimizations", 0))
            {
                ImGui.PushItemWidth(ImGui.GetFontSize() * -20);
                ImGui.Checkbox("Fix D3D Device Flags", ref Config.Data.D3DDeviceFlags.Value);
                Tooltip.TextOnHover("Applies on boot.");

                ImGui.Checkbox("Disable VSync ", ref Config.Data.DisableVSync.Value);
                Tooltip.TextOnHover("Applies on boot.");

                if (ImGui.Checkbox("Frame Pacing Fix", ref Config.Data.FramePacing.Value))
                    _pacingController.ResetSpeedup();

                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (Config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref Config.Data.FramePacingSpeedup.Value);
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    ImGui.Text($"CPU Load {_pacingController.CpuUsage:00.00}%");
                    ImGui.Text($"Windows Timer Granularity: {_pacingController.TimerGranularity}ms");
                    Reflection.MakeControl(ref Config.Data.DisableYieldThreshold.Value, "CPU Spin Disable Thread Yield Threshold");
                    Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                }

                ImGui.PopItemWidth();
            }

            // Restore item width
            ImGui.PopItemWidth();
        }
    }
}
