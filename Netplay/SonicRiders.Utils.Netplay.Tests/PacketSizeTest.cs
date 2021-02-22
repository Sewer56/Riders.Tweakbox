using System.Collections.Generic;
using System.Linq;
using Riders.Netplay.Messages.Misc;
using Sewer56.SonicRiders.Utility;
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
            const int maxPlayers = 32;
            var originalBitField = Constants.PlayerCountBitfield;
            Constants.SetPlayerCountBitfield(new BitField(Misc.Utilities.GetMinimumNumberOfBits(maxPlayers)));

            for (int numPlayers = 2; numPlayers <= maxPlayers; numPlayers++)
            {
                var numPeers    = numPlayers - 1;
                var messageSize = new List<int>();
                var players     = Enumerable.Range(0, numPlayers).Select(x => Utilities.GetRandomPlayer()).ToArray();

                // 600 frame interval.
                for (int x = 0; x < 600; x++)
                {
                    using var packet = new Messages.UnreliablePacket(numPlayers, x, x);
                    for (int playerNo = 0; playerNo < numPlayers; playerNo++)
                        packet.Players[playerNo] = players[playerNo];

                    using var rental = packet.Serialize(out int numBytes);
                    messageSize.Add(numBytes);
                }
                
                var minMessage = messageSize.Min();
                var avgMessage = messageSize.Average();
                var maxMessage = messageSize.Max();

                // 28 = IP + UDP
                // 1  = LiteNetLib
                var headerSize = 28 + 1;

                _testOutputHelper.WriteLine($"Message Sizes:");
                _testOutputHelper.WriteLine($"UDP + Lib Packet Overhead: {headerSize} bytes");
                _testOutputHelper.WriteLine($"Min Size of Message: {minMessage} bytes");
                _testOutputHelper.WriteLine($"Avg Size of Message: {avgMessage} bytes");
                _testOutputHelper.WriteLine($"Max Size of Message: {maxMessage} bytes");

                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players) (Min): {ToKBitsInSecond(headerSize + (minMessage)) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players) (Avg): {ToKBitsInSecond(headerSize + (int)(avgMessage)) * numPeers}Kbit/s");
                _testOutputHelper.WriteLine($"Lobby ({numPlayers} Players) (Max): {ToKBitsInSecond(headerSize + (maxMessage)) * numPeers}Kbit/s");
            }

            Constants.SetPlayerCountBitfield(originalBitField);
        }

        private float ToKBitsInSecond(int structSize) => (int) (((structSize * 60.0f) * 8.0) / 1000.0f);
    }
}