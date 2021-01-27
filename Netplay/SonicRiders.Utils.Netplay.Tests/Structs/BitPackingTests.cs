using System.IO;
using EnumsNET;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class BitPackingTests
    {
        [Fact]
        public void SerializeFirstElement()
        {
            var expected = new MovementFlagsMsg(MovementFlags.Boost | MovementFlags.Braking | MovementFlags.Drifting);
            var packed   = new MovementFlagsPacked().AsInterface().Create(new[] { expected });

            var serialized   = packed.AsInterface().Serialize();
            using var stream = new MemoryStream(serialized);
            using var reader = new BufferedStreamReader(stream, 2048);
            var deserialized = packed.AsInterface().Deserialize(reader);

            var actual = deserialized.Elements[0];
            Assert.Equal(packed.NumElements, deserialized.NumElements);
            Assert.Equal(expected.Modes, actual.Modes);
        }

        [Fact]
        public void SerializeLastElement()
        {
            var expected      = new MovementFlagsMsg(MovementFlags.Drifting | MovementFlags.Boost | MovementFlags.Down);
            var data          = new MovementFlagsMsg[Constants.MaxNumberOfPlayers];
            var lastElement   = Constants.MaxNumberOfPlayers - 1;
            data[lastElement] = expected;
            var packed        = new MovementFlagsPacked().AsInterface().Create(data);
            
            var serialized   = packed.AsInterface().Serialize();
            using var stream = new MemoryStream(serialized);
            using var reader = new BufferedStreamReader(stream, 2048);
            var deserialized = packed.AsInterface().Deserialize(reader);

            var playerData = deserialized.Elements[lastElement];
            Assert.Equal(packed.NumElements, deserialized.NumElements);
            Assert.Equal(expected.Modes, playerData.Modes);
        }

        [Fact]
        public void SerializeAllElements()
        {
            var packed = new MovementFlagsPacked().AsInterface().Create(new MovementFlagsMsg[Constants.MaxNumberOfPlayers]);
            for (int x = 0; x < Constants.MaxNumberOfPlayers; x++)
            {
                var expected        = new MovementFlagsMsg(MovementFlags.Braking | MovementFlags.Drifting);
                packed.Elements[x]  = expected;
            }

            var serialized  = packed.AsInterface().Serialize();
            using var stream = new MemoryStream(serialized);
            using var reader = new BufferedStreamReader(stream, 2048);
            var deserialized = packed.AsInterface().Deserialize(reader);

            Assert.Equal(packed.NumElements, deserialized.NumElements);
            for (int x = 0; x < Constants.MaxNumberOfPlayers; x++)
                Assert.Equal(packed.Elements[x].Modes, deserialized.Elements[x].Modes);
        }

        [Fact]
        public void SerializeAllAttacks()
        {
            var attacks = new SetAttack[Constants.MaxNumberOfPlayers];
            for (int x = 0; x < Constants.MaxNumberOfPlayers - 1; x++)
                attacks[x] = new SetAttack((byte) (x + 1));

            var packed  = new AttackPacked().AsInterface().Create(attacks);

            var serialized   = packed.AsInterface().Serialize();
            using var stream = new MemoryStream(serialized);
            using var reader = new BufferedStreamReader(stream, 2048);
            var deserialized = packed.AsInterface().Deserialize(reader);

            Assert.Equal(packed.NumElements, deserialized.NumElements);
            for (int x = 0; x < Constants.MaxNumberOfPlayers; x++)
                Assert.Equal(packed.Elements[x], deserialized.Elements[x]);
        }
    }
}
