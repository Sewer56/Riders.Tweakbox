using System;
using System.Linq;
using System.Text;
using Reloaded.Memory;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using StructLinq;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct
{
    /// <summary>
    /// Contains information about all gears used in a race.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public struct GearData
    {
        public static int NumGears => Sewer56.SonicRiders.API.Player.NumberOfGears;
        private static readonly Encoding _encoding = Encoding.UTF8;
        private static readonly int _nullTerminatorLength = _encoding.GetByteCount("\0");

        /// <summary>
        /// Currently used list of custom gear names.
        /// </summary>
        public string[] CustomGears;

        /// <summary>
        /// Extreme gears of the host player.
        /// </summary>
        public ExtremeGear[] Gears;

        /// <summary>
        /// Retrieves the approximate size of data to be sent over the network.
        /// </summary>
        public unsafe int GetDataSize()
        {
            var gearDataSize = StructArray.GetSize<ExtremeGear>(NumGears);
            if (CustomGears != null)
                foreach (var gear in CustomGears)
                    gearDataSize += _encoding.GetByteCount(gear) + _nullTerminatorLength; // Null terminated.

            // + 1 for bool flag at start of struct.
            return gearDataSize + 1;
        }

        /// <summary>
        /// Writes the contents of this packet to the game memory.
        /// </summary>
        /// <param name="applyCustomGears">A function which applies custom gears to the game. Returns true on success, else false.</param>
        /// <returns>True on success, else false.</returns>
        public unsafe bool ToGame(Func<string[], bool> applyCustomGears)
        {
            // TODO: Custom Gear Support
            bool result = applyCustomGears != null ? applyCustomGears.Invoke(CustomGears) : true; 
            if (result)
                Player.Gears.CopyFrom(Gears, Gears.Length);

            return result;
        }

        /// <summary>
        /// Retrieves gear information from the game.
        /// </summary>
        /// <param name="customGears">List of custom gear names.</param>
        public static unsafe GearData FromGame(string[] customGears)
        {
            var data = new GearData()
            {
                CustomGears = customGears,
                Gears = Player.Gears.ToArray(),
            };

            return data;
        }

        /// <summary>
        /// Deserializes the contents of this message from a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            // Read header.
            byte gearCount = bitStream.Read<byte>();
            byte hasCustomGear = bitStream.ReadBit();

            // Get raw gear data.
            Gears = new ExtremeGear[gearCount];
            for (int x = 0; x < gearCount; x++)
                Gears[x] = bitStream.ReadGeneric<ExtremeGear>();

            // Get custom gear data.
            if (hasCustomGear > 0)
            {
                int customGearCount = gearCount - Player.OriginalNumberOfGears;
                CustomGears = new string[customGearCount];
                for (int x = 0; x < customGearCount; x++)
                    CustomGears[x] = bitStream.ReadString(1024, _encoding);
            }
        }

        /// <summary>
        /// Serializes the contents of this message to a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            // Write Header
            byte hasCustomGear = CustomGears == null || CustomGears.Length <= 0 ? (byte)0 : (byte)1;
            bitStream.Write<byte>((byte)NumGears);
            bitStream.WriteBit(hasCustomGear);

            // Write raw gear data.
            for (int x = 0; x < NumGears; x++)
                bitStream.WriteGeneric(Gears[x]);

            // Write Custom Gear Data
            if (hasCustomGear > 0)
            {
                foreach (var customGear in CustomGears)
                    bitStream.WriteString(customGear, 1024, _encoding);
            }
        }
    }
}