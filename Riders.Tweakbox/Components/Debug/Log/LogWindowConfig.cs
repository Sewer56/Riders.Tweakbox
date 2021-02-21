using System;
using MessagePack;
using Riders.Netplay.Messages.Misc;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindowConfig : IConfiguration
    {
        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <inheritdoc />
        public byte[] ToBytes() => MessagePackSerializer.Serialize(Misc.Log.EnabledCategories, MessagePack.Resolvers.ContractlessStandardResolver.Options);
        public LogCategory Data = Misc.Log.DefaultCategories;

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Utilities.DeserializeMessagePack<LogCategory>(bytes, out int bytesRead, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            ConfigUpdated?.Invoke();
            return bytes.Slice(bytesRead);
        }

        /// <inheritdoc />
        public void Apply() => Misc.Log.EnabledCategories = Data;
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new LogWindowConfig();
    }
}
