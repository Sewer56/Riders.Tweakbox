using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Misc
{
    public class LogWindow : IComponent
    {
        /// <inheritdoc />
        public string Name { get; set; } = "Log Configuration";

        private bool _isEnabled;
        private LogWindowConfig _config = IoC.GetConstant<LogWindowConfig>();
        private IO _io;
        private ProfileSelector _profileSelector;

        public LogWindow(IO io)
        {
            _io = io;
            _profileSelector = new ProfileSelector(_io.LogConfigFolder, IO.ConfigExtension, _config.GetDefault().ToBytes(), GetConfigFiles, LoadConfig, GetCurrentConfigBytes);
            _profileSelector.Save();
        }

        private void LoadConfig(byte[] data)
        {
            _config.FromBytes(new Span<byte>(data));
            _config.Apply();
        }

        private byte[] GetCurrentConfigBytes() => _config.GetCurrent().ToBytes();
        private string[] GetConfigFiles() => _io.GetFixesConfigFiles();

        /// <inheritdoc />
        public ref bool IsEnabled() => ref _isEnabled;

        /// <inheritdoc />
        public void Disable() { }

        /// <inheritdoc />
        public void Enable() { }

        /// <inheritdoc />
        public unsafe void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                _profileSelector.Render();
                Reflection.MakeControlEnum((LogCategory*) Unsafe.AsPointer(ref Log.EnabledCategories), "Enabled Categories");
            }

            ImGui.End();
        }
    }

    internal class LogWindowConfig : IConfiguration
    {
        /// <inheritdoc />
        public byte[] ToBytes() => Json.SerializeStruct(ref Log.EnabledCategories);

        public LogCategory Data = Log.DefaultCategories;

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Json.DeserializeStruct<LogCategory>(bytes);
            return bytes.Slice(Struct.GetSize<LogCategory>());
        }

        /// <inheritdoc />
        public void Apply() => Log.EnabledCategories = Data;
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new LogWindowConfig();
    }
}
