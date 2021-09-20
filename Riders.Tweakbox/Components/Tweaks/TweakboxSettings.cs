using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DearImguiSharp;
using EnumsNET;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Structures.Misc;
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
    private GameModifiersController _modifiersController;
    private bool _isVulkanLoaded;

    public TweakboxSettings(IO io, NetplayController netController, GameModifiersController modifiersController) : base(io, io.FixesConfigFolder, io.GetFixesConfigFiles, IO.JsonConfigExtension)
    {
        _netplayController = netController;
        Config.Data.AddPropertyUpdatedHandler(ResolutionUpdated);
        _modifiersController = modifiersController;
        _isVulkanLoaded = Native.GetModuleHandle("vulkan-1.dll") != IntPtr.Zero;
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

    public void RenderModifiersMenu() => RenderModifiersMenu_Internal();

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

        if (ImGui.CollapsingHeaderTreeNodeFlags("Fun", 0))
            RenderFunMenu(data);

        if (ImGui.CollapsingHeaderTreeNodeFlags("Controls", 0))
            RenderControls(data);

        // Restore item width
        ImGui.PopItemWidth();
    }

    private unsafe void RenderFunMenu(TweakboxConfig.Internal data)
    {
        if (!_netplayController.IsActive() && ImGui.TreeNodeStr("Race Tweaks (No Override in Netplay)"))
        {
            ImGui.Text("These settings are always enabled in Netplay regardless of user preference.");
            ImGui.Checkbox("Automatic QTE Bug (Simulate Keyboard Left+Right Hold)", ref data.AutoQTE).Notify(data, nameof(data.AutoQTE));
            ImGui.Checkbox("Allow going backwards in Races", ref data.AllowBackwardsDriving).Notify(data, nameof(data.AllowBackwardsDriving));
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Client Side (Unsynced) Options"))
        {
            ImGui.Checkbox("Drift Charge Indicator", ref data.DriftChargeColour.HasValue).Notify(data, nameof(data.DriftChargeColour));
            if (data.DriftChargeColour.HasValue)
                RenderColourPickerForRgba(ref data.DriftChargeColour.Value, "Colour");

            RenderColourPickerForRgba(ref data.IgnoreTurbulenceColour, "Turbulence Ignore Toggle Colour");
            Tooltip.TextOnHover("Colour used when ignoring turbulence via toggle. To enable toggle, see `Race Tweaks -> Turbulence`.");

            ImGui.TreePop();
        }

        if (_netplayController.CanEditSettings() && ImGui.TreeNodeStr("Race Tweaks (Netplay Synced)"))
        {
            RenderModifiersMenu_Internal();
            ImGui.TreePop();
        }
    }

    private void RenderModifiersMenu_Internal()
    {
        ref var mods = ref _modifiersController.Modifiers;

        if (ImGui.TreeNodeStr("Netplay Only Settings"))
        {
            ImGui.Checkbox("No Screenpeek", ref mods.NoScreenpeek).ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Uses the Minimal Amount of Cameras. Same as Automatic setting in Netplay player settings.");
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Player Interaction/Time Trials with Friends"))
        {
            ImGui.Checkbox("Disable Tornadoes", ref mods.DisableTornadoes).ExecuteIfTrue(SendUpdateNotification);
            ImGui.Checkbox("Disable Attacks", ref mods.DisableAttacks).ExecuteIfTrue(SendUpdateNotification);
            ImGui.Checkbox("Normalize Boost Durations", ref mods.NormalizedBoostDurations).ExecuteIfTrue(SendUpdateNotification);

            ref var atkDurFrm = ref mods.DisableAttackDurationFrames;
            Reflection.MakeControl(ref atkDurFrm, "Disable Race Start Attack", 0.5f, $"{atkDurFrm} ({mods.GetDisableAttackDuration().TotalMilliseconds:##}ms)").ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Sets the duration for which attacks should be disabled in the beginning of the race.");

            if (ImGui.TreeNodeStr("Ring Loss on Death"))
            {
                RenderRingLossMenu(ref mods.DeathRingLoss);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Ring Loss on Hit"))
            {
                RenderRingLossMenu(ref mods.HitRingLoss);
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Turbulence"))
        {
            ImGui.Checkbox("No Turbulence", ref mods.NoTurbulence).ExecuteIfTrue(SendUpdateNotification);
            ImGui.Checkbox("Always Turbulence", ref mods.AlwaysTurbulence).ExecuteIfTrue(SendUpdateNotification);
            ImGui.Checkbox("Disable Thin Turbulence", ref mods.DisableSmallTurbulence).ExecuteIfTrue(SendUpdateNotification);
            ImGui.Checkbox("Berserker Speed Fix", ref mods.BerserkerTurbulenceFix).ExecuteIfTrue(SendUpdateNotification);

            ImGui.Checkbox("Ignore Turbulence (Toggle)", ref mods.IgnoreTurbulenceOnToggle).ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Hotkey is DPAD_UP. Rebind this however you like using the Custom mapping mod.\n" +
                                "This will be changed in the future when Tweakbox adds support for more buttons.");
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Item Properties"))
        {
            ImGui.Checkbox("Replace 100 Ring Box", ref mods.ReplaceRing100Settings.Enabled).ExecuteIfTrue(SendUpdateNotification);
            if (mods.ReplaceRing100Settings.Enabled)
                Reflection.MakeControlEnum(ref mods.ReplaceRing100Settings.Replacement, "Ring 100 Replacement").ExecuteIfTrue(SendUpdateNotification);

            ImGui.Checkbox("Replace Air Max Box", ref mods.ReplaceMaxAirSettings.Enabled).ExecuteIfTrue(SendUpdateNotification);
            if (mods.ReplaceMaxAirSettings.Enabled)
                Reflection.MakeControlEnum(ref mods.ReplaceMaxAirSettings.Replacement, "Air Max Replacement").ExecuteIfTrue(SendUpdateNotification);

            Reflection.MakeControl(ref mods.ItemBoxProperties.RespawnTimerFrames, "Itembox Respawn Timer", 0.1f).ExecuteIfTrue(SendUpdateNotification);
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Catch-up/Rubberbanding"))
        {
            ImGui.Checkbox("Slipstream", ref mods.Slipstream.Enabled).ExecuteIfTrue(SendUpdateNotification);
            Reflection.MakeControl(ref mods.Slipstream.SlipstreamMaxAngle, "Max Angle (Degrees)", 0.001f, null).ExecuteIfTrue(SendUpdateNotification);
            ImGui.DragFloat("Max Strength", ref mods.Slipstream.SlipstreamMaxStrength, 0.0001f, 0f, 0.1f, "%.4f", 1).ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Strength is defined as the amount the player speed is multiplied by per frame.\n" +
                                "This value is scaled (using Max Angle) based on how perfectly your angle matches other players; with this being the upper bound.");

            Reflection.MakeControl(ref mods.Slipstream.SlipstreamMaxDistance, "Max Distance", 0.1f, null).ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Maximum distance for Slipstream. The closer the player, the more slipstream is applied. At the distance here, minimum slipstream is applied.");

            Reflection.MakeControlEnum(ref mods.Slipstream.EasingSetting, "Easing Setting").ExecuteIfTrue(SendUpdateNotification);
            Tooltip.TextOnHover("Controls the post processing algorithm used to scale slipstream bonus with range.\n" +
                                "Tweakbox scales' its slipstream such that more bonus is applied when you are closer to the opponent.\n" +
                                "These algorithms are listed in increasing levels of growth rate.\n" +
                                "Don't know what this means? Google \"Easing Functions\"");

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeStr("Miscellaneous"))
        {
            var airGainPercent = 100f + (mods.PitAirGainMultiplier * 100f);
            ImGui.DragFloat("Pit Air Gain Multiplier Offset", ref mods.PitAirGainMultiplier, 0.001f, -10000f, 10000f, $"{airGainPercent}%%", 1).ExecuteIfTrue(SendUpdateNotification);
            ImGui.TreePop();
        }

        void SendUpdateNotification() => _modifiersController.InvokeOnEditModifiers();

        void RenderRingLossMenu(ref RingLossBehaviour behaviour)
        {
            ImGui.Checkbox("Enabled", ref behaviour.Enabled).ExecuteIfTrue(SendUpdateNotification);
            var minPercent = 0.0f;
            var maxPercent = 100f;
            var minLoss = (byte) 0;
            var maxLoss = (byte) 100;

            Reflection.MakeControl(ref behaviour.RingLossBefore, "Loss Before Percentage", 0.1f, ref minLoss, ref maxLoss).ExecuteIfTrue(SendUpdateNotification);
            Reflection.MakeControl(ref behaviour.RingLossPercentage, "Loss Percentage", 0.01f, ref minPercent, ref maxPercent).ExecuteIfTrue(SendUpdateNotification);
            Reflection.MakeControl(ref behaviour.RingLossAfter, "Loss After Percentage", 0.1f, ref minLoss, ref maxLoss).ExecuteIfTrue(SendUpdateNotification);
        }
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
            if (ImGui.TreeNodeStr("Direct3D Flags"))
            {
                ImGui.TextWrapped("Note: These settings apply on boot!!");
                ImGui.Checkbox("Hardware Vertex Processing", ref data.HardwareVertexProcessing).Notify(data, nameof(data.HardwareVertexProcessing));
                ImGui.Checkbox("Disable PSGP Threading", ref data.DisablePsgpThreading).Notify(data, nameof(data.DisablePsgpThreading));
                ImGui.TreePop();
            }

            ImGui.Checkbox("Disable VSync", ref data.DisableVSync).Notify(data, nameof(data.DisableVSync));
            Tooltip.TextOnHover("Applies on boot.");

            if (!_netplayController.IsActive())
            {
                ImGui.Checkbox("Remove FPS Cap", ref data.RemoveFpsCap).Notify(data, nameof(data.RemoveFpsCap));
                Tooltip.TextOnHover("Intended for testing only.");
            }

            ImGui.Checkbox("Frame Pacing Fix", ref data.FramePacing).Notify(data, nameof(data.FramePacing));
            Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

            ImGui.Checkbox("Disable Particles", ref data.NoParticles).Notify(data, nameof(data.NoParticles));
            Tooltip.TextOnHover("Riders' implementation of particle emitters on PC is known for slowing the game down massively; causing some performance issues in some areas.\n" +
                                "This will make it so that particle emitters aren't spawned in levels.\n" +
                                "Setting takes effect on stage load.");

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

        if (!_isVulkanLoaded)
        {
            if (ImGui.Button("Having Performance Issues? Try DXVK.", Constants.Zero))
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/doitsujin/dxvk/releases") { CreateNoWindow = true });

            Tooltip.TextOnHover("Riders' implementation of particle emitters on PC is known for slowing the game down massively in some areas; causing performance issues.\n" +
                                "While Tweakbox can mitigate the problem partially, it can't be fully fixed without a particle system rewrite.\n" +
                                "Using the Vulkan wrapper with proper support for multithreading will improve your performance by 2-3x.");
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

    private void RenderControls(TweakboxConfig.Internal data)
    {
        ImGui.PushItemWidth(ImGui.GetFontSize() * -6);
        Reflection.MakeControlEnum(ref data.InputMode, "Input Mode");
        if (data.InputMode == TweakboxConfig.GameInput.Toggle)
            Reflection.MakeControlEnum(ref data.InputToggleKey, "Toggle Key");
        ImGui.PopItemWidth();
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

    private unsafe void RenderColourPickerForRgba(ref ColorRGBA color, string label)
    {
        Span<float> values = stackalloc float[4];
        ColourToFloat(color, values);
        ImGui.Custom.ColorEdit4(label, (float*)Unsafe.AsPointer(ref values.GetPinnableReference()), 1);
        color = FloatToColour(values);
    }

    private void ColourToFloat(ColorRGBA color, Span<float> colors)
    {
        colors[0] = color.Red   / 255.0f;
        colors[1] = color.Green / 255.0f;
        colors[2] = color.Blue  / 255.0f;
        colors[3] = color.Alpha / 255.0f;
    }

    private ColorRGBA FloatToColour(Span<float> colors)
    {
        return new ColorRGBA()
        {
            Red = (byte)(colors[0] * 255.0f),
            Green = (byte)(colors[1] * 255.0f),
            Blue = (byte)(colors[2] * 255.0f),
            Alpha = (byte)(colors[3] * 255.0f)
        };
    }
}
