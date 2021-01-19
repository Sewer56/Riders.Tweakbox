using System;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindowConfig : IConfiguration
    {
        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <inheritdoc />
        public byte[] ToBytes() => Json.SerializeStruct(ref Misc.Log.EnabledCategories);
        public LogCategory Data = Misc.Log.DefaultCategories;

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Json.DeserializeStruct<LogCategory>(bytes);
            ConfigUpdated?.Invoke();
            return bytes.Slice(Struct.GetSize<LogCategory>());
        }

        /// <inheritdoc />
        public void Apply() => Misc.Log.EnabledCategories = Data;
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new LogWindowConfig();
    }
}
