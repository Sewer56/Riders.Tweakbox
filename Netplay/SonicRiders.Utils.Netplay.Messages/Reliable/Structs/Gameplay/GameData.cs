using System;
using System.IO;
using System.Linq;
using K4os.Compression.LZ4;
using Reloaded.Memory;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct GameData
    {
        public static readonly int StructSize = StructArray.GetSize<ExtremeGear>(Player.NumberOfGears) +
                                                Struct.GetSize<RunningPhysics>() + Struct.GetSize<RunningPhysics2>() + Struct.GetSize<RaceSettings>();

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

        /// <summary>
        /// The current settings for the race.
        /// </summary>
        public RaceSettings RaceSettings;

        /// <summary>
        /// Writes the contents of this packet to the game memory.
        /// </summary>
        public unsafe void ToGame()
        {
            *Player.RunPhysics  = RunningPhysics1;
            *Player.RunPhysics2 = RunningPhysics2;
            Player.Gears.CopyFrom(Gears, Gears.Length);
            *State.CurrentRaceSettings = RaceSettings;
        }

        public static unsafe GameData FromGame()
        {
            var data = new GameData
            {
                RunningPhysics1 = *Player.RunPhysics,
                RunningPhysics2 = *Player.RunPhysics2,
                RaceSettings = *State.CurrentRaceSettings,
                Gears = Player.Gears.ToArray()
            };

            return data;
        }

        public static GameData FromCompressedBytes(BufferedStreamReader reader) => FromUncompressedBytes(Utilities.DecompressLZ4Stream(new byte[StructSize], reader));
        public static GameData FromUncompressedBytes(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var gameDataStream = new BufferedStreamReader(memoryStream, memoryStream.Capacity))
            {
                var gameData = new GameData();
                gameData.Gears = new ExtremeGear[Player.NumberOfGears];
                for (int x = 0; x < gameData.Gears.Length; x++)
                    gameData.Gears[x] = gameDataStream.Read<ExtremeGear>();

                gameData.RunningPhysics1 = gameDataStream.Read<RunningPhysics>();
                gameData.RunningPhysics2 = gameDataStream.Read<RunningPhysics2>();
                gameData.RaceSettings = gameDataStream.Read<RaceSettings>();
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
                memStream.Write(Struct.GetBytes(RaceSettings));
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