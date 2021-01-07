using System;
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
            _profileSelector = new ProfileSelector(_io.FixesConfigFolder, IO.ConfigExtension, _config.ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
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
            if (ImGui.TreeNodeStr("Startup"))
            {
                ImGui.Checkbox("Boot to Menu", ref _config.Data.BootToMenu);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Frame Pacing"))
            {
                if (ImGui.Checkbox("Frame Pacing Fix", ref _config.Data.FramePacing))
                    _controller.ResetSpeedup();
                Tooltip.TextOnHover("Replaces game's framerate limiter with a custom one. Eliminates stuttering. Makes times more consistent.");

                if (_config.Data.FramePacing)
                {
                    ImGui.Checkbox("Lag Compensation", ref _config.Data.FramePacingSpeedup);
                    Tooltip.TextOnHover("Speeds up the game to compensate for lag.");

                    Reflection.MakeControl(ref _config.Data.SpinTime, nameof(_config.Data.SpinTime));
                    Tooltip.TextOnHover("Higher values use more CPU but make frame pacing more accurate. Recommended value: 1");
                }

                ImGui.TreePop();
            }
        }
    }
}
