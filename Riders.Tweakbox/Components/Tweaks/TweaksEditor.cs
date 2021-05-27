using System;
using DearImguiSharp;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TweaksEditor : ComponentBase<TweaksConfig>, IComponent
    {
        public override string Name { get; set; } = "Various Fixes";

        private FramePacingController _pacingController = IoC.Get<FramePacingController>();

        public TweaksEditor(IO io) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles, IO.JsonConfigExtension)
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
            var data = Config.Data;

            if (ImGui.CollapsingHeaderTreeNodeFlags("Startup", 0))
            {
                ImGui.Checkbox("Boot to Menu & Unlock All", ref data.BootToMenu).Notify(data, nameof(data.BootToMenu));
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Misc", 0))
            {
                ImGui.Checkbox("Return to Stage Select from Race", ref data.NormalRaceReturnToTrackSelect).Notify(data, nameof(data.NormalRaceReturnToTrackSelect));
                ImGui.Checkbox("Return to Stage Select from Tag", ref data.TagReturnToTrackSelect).Notify(data, nameof(data.TagReturnToTrackSelect));
                ImGui.Checkbox("Return to Stage Select from Survival", ref data.SurvivalReturnToTrackSelect).Notify(data, nameof(data.SurvivalReturnToTrackSelect));

                ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref data.AutoQTE).Notify(data, nameof(data.AutoQTE));
                ImGui.Checkbox("Force Single Player Stage Data", ref data.SinglePlayerStageData).Notify(data, nameof(data.SinglePlayerStageData));
                Tooltip.TextOnHover("Forces the game to load Single Player stage assets and Single Player Object Layout.");

                ImGui.Checkbox("Force Single Player Models", ref data.SinglePlayerModels).Notify(data, nameof(data.SinglePlayerModels));
                Tooltip.TextOnHover("Forces the game to load high quality single player models for all characters.");
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Graphics", 0))
            {
                ImGui.Text("Startup Settings");

                ImGui.DragInt("Resolution X", ref data.ResolutionX, 1, 640, 16384, null, 0).Notify(data, nameof(data.ResolutionX));
                ImGui.DragInt("Resolution Y", ref data.ResolutionY, 1, 480, 16384, null, 0).Notify(data, nameof(data.ResolutionY));
                Reflection.MakeControl(ref data.Fullscreen, "Fullscreen").Notify(data, nameof(data.Fullscreen));

                ImGui.Text("For these settings to apply, please *save* the default configuration above and restart the game.");

                ImGui.Separator();
                ImGui.Text("Live Settings");
                Reflection.MakeControl(ref data.Blur, "Blur").Notify(data, nameof(data.Blur));;
                Reflection.MakeControl(ref data.Borderless, "Borderless Windowed").Notify(data, nameof(data.Borderless));
                Reflection.MakeControl(ref data.WidescreenHack, "Centered Widescreen Hack").Notify(data, nameof(data.WidescreenHack));
                Tooltip.TextOnHover("Basic widescreen hack that centers the game content to the screen.\n" +
                                    "Do not combine/use with other widescreen hacks.");
            }

            if (ImGui.CollapsingHeaderTreeNodeFlags("Rendering Optimizations", 0))
            {
                ImGui.Text($"Current FPS: " + _pacingController.Fps.StatFPS);

                ImGui.PushItemWidth(ImGui.GetFontSize() * -20);
                ImGui.Checkbox("Fix D3D Device Flags", ref data.D3DDeviceFlags).Notify(data, nameof(data.D3DDeviceFlags));
                Tooltip.TextOnHover("Applies on boot.");

                ImGui.Checkbox("Disable VSync ", ref data.DisableVSync).Notify(data, nameof(data.DisableVSync));
                Tooltip.TextOnHover("Applies on boot.");

                ImGui.Checkbox("Frame Pacing Fix", ref data.FramePacing).Notify(data, nameof(data.FramePacing));
                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (Config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref data.FramePacingSpeedup).Notify(data, nameof(data.FramePacingSpeedup));
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    if (data.FramePacingSpeedup)
                        Reflection.MakeControl(ref data.MaxSpeedupTimeMillis, "Lag Compensation Max Amount (Milliseconds)").Notify(data, nameof(data.MaxSpeedupTimeMillis));

                    ImGui.Text($"CPU Load {_pacingController.CpuUsage:00.00}%");
                    ImGui.Text($"Windows Timer Granularity: {_pacingController.TimerGranularity}ms");
                    Reflection.MakeControl(ref data.DisableYieldThreshold, "CPU Spin Disable Thread Yield Threshold");
                    Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                }

                ImGui.PopItemWidth();
            }

            // Restore item width
            ImGui.PopItemWidth();
        }
    }
}
