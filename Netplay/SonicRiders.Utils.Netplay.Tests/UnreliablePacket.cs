using System.Linq;
using System.Numerics;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.Structures.Enums;
using Xunit;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;

namespace Riders.Netplay.Messages.Tests
{
    public class UnreliablePacket
    {
        [Fact]
        public void SerializeUnreliablePacketTwoPlayers()
        {
            var random0 = Utilities.GetRandomPlayer();
            var random1 = Utilities.GetRandomPlayer();
            var unreliablePacket = new Messages.UnreliablePacket(new[] { random0, random1 });

            var bytes = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
            Assert.Equal(unreliablePacket.Players[1], deserialized.Players[1]);
        }

        [Fact]
        public void SerializeUnreliablePacketFiveSeconds()
        {
            for (int x = 0; x < 300; x++)
            {
                var random0 = Utilities.GetRandomPlayer();
                var unreliablePacket = new Messages.UnreliablePacket(new[] { random0 });

                var bytes = unreliablePacket.Serialize();
                var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

                Assert.Equal(unreliablePacket.Header, deserialized.Header);
                Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
            }
        }

        [Fact]
        public void SerializeUnreliablePacketManyPlayers()
        {
            for (int x = 0; x < 60; x++)
            {
                for (int y = 1; y < Constants.MaxNumberOfPlayers; y++)
                {
                    var randomPlayers = Enumerable.Range(0, y).Select(x => Utilities.GetRandomPlayer()).ToArray();
                    var unreliablePacket = new Messages.UnreliablePacket(randomPlayers);

                    var bytes = unreliablePacket.Serialize();
                    var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

                    Assert.Equal(unreliablePacket.Header, deserialized.Header);
                    for (int z = 0; z < y; z++)
                    {
                        Assert.Equal(unreliablePacket.Players[z], deserialized.Players[z]);
                    }
                }
            }
        }

        [Fact]
        public void SerializeUnreliablePacketPartialData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;
            var hasFlags  = HasData.HasPosition | HasData.HasRotation | HasData.HasVelocity;

            var player = new UnreliablePacketPlayer(position, null, null, null, null, rotation, new Vector2(velocityX, velocityY));
            var unreliablePacket = new Messages.UnreliablePacket(new[] { player }, hasFlags);

            var bytes = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }


    }
}
