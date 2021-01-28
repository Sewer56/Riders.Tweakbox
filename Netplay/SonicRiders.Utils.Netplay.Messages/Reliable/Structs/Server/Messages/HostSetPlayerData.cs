using System;
using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject]
    public struct HostSetPlayerData : IServerMessage
    {
        public ServerMessageType GetMessageType() => ServerMessageType.HostSetPlayerData;

        /// <summary>
        /// Contains indexes and names of all other players.
        /// </summary>
        [Key(0)]
        public HostPlayerData[] Data { get; set; }

        /// <summary>
        /// Index of the player receiving the message.
        /// </summary>
        [Key(1)]
        public int Index { get; set; }

        public HostSetPlayerData(HostPlayerData[] data, int index)
        {
            Data = data;
            Index = index;
        }

        public Span<byte> ToBytes(Span<byte> buffer) => MessagePackSerializer.Serialize(this);
        public static HostSetPlayerData FromBytes(BufferedStreamReader reader) => Utilities.DeserializeMessagePack<HostSetPlayerData>(reader);
    }
}
