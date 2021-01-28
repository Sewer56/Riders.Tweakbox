using System.IO;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class ServerMessageTests
    {
        [Fact]
        public void SerializeArrayMessage()
        {
            var buffer  = new byte[1024];
            var message = new HostSetPlayerData(new []
            {
                new HostPlayerData() { Name = "Yacker", PlayerIndex = 0 },
                new HostPlayerData() { Name = "Xi Kykping", PlayerIndex = 1 },
                new HostPlayerData() { Name = "Pixel", PlayerIndex = 2 }
            }, 3);

            var serverMsg = new ServerMessage(message);
            var bytes = serverMsg.ToBytes(buffer);

            using var memoryStream = new MemoryStream(bytes.ToArray());
            using var streamReader = new BufferedStreamReader(memoryStream, bytes.Length);
            var command = ServerMessage.FromBytes(streamReader);

            Assert.Equal(message.GetMessageType(), command.MessageKind);
            Assert.IsType<ServerMessage>(command);
            Assert.IsType<HostSetPlayerData>(command.Message);

            var messageCopy = (HostSetPlayerData) command.Message;
            Assert.Equal(message, messageCopy);
        }

        [Fact]
        public void SerializeStandardMessage()
        {
            var buffer = new byte[1024];
            var message = new SetAntiCheat() { Cheats = CheatKind.RngManipulation };
            var serverMsg = new ServerMessage(message);
            var bytes = serverMsg.ToBytes(buffer);

            using var memoryStream = new MemoryStream(bytes.ToArray());
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
