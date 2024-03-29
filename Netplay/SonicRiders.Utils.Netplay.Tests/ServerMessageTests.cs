﻿using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Xunit;
namespace Riders.Netplay.Messages.Tests;

public class ServerMessageTests
{
    [Fact]
    public void SerializePlayerData()
    {
        // Make Message
        var message = new HostSetPlayerData(new[]
        {
                new ClientData() { Name = "Yacker", PlayerIndex = 0, ClientIndex = 0 },
                new ClientData() { Name = "Xi Kykping", PlayerIndex = 1, ClientIndex = 1 },
                new ClientData() { Name = "Pixel", PlayerIndex = 2, ClientIndex = 2 }
            }, 3, 3);

        using var packet = ReliablePacket.Create(message);

        // Serialize
        using var messageBytes = packet.Serialize(out int numBytes);

        // Deserialize
        using var newPacket = new ReliablePacket();
        newPacket.Deserialize(messageBytes.Span.Slice(0, numBytes));

        Assert.Equal(message.GetMessageType(), newPacket.MessageType);
        Assert.Equal(packet.MessageType, newPacket.MessageType);

        var original = packet.GetMessage<HostSetPlayerData>();
        var other = newPacket.GetMessage<HostSetPlayerData>();

        Assert.Equal(original.NumElements, other.NumElements);
        for (int x = 0; x < original.NumElements; x++)
            Assert.Equal(original.Data[x], other.Data[x]);
    }
}
