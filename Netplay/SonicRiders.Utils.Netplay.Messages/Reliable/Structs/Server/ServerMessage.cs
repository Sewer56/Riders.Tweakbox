using System;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server
{
    public struct ServerMessage
    {
        public ServerMessageType MessageKind;
        public IServerMessage Message;

        public ServerMessage(IServerMessage message)
        {
            MessageKind = message.GetMessageType();
            Message = message;
        }

        public ServerMessage(IServerMessage message, ServerMessageType messageKind)
        {
            MessageKind = messageKind;
            Message = message;
        }

        /// <summary>
        /// Converts the synchronization command to a set of bytes.
        /// </summary>
        public byte[] ToBytes()
        {
            using var extendedMemoryStream = new ExtendedMemoryStream();
            extendedMemoryStream.Write(MessageKind);
            extendedMemoryStream.Write(Message.ToBytes());
            return extendedMemoryStream.ToArray();
        }

        /// <param name="reader">The stream reader for the current packet.</param>
        public static ServerMessage FromBytes(BufferedStreamReader reader)
        {
            var message = new ServerMessage();
            message.MessageKind = reader.Read<ServerMessageType>();

            switch (message.MessageKind)
            {
                case ServerMessageType.ClientSetPlayerData:
                    message.Message = ClientSetPlayerData.FromBytes(reader);
                    break;
                case ServerMessageType.HostSetPlayerData:
                    message.Message = HostSetPlayerData.FromBytes(reader);
                    break;
                case ServerMessageType.HasSetAntiCheatTypes:
                    message.Message = reader.Read<SetAntiCheat>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return message;
        }
    }
}
