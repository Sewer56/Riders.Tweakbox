using System.Buffers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Server;

public struct HostSetPlayerData : IReliableMessage
{
    private static ArrayPool<ClientData> _pool = ArrayPool<ClientData>.Shared;

    /// <summary>
    /// Contains indexes and names of all other players.
    /// </summary>
    public ClientData[] Data { get; set; }

    /// <summary>
    /// Index of the player (host side) receiving the message.
    /// </summary>
    public int PlayerIndex { get; set; }

    /// <summary>
    /// Index of the client (host side) receiving the message.
    /// </summary>
    public int ClientIndex { get; set; }

    /// <summary>
    /// Number of elements in the <see cref="Data"/> array.
    /// </summary>
    public int NumElements { get; private set; }

    private bool _isPooled;

    public HostSetPlayerData(ClientData[] data, int playerIndex, int clientIndex)
    {
        Data = data;
        PlayerIndex = playerIndex;
        ClientIndex = clientIndex;

        NumElements = Data.Length;
        _isPooled = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isPooled)
        {
            _pool.Return(Data);
        }
    }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.HostSetPlayerData;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        // Our own created elements use the pool.
        _isPooled = true;

        NumElements = bitStream.Read<byte>(Constants.MaxNumberOfClientsBitField.NumBits) + 1;
        PlayerIndex = bitStream.Read<byte>(Constants.PlayerCountBitfield.NumBits);
        ClientIndex = bitStream.Read<byte>(Constants.MaxNumberOfClientsBitField.NumBits);
        Data = _pool.Rent(NumElements);
        for (int x = 0; x < NumElements; x++)
        {
            var data = new ClientData();
            data.FromStream(ref bitStream);
            Data[x] = data;
        }
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(NumElements - 1, Constants.MaxNumberOfClientsBitField.NumBits);
        bitStream.Write(PlayerIndex, Constants.PlayerCountBitfield.NumBits);
        bitStream.Write(ClientIndex, Constants.MaxNumberOfClientsBitField.NumBits);

        for (int x = 0; x < NumElements; x++)
            Data[x].ToStream(ref bitStream);
    }
}
