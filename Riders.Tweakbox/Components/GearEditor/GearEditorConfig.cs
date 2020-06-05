using System;
using System.Linq;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Components.GearEditor
{
    public unsafe class GearEditorConfig : IConfiguration
    {
        private static GearEditorConfig _default = GearEditorConfig.FromGame();

        /// <summary>
        /// Extreme gears assigned to this config.
        /// </summary>
        public ExtremeGear[] Gears;

        /// <summary>
        /// Creates a <see cref="GearEditorConfig"/> from the values present in game memory.
        /// </summary>
        /// <returns></returns>
        public static GearEditorConfig FromGame() => new GearEditorConfig { Gears = Player.Gears.ToArray() };

        /// <summary>
        /// Updates the game information with the gear data stored in the class.
        /// </summary>
        public unsafe void Apply() => Player.Gears.CopyFrom(Gears, Gears.Length);

        public byte[] ToBytes() => StructArray.GetBytes(Gears);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            StructArray.FromArray(bytes, out Gears, Player.NumberOfGears);
            return bytes.Slice(StructArray.GetSize<ExtremeGear>(Player.NumberOfGears));
        }

        public IConfiguration GetDefault() => _default;
    }
}
