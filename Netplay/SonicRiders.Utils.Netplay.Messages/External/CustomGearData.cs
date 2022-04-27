using System;
using Reloaded.Memory;
using Riders.Netplay.Messages.Misc.BitStream.Types;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Netplay.Messages.External;

/// <summary>
/// Contains custom gear data in Tweakbox
/// </summary>
public struct CustomGearData
{
    private const int CustomGearsSizeBits = 8;

    /// <summary>
    /// Executed to apply the custom character data to the game.
    /// </summary>
    public static Action<CustomGearData> ToGame = data => { };

    /// <summary>
    /// Obtains the custom character data from the game.
    /// </summary>
    public static Func<CustomGearData> FromGame = () => default;

    /// <summary>
    /// Currently used list of custom gear names.
    /// </summary>
    public string[] CustomGears;

    /// <summary>
    /// Extreme gears of the host player.
    /// </summary>
    public ExtremeGear[] Gears;

    /// <summary>
    /// Retrieves the approximate size of data in bytes to be sent over the network.
    /// </summary>
    public unsafe int GetDataSize()
    {
        const int otherDataSize = 2; // +1 for the flags. +1 for the gear count.
        var gearDataSize = StructArray.GetSize<ExtremeGear>(Player.NumberOfGears);
        if (CustomGears != null)
            gearDataSize += ((Utf8StringArray)CustomGears).GetDataSize(CustomGearsSizeBits);

        return gearDataSize + otherDataSize; 
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
            CustomGears = Utf8StringArray.Deserialize(ref bitStream, CustomGearsSizeBits);
    }

    /// <summary>
    /// Serializes the contents of this message to a stream.
    /// </summary>
    /// <param name="bitStream">The stream.</param>
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Write Header
        byte hasCustomGear = CustomGears == null || CustomGears.Length <= 0 ? (byte)0 : (byte)1;

        bitStream.Write<byte>((byte)Player.NumberOfGears);
        bitStream.WriteBit(hasCustomGear);

        // Write raw gear data.
        for (int x = 0; x < Player.NumberOfGears; x++)
            bitStream.WriteGeneric(Gears[x]);

        // Write Custom Gear Data
        if (hasCustomGear > 0)
            new Utf8StringArray(CustomGears).ToStream(ref bitStream, CustomGearsSizeBits);
    }
}