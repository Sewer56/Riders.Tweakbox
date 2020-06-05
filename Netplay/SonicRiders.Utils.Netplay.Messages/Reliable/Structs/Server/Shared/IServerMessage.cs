namespace Riders.Netplay.Messages.Reliable.Structs.Server.Shared
{
    public interface IServerMessage
    {
        ServerMessageType GetMessageType();
        byte[] ToBytes();
    }
}