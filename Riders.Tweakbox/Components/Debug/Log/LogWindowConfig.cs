using System;
using System.Text;
using System.Text.Json;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindowConfig : IConfiguration
    {
        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
        };

        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }

        /// <inheritdoc />
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Data, SerializerOptions));
        public Internal Data = new Internal();

        /// <inheritdoc />
        public void FromBytes(Span<byte> bytes)
        {
            Data = JsonSerializer.Deserialize<Internal>(bytes, SerializerOptions);
            ConfigUpdated?.Invoke();
        }

        /// <inheritdoc />
        public void Apply() => Data.Apply();
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new LogWindowConfig();

        public class Internal
        {
            public LogCategory Console = Misc.Log.DefaultConsoleCategories;
            public LogCategory Hud     = Misc.Log.DefaultHudCategories;

            public void Apply()
            {
                Misc.Log.HudCategories = Hud;
                Misc.Log.ConsoleCategories = Console;
            }
        }
    }
}
