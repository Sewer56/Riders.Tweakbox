using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc.BitStream;
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
        /// Currently used list of modified characters.
        /// </summary>
        public List<string>[] ModifiedCharacters;

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
            {
                foreach (var gear in CustomGears)
                    gearDataSize += _encoding.GetByteCount(gear) + _nullTerminatorLength;

                gearDataSize += sizeof(ushort); // Array size
            }

            if (ModifiedCharacters != null)
            {
                foreach (var characterModifiers in ModifiedCharacters)
                    foreach (var characterModifier in characterModifiers)
                        gearDataSize += _encoding.GetByteCount(characterModifier) + _nullTerminatorLength;

                gearDataSize += sizeof(ushort) * ModifiedCharacters.Length;
            }

            // + 1 for bool flags at start of struct.
            // ushort for number of modified characters.
            return gearDataSize + 1 + sizeof(ushort);
        }
        /// <summary>
        /// Writes the contents of this packet to the game memory.
        /// </summary>
        /// <param name="applyCustomGears">A function which applies custom gears to the game. Returns true on success, else false.</param>
        /// <param name="applyModifiedCharacters">A function which applies modified gears to the game. Returns true on success, else false.</param>
        /// <returns>True on success, else false.</returns>
        public unsafe bool ToGame(Func<string[], bool> applyCustomGears, Func<List<string>[], bool> applyModifiedCharacters)
        {
            // TODO: Custom Gear Support
            bool resultGears = applyCustomGears != null ? applyCustomGears.Invoke(CustomGears) : true; 
            if (resultGears)
                Player.Gears.CopyFrom(Gears, Gears.Length);

            bool resultCharacters = applyModifiedCharacters != null ? applyModifiedCharacters.Invoke(ModifiedCharacters) : true;
            return resultGears && resultCharacters;
        }

        /// <summary>
        /// Retrieves gear information from the game.
        /// </summary>
        /// <param name="customGears">List of custom gear names.</param>
        /// <param name="modifiedCharacters">List of modified character names.</param>
        public static unsafe GearData FromGame(string[] customGears, List<string>[] modifiedCharacters)
        {
            var data = new GearData()
            {
                CustomGears = customGears,
                Gears = Player.Gears.ToArray(),
                ModifiedCharacters = modifiedCharacters
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
            byte hasCustomCharacter = bitStream.ReadBit();

            // Get raw gear data.
            Gears = new ExtremeGear[gearCount];
            for (int x = 0; x < gearCount; x++)
                Gears[x] = bitStream.ReadGeneric<ExtremeGear>();

            // Get custom gear data.
            if (hasCustomGear > 0)
                CustomGears = bitStream.ReadStringArray(1024, _encoding);

            // Get custom character data
            if (hasCustomCharacter > 0)
            {
                ModifiedCharacters = new List<string>[bitStream.Read<ushort>()];
                for (int x = 0; x < ModifiedCharacters.Length; x++)
                    ModifiedCharacters[x] = bitStream.ReadStringList(1024, _encoding);
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
            byte hasCustomCharacter = ModifiedCharacters == null || ModifiedCharacters.All(x => x.Count <= 0) ? (byte)0 : (byte)1;

            bitStream.Write<byte>((byte)NumGears);
            bitStream.WriteBit(hasCustomGear);
            bitStream.WriteBit(hasCustomCharacter);

            // Write raw gear data.
            for (int x = 0; x < NumGears; x++)
                bitStream.WriteGeneric(Gears[x]);

            // Write Custom Gear Data
            if (hasCustomGear > 0)
                bitStream.WriteStringArray(CustomGears, 1024, _encoding);

            // Write custom character data.
            if (hasCustomCharacter > 0)
            {
                bitStream.Write<ushort>((ushort)ModifiedCharacters.Length);
                foreach (var modifiedCharacter in ModifiedCharacters)
                    bitStream.WriteStringArray(CollectionsMarshal.AsSpan(modifiedCharacter).Slice(0, modifiedCharacter.Count), 1024, _encoding);
            }
        }
    }
}