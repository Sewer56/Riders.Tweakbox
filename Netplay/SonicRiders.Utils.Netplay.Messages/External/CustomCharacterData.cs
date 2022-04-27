using System;
using Riders.Netplay.Messages.Misc.BitStream.Types;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.External;

/// <summary>
/// Contains custom character data in Tweakbox
/// </summary>
public struct CustomCharacterData
{
    private const int CustomCharacterModifiersSizeBits = 16;

    /// <summary>
    /// Executed to apply the custom character data to the game.
    /// </summary>
    public static Action<CustomCharacterData> ToGame = data => { };

    /// <summary>
    /// Obtains the custom character data from the game.
    /// </summary>
    public static Func<CustomCharacterData> FromGame = () => default;

    /// <summary>
    /// Currently used list of modified characters.
    /// </summary>
    public string[] ModifiedCharacters = null;

    public CustomCharacterData() { }

    /// <summary>
    /// Retrieves the approximate size of data in bytes to be sent over the network.
    /// </summary>
    public unsafe int GetDataSize()
    {
        var dataSize = 0;
        if (ModifiedCharacters != null)
            dataSize += new Utf8StringArray(ModifiedCharacters).GetDataSize(CustomCharacterModifiersSizeBits);

        return dataSize;
    }

    /// <summary>
    /// Deserializes the contents of this message from a stream.
    /// </summary>
    /// <param name="bitStream">The stream.</param>
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Read header.
        byte hasCustomCharacter = bitStream.ReadBit();
        
        // Get custom character data
        if (hasCustomCharacter > 0)
            ModifiedCharacters = Utf8StringArray.Deserialize(ref bitStream, CustomCharacterModifiersSizeBits);
        else
            ModifiedCharacters = Array.Empty<string>();
    }

    /// <summary>
    /// Serializes the contents of this message to a stream.
    /// </summary>
    /// <param name="bitStream">The stream.</param>
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Write Header
        byte hasCustomCharacter = ModifiedCharacters == null || ModifiedCharacters.Length <= 0 ? (byte)0 : (byte)1;
        bitStream.WriteBit(hasCustomCharacter);
        
        // Write custom character data.
        if (hasCustomCharacter > 0)
            new Utf8StringArray(ModifiedCharacters).ToStream(ref bitStream, CustomCharacterModifiersSizeBits);
    }
}