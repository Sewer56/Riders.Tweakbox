using System;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components
{
    /// <summary>
    /// Provides a base implementation for various windows to be implemented.
    /// </summary>
    public abstract class ComponentBase<TConfig> : IComponent where TConfig : IConfiguration
    {
        /// <inheritdoc />
        public abstract string Name { get; set; }

        protected bool Enabled = false;
        protected TConfig Config = IoC.GetConstant<TConfig>();
        protected IO Io;
        protected ProfileSelector ProfileSelector;

        protected ComponentBase(IO io, string configFolder, Func<string[]> getConfigFiles)
        {
            Io = io;
            ProfileSelector = new ProfileSelector(configFolder, IO.ConfigExtension, Config.GetDefault().ToBytes(), getConfigFiles, LoadConfig, GetCurrentConfigBytes);
            ProfileSelector.Save();
        }

        public virtual void LoadConfig(byte[] data)
        {
            Config.FromBytes(new Span<byte>(data));
            Config.Apply();
        }

        public virtual byte[] GetCurrentConfigBytes() => Config.GetCurrent().ToBytes();

        /// <inheritdoc />
        public ref bool IsEnabled() => ref Enabled;

        /// <inheritdoc />
        public virtual void Disable() { }

        /// <inheritdoc />
        public virtual void Enable() { }

        /// <inheritdoc />
        public abstract void Render();
    }
}
