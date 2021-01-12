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

        public byte[] ToBytes() => Utilities.CompressLZ4Stream(StructArray.GetBytes(Gears), LZ4Level.L12_MAX);

        public Span<byte> FromBytes(Span<byte> bytes)
        {
            fixed (byte* ptr = bytes)
            {
                using var source = new UnmanagedMemoryStream(ptr, bytes.Length);
                using var compressor = LZ4Stream.Decode(source, 0, true);
                using var target = new MemoryStream(LZ4Codec.MaximumOutputSize(bytes.Length));

                compressor.CopyTo(target);
                StructArray.FromArray(target.ToArray(), out Gears, true, Player.NumberOfGears);
                return bytes.Slice((int)source.Position);
            }
        }

        public IConfiguration GetCurrent() => FromGame();
        public IConfiguration GetDefault() => _default;
    }
}
