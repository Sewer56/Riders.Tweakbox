using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server
{
    public struct ServerMessage
    {
        public ServerMessageType MessageKind;
        public IServerMessage Message;

        public ServerMessage(IServerMessage message, ServerMessageType messageKind)
        {
            MessageKind = messageKind;
            Message = message;
        }

        /// <summary>
        /// Converts the synchronization command to a set of bytes.
        /// </summary>
        /// <param name="command">The command.</param>
        public static byte[] ToBytes<T>(T command) where T : IServerMessage
        {
            using var extendedMemoryStream = new ExtendedMemoryStream();
            extendedMemoryStream.Write(command.GetMessageType());
            extendedMemoryStream.Write(command.ToBytes());
            return extendedMemoryStream.ToArray();
        }

        /// <param name="reader">The stream reader for the current packet.</param>
        public static ServerMessage FromBytes(BufferedStreamReader reader)
        {
            var message = new ServerMessage();
            message.MessageKind = reader.Read<ServerMessageType>();

            switch (message.MessageKind)
            {
                case ServerMessageType.ClientSetPlayerName:
                    message.Message = SetPlayerName.FromBytes(reader);
                    break;
                case ServerMessageType.HostSetPlayerNames:
                    message.Message = SetPlayerNames.FromBytes(reader);
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
