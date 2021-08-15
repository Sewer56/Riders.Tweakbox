using System.Collections.Generic;
using DearImguiSharp;
using EnumsNET;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using SharpDX.Direct3D9;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;
namespace Riders.Tweakbox.Components.Tweaks;

public class TweakboxSettings : ComponentBase<TweakboxConfig>, IComponent
{
    public override string Name { get; set; } = "Tweakbox Settings";

    private FramePacingController _pacingController = IoC.Get<FramePacingController>();
    private Direct3DController _d3dController = IoC.Get<Direct3DController>();

    private string _currentModeString;
    private DisplayMode? _currentMode;
    private List<string> _modes;
    private NetplayController _netplayController;

    public TweakboxSettings(IO io, NetplayController netController) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles, IO.JsonConfigExtension)
    {
        _netplayController = netController;
        Config.Data.AddPropertyUpdatedHandler(ResolutionUpdated);
    }

    // UI
    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            ProfileSelector.Render();
            RenderMenu();
        }

        ImGui.End();
    }

    private unsafe void RenderMenu()
    {
        // Fix item width for long labels.
        ImGui.PushItemWidth(ImGui.GetFontSize() * -12);
        var data = Config.Data;

        if (ImGui.CollapsingHeaderTreeNodeFlags("Graphics", 0))
            RenderGraphicsMenu(data);

        if (ImGui.CollapsingHeaderTreeNodeFlags("Startup", 0))
            RenderStartupMenu(data);

        if (ImGui.CollapsingHeaderTreeNodeFlags("Misc", 0))
            RenderMiscMenu(data);

        // Restore item width
        ImGui.PopItemWidth();
    }

    private void RenderGraphicsMenu(TweakboxConfig.Internal data)
    {
        ImGui.Text("Startup Settings");
        if (RenderChangeResolutionCombo())
        {
            var mode = _currentMode.Value;
            data.ResolutionX = mode.Width;
            data.ResolutionY = mode.Height;
            data.RaisePropertyUpdated(nameof(data.ResolutionX));
            data.RaisePropertyUpdated(nameof(data.ResolutionY));
        }

        if (!data.Fullscreen)
        {
            ImGui.DragInt("Resolution X", ref data.ResolutionX, 1, 640, 16384, null, 0).Notify(data, nameof(data.ResolutionX));
            ImGui.DragInt("Resolution Y", ref data.ResolutionY, 1, 480, 16384, null, 0).Notify(data, nameof(data.ResolutionY));
        }

        if (IsFullscreenSupported())
            Reflection.MakeControl(ref data.Fullscreen, "Fullscreen").Notify(data, nameof(data.Fullscreen));

        Reflection.MakeControl(ref data.Borderless, "Borderless Windowed").Notify(data, nameof(data.Borderless));
        Reflection.MakeControl(ref data.Blur, "Blur").Notify(data, nameof(data.Blur));

        if (ImGui.TreeNodeStr("High Quality Models"))
        {
            ImGui.Checkbox("Force Single Player Stage Data", ref data.SinglePlayerStageData).Notify(data, nameof(data.SinglePlayerStageData));
            Tooltip.TextOnHover("Forces the game to load Single Player stage assets and Single Player Object Layout.");

            ImGui.Checkbox("Force Single Player Models", ref data.SinglePlayerModels).Notify(data, nameof(data.SinglePlayerModels));
            Tooltip.TextOnHover("Forces the game to load high quality single player models for all characters.");
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Rendering Optimizations"))
        {
            ImGui.Text($"Current FPS: " + _pacingController.Fps.StatFPS);

            ImGui.PushItemWidth(ImGui.GetFontSize() * -20);
            ImGui.Checkbox("Fix D3D Device Flags", ref data.D3DDeviceFlags).Notify(data, nameof(data.D3DDeviceFlags));
            Tooltip.TextOnHover("Applies on boot.");

            ImGui.Checkbox("Disable VSync", ref data.DisableVSync).Notify(data, nameof(data.DisableVSync));
            Tooltip.TextOnHover("Applies on boot.");

            if (!_netplayController.IsActive())
            {
                ImGui.Checkbox("Remove FPS Cap", ref data.RemoveFpsCap).Notify(data, nameof(data.RemoveFpsCap));
                Tooltip.TextOnHover("Intended for testing only.");
            }

            ImGui.Checkbox("Frame Pacing Fix", ref data.FramePacing).Notify(data, nameof(data.FramePacing));
            Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

            if (Config.Data.FramePacing && ImGui.TreeNodeStr("Frame Pacing Settings"))
            {
                ImGui.Checkbox("Lag Compensation", ref data.FramePacingSpeedup).Notify(data, nameof(data.FramePacingSpeedup));
                Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                if (data.FramePacingSpeedup)
                    Reflection.MakeControl(ref data.MaxSpeedupTimeMillis, "Lag Compensation Max Amount (Milliseconds)").Notify(data, nameof(data.MaxSpeedupTimeMillis));

                ImGui.Text($"CPU Load {_pacingController.CpuUsage:00.00}%");
                ImGui.Text($"Windows Timer Granularity: {_pacingController.TimerGranularity}ms");
                Reflection.MakeControl(ref data.DisableYieldThreshold, "CPU Spin Disable Thread Yield Threshold");
                Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                ImGui.TreePop();
            }

            ImGui.PopItemWidth();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Experimental"))
        {
            Reflection.MakeControl(ref data.WidescreenHack, "Centered Widescreen Hack").Notify(data, nameof(data.WidescreenHack));
            Tooltip.TextOnHover("Basic widescreen hack that centers the game content to the screen.\n" +
                                "Do not combine/use with other widescreen hacks.");
            ImGui.TreePop();
        }
    }

    private void RenderMiscMenu(TweakboxConfig.Internal data)
    {
        if (!_netplayController.IsActive() && ImGui.TreeNodeStr("Main Menu Behaviour"))
        {
            ImGui.Checkbox("Return to Stage Select from Race", ref data.NormalRaceReturnToTrackSelect).Notify(data, nameof(data.NormalRaceReturnToTrackSelect));
            ImGui.Checkbox("Return to Stage Select from Tag", ref data.TagReturnToTrackSelect).Notify(data, nameof(data.TagReturnToTrackSelect));
            ImGui.Checkbox("Return to Stage Select from Survival", ref data.SurvivalReturnToTrackSelect).Notify(data, nameof(data.SurvivalReturnToTrackSelect));
            ImGui.TreePop();
        }

        if (!_netplayController.IsActive() && ImGui.TreeNodeStr("Race Tweaks"))
        {
            ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref data.AutoQTE).Notify(data, nameof(data.AutoQTE));
            ImGui.Checkbox("Allow going backwards in Races", ref data.AllowBackwardsDriving).Notify(data, nameof(data.AllowBackwardsDriving));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Music Injection"))
        {
            ImGui.Checkbox("Include Vanilla Music", ref data.IncludeVanillaMusic).Notify(data, nameof(data.IncludeVanillaMusic));
            Tooltip.TextOnHover("When playing custom music tracks, allow Vanilla tracks to still be played.");
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Language Options"))
        {
            Reflection.MakeControlEnum(ref data.Language, "Message Language").Notify(data, nameof(data.Language));
            Reflection.MakeControlEnum(ref data.VoiceLanguage, "Voice Language").Notify(data, nameof(data.VoiceLanguage));
            ImGui.TextWrapped("Language changes will take effect after the next loading screen.");
            ImGui.TreePop();
        }
    }

    private static void RenderStartupMenu(TweakboxConfig.Internal data)
    {
        ImGui.Checkbox("Boot to Menu & Unlock All", ref data.BootToMenu).Notify(data, nameof(data.BootToMenu));
        if (data.BootToMenu && ImGui.TreeNodeStr("Boot to Menu Settings"))
        {
            ImGui.Checkbox("Boot to Race", ref data.BootToRace).Notify(data, nameof(data.BootToRace));

            if (data.BootToRace && ImGui.TreeNodeStr("Boot to Race Settings"))
            {
                Reflection.MakeControlEnum(ref data.BootToRaceLevel, "Stage");
                Reflection.MakeControlEnum(ref data.BootToRaceCharacter, "Character");
                Reflection.MakeControlEnum(ref data.BootToRaceGear, "Gear");
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        Reflection.MakeControlEnum(ref data.MemoryLimit, "Memory Limit (MB)");
        Tooltip.TextOnHover("Sets the max amount of memory allowed to be used by the game's internal buffer.\n" +
                            "This value affects maximum file sizes allowed for native game models, textures etc.");
    }

    private bool IsFullscreenSupported()
    {
        foreach (var mode in _d3dController.Modes)
        {
            if (_currentMode.Value.ResolutionEqual(mode))
                return true;
        }

        return false;
    }

    private bool RenderChangeResolutionCombo()
    {
        _modes ??= _d3dController.Modes.AsStrings();
        _currentMode ??= _d3dController.D3dEx.GetAdapterDisplayMode(0);
        _currentModeString = _currentMode.Value.AsString();
        var result = false;

        if (ImGui.BeginCombo("Resolution Preset", _currentModeString, 0))
        {
            var modes = _d3dController.Modes;
            for (int x = 0; x < modes.Count; x++)
            {
                bool isSelected = _currentMode.Value.Equal(modes[x]);
                if (ImGui.SelectableBool(_modes[x], isSelected, 0, Constants.DefaultVector2))
                {
                    SetDisplayMode(modes[x]);
                    result = true;
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        return result;
    }

    private void ResolutionUpdated(string propertyname)
    {
        var data = Config.Data;
        if (propertyname.Equals(nameof(data.ResolutionX)) || propertyname.Equals(nameof(data.ResolutionY)))
            SetDisplayMode(GetCurrentDisplayMode());
    }

    private void SetDisplayMode(DisplayMode mode)
    {
        _currentMode = mode;
        _currentModeString = _currentMode.Value.AsString();
    }

    private DisplayMode GetCurrentDisplayMode()
    {
        var data = Config.Data;
        return new DisplayMode()
        {
            Format = Format.X8R8G8B8,
            Height = data.ResolutionY,
            Width = data.ResolutionX,
            RefreshRate = 0
        };
    }
}
