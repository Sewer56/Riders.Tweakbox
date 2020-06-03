using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs;
using Xunit;

namespace Riders.Netplay.Messages.Tests.Structs
{
    public class HashingTests
    {

        [Fact]
        public void HashIsDeterministic()
        {
            var gameData     = GameData.Random();
            var gameDataHash = DataHash.FromData(gameData);
            var gameDataHash2 = DataHash.FromData(gameData);
            Assert.Equal(gameDataHash, gameDataHash2);
        }

        [Fact]
        public void SerializeDeserialize()
        {
            var gameData           = GameData.Random();
            var gameDataFromBytes  = GameData.FromUncompressedBytes(gameData);
            var gameDataToBytes    = gameDataFromBytes.ToUncompressedBytes();
            Assert.Equal(gameData, gameDataToBytes);
        }

        [Fact]
        public void SerializeDeserializeCompressed()
        {
            var gameData      = GameData.FromUncompressedBytes(GameData.Random());
            var gameDataBytes = gameData.ToCompressedBytes();

            using var memoryStream = new MemoryStream(gameDataBytes);
            using var streamReader = new BufferedStreamReader(memoryStream, (int) memoryStream.Length);
            var gameDataCopy       = GameData.FromCompressedBytes(streamReader);
            var gameDataCopyBytes  = gameDataCopy.ToCompressedBytes();

            Assert.Equal(gameDataBytes, gameDataCopyBytes);
        }
    }
}
