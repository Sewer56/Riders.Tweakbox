﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.FixesEditor
{
    public class FixesEditor : IComponent
    {
        public string Name { get; set; } = "Various Fixes";

        private bool _isEnabled;
        private FixesEditorConfig _config = IoC.GetConstant<FixesEditorConfig>();
        private FixesController _controller = IoC.GetConstant<FixesController>();
        private IO _io;
        private ProfileSelector _profileSelector;

        public FixesEditor(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.FixesConfigFolder, IO.ConfigExtension, _config.GetDefault().ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();
        }

        // Profile Selector Implementation
        private void LoadConfig(byte[] data)
        {
            _config.FromBytes(new Span<byte>(data));
            _config.Apply();
        }

        private string[] GetConfigFiles() => _io.GetFixesConfigFiles();
        private byte[] GetCurrentConfigBytes() => _config.GetCurrent().ToBytes();

        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() => _controller.Disable();
        public void Enable() => _controller.Enable();

        // UI
        public void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                _profileSelector.Render();
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
                ImGui.Checkbox("Boot to Menu", ref _config.Data.BootToMenu);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Rendering Optimizations"))
            {
                ImGui.Checkbox("Fix D3D Device Flags", ref _config.Data.D3DDeviceFlags);
                Tooltip.TextOnHover("Applies on boot.");

                if (ImGui.Checkbox("Frame Pacing Fix", ref _config.Data.FramePacing))
                    _controller.ResetSpeedup();

                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (_config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref _config.Data.FramePacingSpeedup);
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    ImGui.Text($"CPU Load {_controller.CpuUsage:00.00}%");
                    ImGui.Text($"Windows Timer Granularity: {_controller.TimerGranularity}ms");
                    Reflection.MakeControl(ref _config.Data.DisableYieldThreshold, "CPU Spin Disable Thread Yield Threshold");
                    Tooltip.TextOnHover("Calls Sleep(0) while spinning when CPU usage is below this threshold.");
                }

                ImGui.TreePop();
            }

            // Restore item width
            ImGui.PopItemWidth();
        }
    }
}
