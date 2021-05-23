using System;
using System.IO;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components
{
    public abstract class ComponentBase : IComponent
    {
        /// <inheritdoc />
        public abstract string Name { get; set; }
        protected bool Enabled = false;

        /// <inheritdoc />
        public virtual ref bool IsEnabled() => ref Enabled;

        /// <inheritdoc />
        public abstract void Render();
    }

    /// <summary>
    /// Provides a base implementation for various windows to be implemented.
    /// </summary>
    public abstract class ComponentBase<TConfig> : ComponentBase, IComponent where TConfig : IConfiguration
    {
        protected TConfig Config = IoC.Get<TConfig>();
        protected IO Io;
        protected ProfileSelector ProfileSelector;

        protected ComponentBase(IO io, string configFolder, Func<string[]> getConfigFiles, string configExtension = IO.ConfigExtension)
        {
            Io = io;
            ProfileSelector = new ProfileSelector(configFolder, configExtension, Config.GetDefault().ToBytes(), getConfigFiles, LoadConfig, GetCurrentConfigBytes);
            if (File.Exists(ProfileSelector.CurrentConfiguration))
                LoadConfig(File.ReadAllBytes(ProfileSelector.CurrentConfiguration));
        }

        public virtual void LoadConfig(byte[] data)
        {
            Config.FromBytes(new Span<byte>(data));
            Config.Apply();
        }

        public virtual byte[] GetCurrentConfigBytes() => Config.GetCurrent().ToBytes();

        /// <inheritdoc />
        public virtual void Disable() { }

        /// <inheritdoc />
        public virtual void Enable() { }
    }
}
