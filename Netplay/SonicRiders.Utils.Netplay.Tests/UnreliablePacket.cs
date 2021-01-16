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
            var rings     = (byte) 99;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;
            var air       = (uint) 199999;

            var player            = new UnreliablePacketPlayer(position, air, rings, PlayerState.Retire, PlayerState.ElectricShockCrash, rotation, new Vector2(velocityX, velocityY));
            player.Animation      = (byte?) CharacterAnimation.AfterShock;
            player.LastAnimation  = (byte?) CharacterAnimation.GeneralCustomAttack3Loop;
            var unreliablePacket  = new Messages.UnreliablePacket(new[] { player });

            var bytes        = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }

        [Fact]
        public void SerializeUnreliablePacketMultiplePlayers()
        {
            var random0 = UnreliablePacketPlayer.GetRandom(0);
            var random1 = UnreliablePacketPlayer.GetRandom(0);
            var unreliablePacket = new Messages.UnreliablePacket(new[] { random0, random1 });

            var bytes = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
            Assert.Equal(unreliablePacket.Players[1], deserialized.Players[1]);
        }

        [Fact]
        public void SerializeUnreliablePacketBitOffset0()
        {
            var random0 = UnreliablePacketPlayer.GetRandom(0);
            var random1 = UnreliablePacketPlayer.GetRandom(0);
            random1.LastState = null;
            random0.LastState = null;
            random1.State = null;
            random0.State = null;
            random1.Rings = null;
            random0.Rings = null;
            random0.Air = null;
            random1.Air = null;
            random0.ControlFlags = 0;
            random1.ControlFlags = 0;
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
                var random0 = UnreliablePacketPlayer.GetRandom(x);
                var unreliablePacket = new Messages.UnreliablePacket(new[] { random0 });

                var bytes = unreliablePacket.Serialize();
                var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

                Assert.Equal(unreliablePacket.Header, deserialized.Header);
                Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
            }
        }

        [Fact]
        public void SerializeUnreliablePacketPartialData()
        {
            var position  = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation  = 0.03664770722f;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;

            var player = new UnreliablePacketPlayer(position, 0, null, default, default, rotation, new Vector2(velocityX, velocityY));
            var unreliablePacket = new Messages.UnreliablePacket(new[] { player });

            var bytes = unreliablePacket.Serialize();
            var deserialized = IPacket<Messages.UnreliablePacket>.FromSpan(bytes);

            Assert.Equal(unreliablePacket.Header, deserialized.Header);
            Assert.Equal(unreliablePacket.Players[0], deserialized.Players[0]);
        }
    }
}
