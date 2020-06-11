using System;
using System.Runtime.CompilerServices;
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
        private FixesConfig _config = IoC.GetConstant<FixesConfig>();
        private IO _io;
        private ProfileSelector _profileSelector;

        public FixesEditor(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.GearConfigFolder, IO.ConfigExtension, _config.ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();
        }

        // Profile Selector Implementation
        private void LoadConfig(byte[] data)
        {
            var decompressed = IO.DecompressLZ4(data);
            var fileSpan = new Span<byte>(decompressed);
            _config.FromBytes(fileSpan);
            _config.Apply();
        }

        private string[] GetConfigFiles() => _io.GetGearConfigFiles();
        private byte[] GetCurrentConfigBytes() => _config.GetCurrent().ToBytes();

        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() => _config.Controller.Disable();
        public void Enable() => _config.Controller.Enable();

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
            if (ImGui.TreeNodeStr("Frame Pacing"))
            {
                ImGui.Checkbox("Frame Pacing Fix", ref Unsafe.AsRef<bool>(_config.Controller.FramePacing.Pointer));
                Reflection.MakeControl((byte*)_config.Controller.SpinTime.Pointer, nameof(FixesController.SpinTime));
            }
        }
    }
}
