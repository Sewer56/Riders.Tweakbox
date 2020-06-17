using System;
using System.Numerics;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.Structures.Enums;
using Xunit;
using Xunit.Abstractions;

namespace Riders.Netplay.Messages.Tests
{
    /// <summary>
    /// Not actual tests, used for checking packet sizes.
    /// </summary>
    public class PacketSizeTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public PacketSizeTest(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [Fact]
        public unsafe void PlayerPacket()
        {
            var position    = new Vector3(-49.85133362f, -41.55332947f, 167.2761993f);
            var rotation    = 0.03664770722f;
            var rings       = (byte)52;
            var state     = PlayerState.Turbulence;
            var velocityX = 0.7286906838f;
            var velocityY = 0.123456789f;

            var player           = new UnreliablePacketPlayer(position, rings, state, rotation, new Vector2(velocityX, velocityY));
            var playerSerialized = player.Serialize();

            // 28 = IP + UDP
            // 1  = LiteNetLib
            var headerSize    = 28 + 1 + new UnreliablePacketHeader().Serialize().Length;
            var playerSize    = playerSerialized.Length;
            var allPlayerSize = playerSize * Constants.MaxNumberOfPeers;

            _testOutputHelper.WriteLine($"Message Sizes:");
            _testOutputHelper.WriteLine($"Packet Overhead: {headerSize} bytes");
            _testOutputHelper.WriteLine($"Size of Player: {playerSize} bytes");
            _testOutputHelper.WriteLine($"Kbits Per Second ({Constants.MaxNumberOfPeers} Players + Headers): {ToKBitsInSecond(headerSize + allPlayerSize)}");

            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine($"Transmission Upload Costs:");
            _testOutputHelper.WriteLine($"Full Lobby (Packet Overhead Only): {ToKBitsInSecond(headerSize) * Constants.MaxNumberOfPeers}Kbit/s");
            _testOutputHelper.WriteLine($"Full Lobby ({Constants.MaxNumberOfPlayers} Players): {ToKBitsInSecond(headerSize + allPlayerSize) * Constants.MaxNumberOfPeers}Kbit/s");
        }

        private float ToKBitsInSecond(int structSize) => (int) (((structSize * 60.0f) * 8.0) / 1000.0f);
    }
}