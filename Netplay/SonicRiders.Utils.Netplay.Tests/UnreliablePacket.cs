using Riders.Netplay.Messages.Misc;
using Xunit;
using Xunit.Abstractions;

namespace Riders.Netplay.Messages.Tests
{
    public class UnreliablePacket
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnreliablePacket(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TwoPlayers()
        {
            using var unreliablePacket = new Messages.UnreliablePacket(2);
            unreliablePacket.Players[0] = Utilities.GetRandomPlayer();
            unreliablePacket.Players[1] = Utilities.GetRandomPlayer();

            // Serialize
            using var bytes = unreliablePacket.Serialize(out int numBytes);
            
            // Deserialize.
            using var newPacket = new Messages.UnreliablePacket(Constants.MaxNumberOfPlayers);
            newPacket.Deserialize(bytes.Span.Slice(0, numBytes));

            Assert.Equal(unreliablePacket.Header, newPacket.Header);
            Assert.Equal(unreliablePacket.Players[0], newPacket.Players[0]);
            Assert.Equal(unreliablePacket.Players[1], newPacket.Players[1]);
        }

        [Fact]
        public void ManyPlayers()
        {
            for (int numFrame = 0; numFrame < 60; numFrame++)
            {
                for (int maxPlayers = 1; maxPlayers < Constants.MaxNumberOfPlayers; maxPlayers++)
                {
                    using var unreliablePacket = new Messages.UnreliablePacket(maxPlayers);
                    for (int z = 0; z < maxPlayers; z++)
                        unreliablePacket.Players[z] = Utilities.GetRandomPlayer();

                    // Serialize
                    using var bytes = unreliablePacket.Serialize(out int numBytes);
                    _testOutputHelper.WriteLine($"Players: {maxPlayers} Bytes: {numBytes}");

                    // Deserialize.
                    using var newPacket = new Messages.UnreliablePacket(Constants.MaxNumberOfPlayers);
                    newPacket.Deserialize(bytes.Span.Slice(0, numBytes));

                    Assert.Equal(unreliablePacket.Header, newPacket.Header);
                    for (int z = 0; z < maxPlayers; z++)
                        Assert.Equal(unreliablePacket.Players[z], newPacket.Players[z]);
                }
            }
        }
    }
}
