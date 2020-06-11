using System;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.FixesEditor
{
    public class FixesConfig : IConfiguration
    {
        public FixesController Controller = IoC.GetConstant<FixesController>();
        private Internal _internal = Internal.GetDefault();

        public byte FramePacing
        {
            get => _internal.FramePacing;
            set => _internal.FramePacing = value;
        }

        public byte SpinTime
        {
            get => _internal.SpinTime;
            set => _internal.SpinTime = value;
        }

        // Serialization
        public byte[] ToBytes() => Struct.GetBytes(_internal);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Struct.FromArray(bytes, out _internal);
            return bytes.Slice(Struct.GetSize<Internal>());
        }

        // Apply
        public void Apply()
        {
            Controller.FramePacing.Value = FramePacing;
            Controller.SpinTime.Value = SpinTime;
        }

        public IConfiguration GetCurrent() => new FixesConfig() { FramePacing = Controller.FramePacing.Value };
        public IConfiguration GetDefault() => new FixesConfig();

        #region Internal
        public struct Internal
        {
            public byte FramePacing;
            public byte SpinTime;

            internal static Internal GetDefault() => new Internal { FramePacing = 1, SpinTime = 1 };
        }
        #endregion
    }
}
