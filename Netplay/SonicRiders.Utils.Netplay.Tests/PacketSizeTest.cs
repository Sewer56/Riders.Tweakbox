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
        public unsafe void PlayerPacket()
        {
            var player = UnreliablePacketPlayer.GetRandom(0);
            var playerSerialized = player.Serialize();

            // 28 = IP + UDP
            // 1  = LiteNetLib
            int numberOfPeers = 7;
            var headerSize    = 28 + 1 + new UnreliablePacketHeader().Serialize().Length;
            var playerSize    = playerSerialized.Length;
            var allPlayerSize = playerSize * numberOfPeers;

            _testOutputHelper.WriteLine($"Message Sizes:");
            _testOutputHelper.WriteLine($"Packet Overhead: {headerSize} bytes");
            _testOutputHelper.WriteLine($"Size of Player: {playerSize} bytes");
            _testOutputHelper.WriteLine($"Size of all Players: {allPlayerSize} bytes");
            _testOutputHelper.WriteLine($"Kbits Per Second ({numberOfPeers} Players + Headers): {ToKBitsInSecond(headerSize + allPlayerSize)}");

            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine($"Transmission Upload Costs:");
            _testOutputHelper.WriteLine($"Full Lobby (Packet Overhead Only): {ToKBitsInSecond(headerSize) * numberOfPeers}Kbit/s");
            _testOutputHelper.WriteLine($"Full Lobby ({numberOfPeers + 1} Players): {ToKBitsInSecond(headerSize + allPlayerSize) * numberOfPeers}Kbit/s");
        }

        private float ToKBitsInSecond(int structSize) => (int) (((structSize * 60.0f) * 8.0) / 1000.0f);
    }
}