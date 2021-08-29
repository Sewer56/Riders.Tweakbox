using K4os.Hash.xxHash;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay;

[Equals(DoNotAddEqualityOperators = true)]
public struct AntiCheatDataHash : IReliableMessage
{
    public ulong Hash;
    public AntiCheatDataHash(ulong hash) => Hash = hash;

    /// <summary>
    /// Gets a hash of the current game data.
    /// </summary>
    public static AntiCheatDataHash FromGame(string[] customGears = null)
    {
        using var data = GameData.FromGame(customGears);
        using var bytes = data.ToCompressedBytes(out int bytesWritten);

        return new AntiCheatDataHash(XXH64.DigestOf(bytes.Span.Slice(0, bytesWritten)));
    }

    /// <summary>
    /// Gets a hash of the current game data.
    /// </summary>
    /// <returns></returns>
    public static AntiCheatDataHash FromData(byte[] data) => new AntiCheatDataHash(XXH64.DigestOf(data, 0, data.Length));

    /// <summary>
    /// True if external hash matches our game, else false.
    /// </summary>
    public static bool Verify(AntiCheatDataHash external, string[] customGears = null) => external.Equals(FromGame(customGears));

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.AntiCheatDataHash;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Hash = bitStream.Read<ulong>();
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Hash);
    }
}
