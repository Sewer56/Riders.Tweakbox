using System;
using System.IO;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Unreliable;

namespace Riders.Netplay.Messages
{
    public class UnreliablePacket : IPacket<UnreliablePacket>, IPacket 
    {
        public PacketKind GetPacketType() => PacketKind.Unreliable;

        /*
            // Packet Format (Some Features Currently Unimplemented)
            // 30Hz indicates, send every 2nd frame. 
            // 10Hz indicates, send every 6th frame. etc.
            // See structs inside this struct for definitions.

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

        public UnreliablePacket() { }

        /// <summary>
        /// Constructs a packet to be sent over the unreliable channel.
        /// </summary>
        /// <param name="players">
        ///     The individual player data associated with this packet to be sent.
        ///     Should be of length 1 - 8.
        /// </param>
        public UnreliablePacket(UnreliablePacketPlayer[] players)
        {
            Header = new UnreliablePacketHeader(players);
            Players = players;
        }

        /// <summary>
        /// Serializes the current instance of the packet.
        /// </summary>
        public byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(1024);
            writer.Write(Header.Serialize());

            foreach (var player in Players)
                writer.Write(player.Serialize());

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
                using var bufferedStreamReader  = new BufferedStreamReader(unmanagedMemoryStream, 512);

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
