using System;
using System.IO;
using K4os.Compression.LZ4;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Sewer56.SonicRiders.Fields;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct GameData
    {
        public static readonly int StructSize = StructArray.GetSize<ExtremeGear>(ExtremeGears.NumberOfGears) +
                                                Struct.GetSize<RunningPhysics>() + Struct.GetSize<RunningPhysics2>();

        /// <summary>
        /// Extreme gears of the host player.
        /// </summary>
        public ExtremeGear[] Gears;

        /// <summary>
        /// Contains the running physics for this instance.
        /// </summary>
        public RunningPhysics RunningPhysics1;

        /// <summary>
        /// Contains the running physics (struct 2) for this instance.
        /// </summary>
        public RunningPhysics2 RunningPhysics2;

        public static unsafe GameData FromGame()
        {
            var data = new GameData
            {
                RunningPhysics1 = *Physics.RunningPhysics1, 
                RunningPhysics2 = *Physics.RunningPhysics2
            };

            StructArray.FromPtr((IntPtr)ExtremeGears.ExtremeGear, out data.Gears, ExtremeGears.NumberOfGears);
            return data;
        }

        public static GameData FromCompressedBytes(BufferedStreamReader reader) => FromUncompressedBytes(Utilities.DecompressLZ4Stream(new byte[StructSize], reader));
        public static GameData FromUncompressedBytes(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var gameDataStream = new BufferedStreamReader(memoryStream, memoryStream.Capacity))
            {
                var gameData = new GameData();
                gameData.Gears = new ExtremeGear[ExtremeGears.NumberOfGears];
                for (int x = 0; x < gameData.Gears.Length; x++)
                    gameData.Gears[x] = gameDataStream.Read<ExtremeGear>();

                gameData.RunningPhysics1 = gameDataStream.Read<RunningPhysics>();
                gameData.RunningPhysics2 = gameDataStream.Read<RunningPhysics2>();
                return gameData;
            }
        }

        public byte[] ToCompressedBytes(LZ4Level level = LZ4Level.L10_OPT) => Utilities.CompressLZ4Stream(ToUncompressedBytes(), level);
        public byte[] ToUncompressedBytes()
        {
            using (var memStream = new ExtendedMemoryStream())
            {
                memStream.Write(StructArray.GetBytes(Gears));
                memStream.Write(Struct.GetBytes(RunningPhysics1));
                memStream.Write(Struct.GetBytes(RunningPhysics2));
                return memStream.ToArray();
            }
        }

        public void ToCompressedBytes(ExtendedMemoryStream stream, LZ4Level level = LZ4Level.L10_OPT) => stream.Write(ToCompressedBytes(level));
        public void ToUncompressedBytes(ExtendedMemoryStream stream) => stream.Write(ToUncompressedBytes());
        
        /// <summary>
        /// Internal use only.
        /// </summary>
        public static unsafe byte[] Random()
        {
            var data = new byte[StructSize];
            var random = new Random();
            for (int x = 0; x < data.Length; x++) 
                data[x] = (byte) random.Next();

            return data;
        }
    }
}