using System;
using System.Collections.Generic;
using System.Text;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Shared;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class BitPackingTests
    {

        [Fact]
        public void SerializeFirst()
        {
            var packed = new BoostTornadoAttackPacked();
            var expected = new BoostTornadoAttack(AttackModes.Attack, 5);
            packed.SetPlayerData(expected, 0);

            var playerData = packed.GetPlayerData(0);
            Assert.Equal(expected.Modes, playerData.Modes);
            Assert.Equal(expected.TargetPlayer, playerData.TargetPlayer);
        }

        [Fact]
        public void SerializeLast()
        {
            var packed = new BoostTornadoAttackPacked();
            var expected = new BoostTornadoAttack(AttackModes.Attack | AttackModes.Boost, 3);
            packed.SetPlayerData(expected, 7);

            var playerData = packed.GetPlayerData(7);
            Assert.Equal(expected.Modes, playerData.Modes);
            Assert.Equal(expected.TargetPlayer, playerData.TargetPlayer);
        }

        [Fact]
        public void SerializeAll()
        {
            var packed   = new BoostTornadoAttackPacked();
            for (int x = 0; x < 8; x++)
            {
                var expected = new BoostTornadoAttack(AttackModes.Boost | AttackModes.Attack, (byte) x);
                packed.SetPlayerData(expected, x);

                var playerData = packed.GetPlayerData(x);
                Assert.Equal(expected.Modes, playerData.Modes);
                Assert.Equal(expected.TargetPlayer, playerData.TargetPlayer);
            }
        }
    }
}
