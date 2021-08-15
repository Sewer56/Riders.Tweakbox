using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;
using Xunit;
namespace Riders.Netplay.Messages.Tests;

public class ReliableMenuTests
{
    [Fact]
    public void SerializeStandardMessage()
    {
        // Make Message
        var message = new CharaSelectLoop(0, 5, PlayerStatus.GearSelect);
        using var packet = ReliablePacket.Create(message);

        // Serialize
        using var messageBytes = packet.Serialize(out int numBytes);

        // Deserialize
        using var newPacket = new ReliablePacket();
        newPacket.Deserialize(messageBytes.Span.Slice(0, numBytes));

        Assert.Equal(message.GetMessageType(), newPacket.MessageType);
        Assert.Equal(packet.MessageType, newPacket.MessageType);
        Assert.Equal(message, newPacket.GetMessage<CharaSelectLoop>());
    }

    [Fact]
    public void SerializePacked()
    {
        // Make Message
        var message = new CharaSelectSync(new CharaSelectLoop[4]
        {
                new CharaSelectLoop(0, 1, PlayerStatus.Active),
                new CharaSelectLoop(1, 2, PlayerStatus.GearSelect),
                new CharaSelectLoop(2, 3, PlayerStatus.Inactive),
                new CharaSelectLoop(3, 4, PlayerStatus.SetReady)
        });

        using var packet = ReliablePacket.Create(message);

        // Serialize
        using var messageBytes = packet.Serialize(out int numBytes);

        // Deserialize
        using var newPacket = new ReliablePacket();
        newPacket.Deserialize(messageBytes.Span.Slice(0, numBytes));

        Assert.Equal(message.GetMessageType(), newPacket.MessageType);
        Assert.Equal(packet.MessageType, newPacket.MessageType);

        var messageCopy = newPacket.GetMessage<CharaSelectSync>();
        Assert.Equal(message.NumElements, messageCopy.NumElements);

        for (int x = 0; x < message.NumElements; x++)
            Assert.Equal(message.Elements[x], messageCopy.Elements[x]);
    }
}
