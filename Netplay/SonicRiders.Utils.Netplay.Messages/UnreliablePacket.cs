using System;
using System.IO;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Unreliable;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;

namespace Riders.Netplay.Messages
{
    public struct UnreliablePacket : IPacket<UnreliablePacket>, IPacket 
    {
        public PacketKind GetPacketType() => PacketKind.Unreliable;

        /*
            // Packet Format (Some Features Currently Unimplemented)
        
            // Data:
            // -> Header       (2 bytes)
            // -> Player Data  (All Optional)
        */

        /// <summary>
        /// Declares the fields present in the packet to be serialized/deserialized.
        /// </summary>
        public UnreliablePacketHeader Header { get; private set; }

        /// <summary>
        /// Contains all of the player data present in this structure.
        /// </summary>
        public UnreliablePacketPlayer[] Players { get; private set; }

        /// <summary>
        /// Constructs a packet to be sent over the unreliable channel.
        /// </summary>
        /// <param name="players">
        ///     The individual player data associated with this packet to be sent.
        /// </param>
        /// <param name="data">The data to include in the player packets.</param>
        public UnreliablePacket(UnreliablePacketPlayer[] players, HasData data = HasData.All)
        {
            Header = new UnreliablePacketHeader(players, data);
            Players = players;
        }

        /// <summary>
        /// Constructs a packet to be sent over the unreliable channel.
        /// This overload uses a frame counter to determine what should be sent
        /// and should be used when upload bandwidth is constrained (Bad Internet + 7/8 player game)
        /// </summary>
        /// <param name="players">
        ///     The individual player data associated with this packet to be sent.
        /// </param>
        /// <param name="frameCounter">The current frame counter.</param>
        public UnreliablePacket(UnreliablePacketPlayer[] players, int frameCounter)
        {
            Header = new UnreliablePacketHeader(players, frameCounter);
            Players = players;
        }

        /// <summary>
        /// Constructs a packet to be sent over the unreliable channel.
        /// </summary>
        /// <param name="player">
        ///     The individual player data associated with this packet to be sent.
        /// </param>
        /// <param name="data">The data to include in the player packets.</param>
        public UnreliablePacket(UnreliablePacketPlayer player, HasData data = HasData.All)
        {
            Players = new[] {player};
            Header = new UnreliablePacketHeader(Players, data);
        }

        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        public byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(1280);
            writer.Write(Header.Serialize());

            foreach (var player in Players)
                writer.Write(player.Serialize(Header.Fields));

            return writer.ToArray();
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public unsafe void Deserialize(Span<byte> data)
        {
            fixed (byte* dataPtr = data)
            {
                using var unmanagedMemoryStream = new UnmanagedMemoryStream(dataPtr, data.Length);
                using var bufferedStreamReader  = new BufferedStreamReader(unmanagedMemoryStream, 1280);

                var header  = UnreliablePacketHeader.Deserialize(bufferedStreamReader);
                var players = new UnreliablePacketPlayer[header.NumberOfPlayers];

                for (int x = 0; x < header.NumberOfPlayers; x++)
                    players[x] = UnreliablePacketPlayer.Deserialize(bufferedStreamReader, header.Fields);

                Header = header;
                Players = players;
            }
        }
    }
}
