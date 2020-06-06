using System.IO;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class ServerMessageTests
    {
        [Fact]
        public void SerializeArrayMessage()
        {
            var message = new SetPlayerNames(new string[]
            {
                "Yacker",
                "Xi Kykping",
                "Pixel"
            });

            var serverMsg = new ServerMessage(message);
            var bytes = serverMsg.ToBytes();

            using var memoryStream = new MemoryStream(bytes);
            using var streamReader = new BufferedStreamReader(memoryStream, bytes.Length);
            var command = ServerMessage.FromBytes(streamReader);

            Assert.Equal(message.GetMessageType(), command.MessageKind);
            Assert.IsType<ServerMessage>(command);
            Assert.IsType<SetPlayerNames>(command.Message);

            var messageCopy = (SetPlayerNames) command.Message;
            Assert.Equal(message, messageCopy);
        }

        [Fact]
        public void SerializeStandardMessage()
        {
            var message = new SetAntiCheat() { Cheats = CheatKind.RngManipulation };
            var serverMsg = new ServerMessage(message);
            var bytes = serverMsg.ToBytes();

            using var memoryStream = new MemoryStream(bytes);
            using var streamReader = new BufferedStreamReader(memoryStream, bytes.Length);
            var command = ServerMessage.FromBytes(streamReader);

            Assert.Equal(message.GetMessageType(), command.MessageKind);
            Assert.IsType<ServerMessage>(command);
            Assert.IsType<SetAntiCheat>(command.Message);

            var messageCopy = (SetAntiCheat) command.Message;
            Assert.Equal(message, messageCopy);
        }
    }
}
