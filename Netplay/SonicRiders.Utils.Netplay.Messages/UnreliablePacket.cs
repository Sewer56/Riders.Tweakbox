﻿using System;
using System.Buffers;
using DotNext.Buffers;
using Reloaded.Memory.Pointers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.BitStream;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;
namespace Riders.Netplay.Messages;

[Equals(DoNotAddEqualityOperators = true)]
public struct UnreliablePacket : ISequenced, IPacket
{
    private static readonly ArrayPool<UnreliablePacketPlayer> _pool = ArrayPool<UnreliablePacketPlayer>.Shared;

    /*
        // Packet Format (Some Features Currently Unimplemented)

        // Data:
        // -> Header       (2 bytes)
        // -> Player Data  (All Optional)
    */

    /// <summary>
    /// Declares the fields present in the packet to be serialized/deserialized.
    /// </summary>
    public UnreliablePacketHeader Header;

    /// <summary>
    /// Contains all of the player data present in this structure.
    /// </summary>
    public UnreliablePacketPlayer[] Players { get; private set; }

    /// <summary>
    /// Constructs a packet to be sent over the unreliable channel.
    /// </summary>
    /// <param name="numPlayers">The number of players in this message.</param>
    /// <param name="sequenceNumber">Sequence number assigned to this packet.</param>
    /// <param name="data">The data to include in the player packets.</param>
    public UnreliablePacket(int numPlayers, int sequenceNumber = 0, HasData data = HasDataAll)
    {
        Header = new UnreliablePacketHeader((byte)numPlayers, sequenceNumber, data);
        Players = _pool.Rent(Constants.MaxNumberOfPlayers);
    }

    /// <summary>
    /// Constructs a packet to be sent over the unreliable channel.
    /// This overload uses a frame counter to determine what should be sent
    /// and should be used when upload bandwidth is constrained (Bad Internet + 7/8 player game)
    /// </summary>
    /// <param name="numPlayers">The number of players in this message.</param>
    /// <param name="sequenceNumber">Sequence number assigned to this packet.</param>
    /// <param name="frameCounter">The current frame counter.</param>
    public UnreliablePacket(int numPlayers, int sequenceNumber, int frameCounter)
    {
        Header = new UnreliablePacketHeader((byte)numPlayers, sequenceNumber, frameCounter);
        Players = _pool.Rent(Constants.MaxNumberOfPlayers);
    }

    /// <summary>
    /// Constructs a packet to be sent over the unreliable channel.
    /// </summary>
    /// <param name="player">
    ///     The individual player data associated with this packet to be sent.
    /// </param>
    /// <param name="data">The data to include in the player packets.</param>
    /// <param name="sequenceNumber">Sequence number assigned to this packet.</param>
    public UnreliablePacket(UnreliablePacketPlayer player, int sequenceNumber = 0, HasData data = HasDataAll)
    {
        Header = new UnreliablePacketHeader((byte)1, sequenceNumber, data);
        Players = _pool.Rent(Constants.MaxNumberOfPlayers);
        Players[0] = player;
    }

    public void Dispose()
    {
        if (Players != null)
            _pool.Return(Players);
    }

    /// <summary>
    /// Clones the contents of the current packet.
    /// Cloned packet will need separate disposal from original packet.
    /// </summary>
    public UnreliablePacket Clone()
    {
        var copy = new UnreliablePacket(Header.NumberOfPlayers, Header.SequenceNumber, Header.Fields) { Header = Header };
        for (int x = 0; x < copy.Header.NumberOfPlayers; x++)
            copy.Players[x] = Players[x];

        return copy;
    }

    /// <summary>
    /// Serializes the current instance of the packet.
    /// </summary>
    /// <param name="numBytes">Number of bytes serialized in the returned byte stream.</param>
    public ArrayRental<byte> Serialize(out int numBytes)
    {
        // Rent some bytes.
        var rental = new ArrayRental<byte>(2048);
        var rentalStream = new RentalByteStream(rental);
        var bitStream = new BitStream<RentalByteStream>(rentalStream);

        Header.Serialize(ref bitStream);
        for (int x = 0; x < Header.NumberOfPlayers; x++)
            Players[x].Serialize(ref bitStream, Header.Fields);

        numBytes = bitStream.NextByteIndex;
        return rental;
    }

    /// <summary>
    /// Serializes an instance of the packet.
    /// </summary>
    public unsafe void Deserialize(Span<byte> data)
    {
        fixed (byte* dataPtr = data)
        {
            var stream = new FixedPointerByteStream(new RefFixedArrayPtr<byte>(dataPtr, data.Length));
            var bitStream = new BitStream<FixedPointerByteStream>(stream);

            Header = UnreliablePacketHeader.Deserialize(ref bitStream);
            for (int x = 0; x < Header.NumberOfPlayers; x++)
                Players[x] = UnreliablePacketPlayer.Deserialize(ref bitStream, Header.Fields);
        }
    }

    /// <summary>
    /// Updates the value of the <see cref="Header"/>
    /// </summary>
    public void SetHeader(UnreliablePacketHeader value) => Header = value;

    /// <inheritdoc />
    public int MaxValue => SequenceNumberBitfield.MaxValue;

    /// <inheritdoc />
    public int SequenceNo => Header.SequenceNumber;
}
