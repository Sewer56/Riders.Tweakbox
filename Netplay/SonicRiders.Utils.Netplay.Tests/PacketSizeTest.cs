using System.Collections.Generic;
using System.Linq;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Unreliable;
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
        public unsafe void MovementFlagsPacket()
        {
            // 28 = IP + UDP
            // 4  = LiteNetLib
            var headerSize = 28 + 4 + sizeof(ReliablePacket.HasData);
            var flagsPackedSize = new MovementFlagsPacked().GetBufferSize();

            _testOutputHelper.WriteLine($"Message Sizes:");
            _testOutputHelper.WriteLine($"Packet Overhead: {headerSize} bytes");
            _testOutputHelper.WriteLine($"Transmission Upload Costs:");

            // Players
            for (int x = 2; x <= 8; x++)
            {
                var numPeers = x - 1;
                _testOutputHelper.WriteLine("----------");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Min): {ToKBitsInSecond(headerSize + flagsPackedSize) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Avg): {ToKBitsInSecond(headerSize + flagsPackedSize) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Max): {ToKBitsInSecond(headerSize + flagsPackedSize) * numPeers}Kbit/s");
            }
        }

        [Fact]
        public unsafe void PlayerPacket()
        {
            var player            = Utilities.GetRandomPlayer();
            var playerPacketSizes = new List<int>();
            var players           = new UnreliablePacketPlayer[] { player };

            // 600 frame interval.
            for (int x = 0; x < 600; x++)
            {
                var header           = new UnreliablePacketHeader(players, x);
                var fields           = header.Fields;
                playerPacketSizes.Add(player.Serialize(fields).Length);
            }

            var minPlayer = playerPacketSizes.Min();
            var avgPlayer = playerPacketSizes.Average();
            var maxPlayer = playerPacketSizes.Max();

            // 28 = IP + UDP
            // 1  = LiteNetLib
            var headerSize    = 28 + 1 + new UnreliablePacketHeader().Serialize().Length;

            _testOutputHelper.WriteLine($"Message Sizes:");
            _testOutputHelper.WriteLine($"Packet Overhead: {headerSize} bytes");
            _testOutputHelper.WriteLine($"Min Size of Player: {minPlayer} bytes");
            _testOutputHelper.WriteLine($"Avg Size of Player: {avgPlayer} bytes");
            _testOutputHelper.WriteLine($"Max Size of Player: {maxPlayer} bytes");
            
            _testOutputHelper.WriteLine($"Size of 8 Players (Min): {minPlayer * 8} bytes");
            _testOutputHelper.WriteLine($"Size of 8 Players (Avg): {avgPlayer * 8} bytes");
            _testOutputHelper.WriteLine($"Size of 8 Players (Max): {maxPlayer * 8} bytes");

            _testOutputHelper.WriteLine($"Size of 32 Players (Min): {minPlayer * 32} bytes");
            _testOutputHelper.WriteLine($"Size of 32 Players (Avg): {avgPlayer * 32} bytes");
            _testOutputHelper.WriteLine($"Size of 32 Players (Max): {maxPlayer * 32} bytes");

            _testOutputHelper.WriteLine($"Transmission Upload Costs:");
            
            for (int x = 2; x <= 32; x++)
            {
                var numPeers = x - 1;
                _testOutputHelper.WriteLine("----------");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Min): {ToKBitsInSecond(headerSize + (minPlayer * numPeers)) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Avg): {ToKBitsInSecond(headerSize + (int)(avgPlayer * numPeers)) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({x} Players) (Max): {ToKBitsInSecond(headerSize + (maxPlayer * numPeers)) * numPeers}Kbit/s");
            }

            // Spectators
            for (int x = 1; x <= 8; x++)
            {
                var numSpectators = x;
                var numPlayers    = 8;
                var numPeers      = (numPlayers - 1);

                var playerBytesMin = ToKBitsInSecond(headerSize + (minPlayer * numPeers)) * numPeers;
                var playerBytesAvg = ToKBitsInSecond((int) (headerSize + (avgPlayer * numPeers))) * numPeers;
                var playerBytesMax = ToKBitsInSecond(headerSize + (maxPlayer * numPeers)) * numPeers;

                var spectatorBytesMin = ToKBitsInSecond(headerSize + (minPlayer * numPlayers)) * numSpectators;
                var spectatorBytesAvg = ToKBitsInSecond((int) (headerSize + (avgPlayer * numPlayers))) * numSpectators;
                var spectatorBytesMax = ToKBitsInSecond(headerSize + (maxPlayer * numPlayers)) * numSpectators;

                _testOutputHelper.WriteLine("----------");
                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players, {x} Spectators) (Min): {playerBytesMin + spectatorBytesMin}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players, {x} Spectators) (Avg): {playerBytesAvg + spectatorBytesAvg}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players, {x} Spectators) (Max): {playerBytesMax + spectatorBytesMax}Kbit/s");
            }
        }

        private float ToKBitsInSecond(int structSize) => (int) (((structSize * 60.0f) * 8.0) / 1000.0f);
    }
}