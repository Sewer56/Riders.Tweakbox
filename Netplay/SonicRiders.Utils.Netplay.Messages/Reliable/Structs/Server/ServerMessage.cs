using System;
using System.Buffers;
using System.IO;
using DotNext.Buffers;
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
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <returns>A sliced version of the buffer.</returns>
        public unsafe Span<byte> ToBytes(Span<byte> buffer)
        {
            using var rental = new ArrayRental<byte>(256);
            fixed (byte* bytePtr = buffer)
            {
                using var unmanagedStream = new UnmanagedMemoryStream(bytePtr, buffer.Length, buffer.Length, FileAccess.Write);
                unmanagedStream.Write(MessageKind);
                unmanagedStream.Write(Message.ToBytes(rental.Span));
                return buffer.Slice(0, (int)unmanagedStream.Position);
            }
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
                case ServerMessageType.HostUpdateClientPing:
                    message.Message = HostUpdateLatency.FromBytes(reader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return message;
        }
    }
}
