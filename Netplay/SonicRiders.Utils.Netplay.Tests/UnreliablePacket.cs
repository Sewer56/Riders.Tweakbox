using System.Numerics;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.Structures.Enums;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class UnreliablePacket
    {
        [Fact]
        public void SerializeUnreliablePacketAllData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var rings     = (byte) 52;
            var state     = PlayerState.Turbulence;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;

            var player            = new UnreliablePacketPlayer(position, rings, state, rotation, velocityX, velocityY);
            var unreliablePacket  = new Messages.UnreliablePacket(new[] { player });

            var bytes        = unreliablePacket.Serialize();
            var deserialized = Messages.UnreliablePacket.Deserialize(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }

        [Fact]
        public void SerializeUnreliablePacketPartialData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;

            var player = new UnreliablePacketPlayer(position, null, null, rotation, velocityX, velocityY);
            var unreliablePacket = new Messages.UnreliablePacket(new[] { player });

            var bytes = unreliablePacket.Serialize();
            var deserialized = Messages.UnreliablePacket.Deserialize(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }
    }
}
