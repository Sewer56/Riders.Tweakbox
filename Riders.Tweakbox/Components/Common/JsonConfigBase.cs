using System;
using System.Text;
using System.Text.Json;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Serializers.Json;

namespace Riders.Tweakbox.Components.Common
{
    public abstract class JsonConfigBase<TParent, TConfig> : IConfiguration where TParent : new() where TConfig : new()
    {
        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            Converters = { new TextInputJsonConverter() }
        };

        protected JsonConfigBase() { }

        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <summary>
        /// The data of the current config.
        /// </summary>
        public TConfig Data = new TConfig();

        /// <inheritdoc />
        public virtual byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Data, SerializerOptions));

        /// <inheritdoc />
        public virtual void FromBytes(Span<byte> bytes)
        {
            Data = JsonSerializer.Deserialize<TConfig>(bytes, SerializerOptions);
            ConfigUpdated?.Invoke();
        }

        /// <inheritdoc />
        public virtual void Apply() { }

        /// <inheritdoc />
        public virtual IConfiguration GetCurrent() => this;

        /// <inheritdoc />
        public virtual IConfiguration GetDefault() => (IConfiguration) new TParent();
    }
}
