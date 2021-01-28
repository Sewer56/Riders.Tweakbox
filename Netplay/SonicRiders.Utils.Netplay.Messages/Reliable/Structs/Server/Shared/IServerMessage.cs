using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Shared
{
    public interface IServerMessage
    {
        ServerMessageType GetMessageType();
        Span<byte> ToBytes(Span<byte> buffer);
    }
}