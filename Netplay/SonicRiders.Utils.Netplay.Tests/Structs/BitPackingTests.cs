using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class BitPackingTests
    {
        [Fact]
        public void MovementSerializeFirst()
        {
            var packed = new MovementFlagsPacked().AsInterface();
            var expected = new MovementFlagsMsg(MovementFlags.Boost | MovementFlags.Braking | MovementFlags.Drifting);
            packed.SetData(expected, 0);

            var playerData = packed.GetData(0);
            Assert.Equal(expected.Modes, playerData.Modes);
        }

        [Fact]
        public void MovementSerializeLast()
        {
            var packed = new MovementFlagsPacked().AsInterface();
            var expected = new MovementFlagsMsg(MovementFlags.Drifting | MovementFlags.Boost | MovementFlags.Down);
            packed.SetData(expected, 6);

            var playerData = packed.GetData(6);
            Assert.Equal(expected.Modes, playerData.Modes);
        }

        [Fact]
        public void MovementSerializeAll()
        {
            var packed   = new MovementFlagsPacked().AsInterface();
            for (int x = 0; x < 8; x++)
            {
                var expected = new MovementFlagsMsg(MovementFlags.Braking | MovementFlags.Drifting);
                packed.SetData(expected, x);

                var playerData = packed.GetData(x);
                Assert.Equal(expected.Modes, playerData.Modes);
            }
        }

        [Fact]
        public void AttackSerialize()
        {
            var packed = new AttackPacked().AsInterface();
            var expected = new SetAttack(7);
            packed.SetData(expected, 0);

            var playerData = packed.GetData(0);
            Assert.Equal(expected, playerData);
        }

        [Fact]
        public void AttackSerializeLast()
        {
            var packed = new AttackPacked().AsInterface();
            var expected = new SetAttack(1);
            packed.SetData(expected, 7);

            var playerData = packed.GetData(7);
            Assert.Equal(expected, playerData);
        }

        [Fact]
        public void AttackSerializeInvalid()
        {
            var packed = new AttackPacked().AsInterface();
            var expected = new SetAttack(false, 0);
            packed.SetData(expected, 0);

            var playerData = packed.GetData(0);
            Assert.Equal(expected, playerData);
            Assert.False(playerData.IsValid);
        }

        [Fact]
        public unsafe void AttackSerializeOutOfRange()
        {
            var packed = new AttackPacked().AsInterface();
            var expected = new SetAttack(AttackPacked.NumberOfEntries);
            packed.SetData(expected, 0);

            var playerData = packed.GetData(0);
            Assert.NotEqual(expected, playerData);
        }
    }
}
