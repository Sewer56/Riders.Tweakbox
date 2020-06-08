using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject]
    public struct HostSetPlayerData : IServerMessage
    {
        public ServerMessageType GetMessageType() => ServerMessageType.HostSetPlayerData;

        [Key(0)]
        public HostPlayerData[] Data { get; set; }

        public HostSetPlayerData(HostPlayerData[] data) => Data = data;

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static HostSetPlayerData FromBytes(BufferedStreamReader reader) => Utilities.DesrializeMessagePack<HostSetPlayerData>(reader);
    }
}
