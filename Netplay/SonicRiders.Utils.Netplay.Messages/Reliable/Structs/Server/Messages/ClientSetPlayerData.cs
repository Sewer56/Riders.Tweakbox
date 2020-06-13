using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public class ClientSetPlayerData : IServerMessage
    {
        public ServerMessageType GetMessageType() => ServerMessageType.ClientSetPlayerData;

        public ClientSetPlayerData() { }
        public ClientSetPlayerData(HostPlayerData data) { Data = data; }

        [Key(0)]
        public HostPlayerData Data { get; set; }

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static ClientSetPlayerData FromBytes(BufferedStreamReader reader) => Utilities.DesrializeMessagePack<ClientSetPlayerData>(reader);
    }
}
