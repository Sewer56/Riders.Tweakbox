using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject]
    public struct HostUpdateLatency : IServerMessage
    {
        public ServerMessageType GetMessageType() => ServerMessageType.HostUpdateClientPing;

        /// <summary>
        /// Contains latency numbers for each client.
        /// </summary>
        [Key(0)]
        public short[] Data { get; set; }

        public HostUpdateLatency(short[] data)
        {
            Data = data;
        }

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static HostUpdateLatency FromBytes(BufferedStreamReader reader) => Utilities.DeserializeMessagePack<HostUpdateLatency>(reader);
    }
}