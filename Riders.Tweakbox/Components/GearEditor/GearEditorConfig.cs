using System;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Gameplay;

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
        public static GearEditorConfig FromGame()
        {
            var config = new GearEditorConfig();
            StructArray.FromPtr((IntPtr)ExtremeGears.ExtremeGear, out config.Gears, ExtremeGears.NumberOfGears);
            return config;
        }

        /// <summary>
        /// Updates the game information with the gear data stored in the class.
        /// </summary>
        public unsafe void Apply() => StructArray.ToPtr((IntPtr)ExtremeGears.ExtremeGear, Gears);

        public byte[] ToBytes() => StructArray.GetBytes(Gears);
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            StructArray.FromArray(bytes, out Gears, ExtremeGears.NumberOfGears);
            return bytes.Slice(StructArray.GetSize<ExtremeGear>(ExtremeGears.NumberOfGears));
        }

        public IConfiguration GetDefault() => _default;
    }
}
