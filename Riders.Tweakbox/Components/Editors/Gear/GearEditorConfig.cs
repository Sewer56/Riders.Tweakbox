using System;
using System.IO;
using System.Linq;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Components.Editors.Gear
{
    public unsafe class GearEditorConfig : IConfiguration
    {
        private static GearEditorConfig _default = new GearEditorConfig();

        /// <summary>
        /// Extreme gears assigned to this config.
        /// </summary>
        public ExtremeGear[] Gears;

        /// <summary>
        /// Creates the default editor config.
        /// </summary>
        public GearEditorConfig()
        {
            Gears = Player.Gears.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="GearEditorConfig"/> from the values present in game memory.
        /// </summary>
        /// <returns></returns>
        public static GearEditorConfig FromGame() => new GearEditorConfig();

        /// <summary>
        /// Updates the game information with the gear data stored in the class.
        /// </summary>
        public unsafe void Apply() => Player.Gears.CopyFrom(Gears, Gears.Length);

        /// <inheritdoc />
        public Action ConfigUpdated { get; set; }
        public byte[] ToBytes() => LZ4.CompressLZ4Stream(StructArray.GetBytes(Gears), LZ4Level.L12_MAX);

        public Span<byte> FromBytes(Span<byte> bytes)
        {
            var outputArray  = new byte[StructArray.GetSize<ExtremeGear>(Player.NumberOfGears)];
            var decompressed = LZ4.DecompressLZ4Stream(outputArray, bytes, out int bytesRead);

            StructArray.FromArray(decompressed, out Gears, true, Player.NumberOfGears);
            ConfigUpdated?.Invoke();
            return bytes.Slice((int)bytesRead);
        }

        public IConfiguration GetCurrent() => FromGame();
        public IConfiguration GetDefault() => _default;
    }
}
