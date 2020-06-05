using System;
using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    [MessagePackObject]
    public struct SetPlayerNames : IServerMessage, IEquatable<SetPlayerNames>
    {
        public ServerMessageType GetMessageType() => ServerMessageType.HostSetPlayerNames;

        [Key(0)]
        public string[] Names { get; set; }

        public SetPlayerNames(string[] names)
        {
            Names = names;
        }

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static SetPlayerNames FromBytes(BufferedStreamReader reader) => Utilities.DesrializeMessagePack<SetPlayerNames>(reader);

        #region Autoimplemented
        public bool Equals(SetPlayerNames other)
        {
            if (Names.Length == other.Names.Length)
            {
                for (int x = 0; x < Names.Length; x++)
                {
                    if (!Names[x].Equals(other.Names[x]))
                        return false;
                }

                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is SetPlayerNames other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Names != null ? Names.GetHashCode() : 0);
        }
        #endregion
    }
}
