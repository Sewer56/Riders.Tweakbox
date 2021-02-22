using System;
using DotNext.Buffers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.BitStream;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Sewer56.BitStream;
using Sewer56.SonicRiders.Structures.Gameplay;
using Xunit;

namespace Riders.Netplay.Messages.Tests
{
    public class BitPackingTests
    {
        [Fact]
        public void SerializeAllAttacks()
        {
            var attacks = new SetAttack[Constants.MaxNumberOfPlayers];
            for (int x = 0; x < Constants.MaxNumberOfPlayers - 1; x++)
                attacks[x] = new SetAttack((byte) (x + 1));

            var packed  = new AttackPacked().Create(attacks);

            using var rental = new ArrayRental<byte>(1024);
            var bitStream    = new BitStream<RentalByteStream>(new RentalByteStream(rental));

            packed.ToStream(ref bitStream);
            bitStream.BitIndex = 0;
            var deserialized = new AttackPacked();
            deserialized.FromStream(ref bitStream);

            Assert.Equal(packed.NumElements, deserialized.NumElements);
            for (int x = 0; x < Constants.MaxNumberOfPlayers; x++)
                Assert.Equal(packed.Elements[x], deserialized.Elements[x]);
        }

        [Fact]
        public void SetializeLapCounters()
        {
            using var attacks = new LapCountersPacked();
            attacks.ToPooled(Constants.MaxRidersNumberOfPlayers);

            var random = new Random();
            for (int x = 0; x < 1000; x++)
            {
                // Populate random
                for (int y = 0; y < attacks.NumElements; y++)
                    attacks.Elements[y] = new LapCounter((byte) y, new Timer((byte) random.Next(), (byte) random.Next(), (byte) random.Next()));

                // Serialize
                using var rental = new ArrayRental<byte>(1024);
                var bitStream    = new BitStream<RentalByteStream>(new RentalByteStream(rental));
                attacks.ToStream(ref bitStream);
                bitStream.BitIndex = 0;

                var deserialized = new LapCountersPacked();
                deserialized.FromStream(ref bitStream);

                // Assert
                Assert.Equal(attacks.NumElements, deserialized.NumElements);
                for (int y = 0; y < attacks.NumElements; y++)
                    Assert.Equal(attacks.Elements[y], deserialized.Elements[y]);
            }
        }
    }
}
