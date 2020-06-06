using System;
using EnumsNET;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Unreliable;

namespace Riders.Netplay.Messages
{
    public unsafe struct ReliablePacket
    {
        /// <summary>
        /// Flags attacked to the original packet.
        /// </summary>
        public HasData Flags { get; private set; }

        /// <summary>
        /// Random seed.
        /// </summary>
        public Seed? Random;

        /// <summary>
        /// Gear and physics data.
        /// </summary>
        public GameData? GameData;
        public bool HasSyncStartReady;

        /// <summary>
        /// Time to resume gameplay at after stage load.
        /// </summary>
        public SyncStartGo? SyncStartGo;
        public bool HasIncrementLapCounter;

        /// <summary>
        /// Sets the lap counters for each players.
        /// </summary>
        public LapCounters? SetLapCounters;

        /// <summary>
        /// Set if client demands to set boost/tornado/attack for them.
        /// </summary>
        public SetBoostTornadoAttack? SetBoostTornadoAttack;

        /// <summary>
        /// Sets boost/tornado/attack if sent to players.
        /// </summary>
        public BoostTornadoAttackPacked? BoostTornadoAttack;

        /// <summary>
        /// Message from host that anti-cheat was triggered.
        /// </summary>
        public AntiCheatTriggered? AntiCheatTriggered;

        /// <summary>
        /// Contains a hash of board data from clients.
        /// </summary>
        public DataHash? AntiCheatGameData;

        /// <summary>
        /// Contains a timestamp sent at regular intervals from clients.
        /// </summary>
        public Heartbeat? AntiCheatHeartbeat;

        /// <summary>
        /// Menu synchronization command.
        /// </summary>
        public MenuSynchronizationCommand? SynchronizationCommand;

        /// <summary>
        /// Menu synchronization command.
        /// </summary>
        public ServerMessage? ServerMessage;

        /// <summary>
        /// Converts this message to an set of bytes.
        /// </summary>
        public byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(sizeof(UnreliablePacketHeader));
            writer.Write(GetFlags());
            writer.WriteNullable(Random);
            if (GameData.HasValue) writer.Write(GameData.Value.ToCompressedBytes());
            writer.WriteNullable(SyncStartGo);
            writer.WriteNullable(SetLapCounters);
            writer.WriteNullable(SetBoostTornadoAttack);

            writer.WriteNullable(BoostTornadoAttack);
            writer.WriteNullable(AntiCheatTriggered);
            writer.WriteNullable(AntiCheatGameData);
            writer.WriteNullable(AntiCheatHeartbeat);

            if (SynchronizationCommand.HasValue) writer.Write(SynchronizationCommand.Value.ToBytes());
            if (ServerMessage.HasValue) writer.Write(ServerMessage.Value.ToBytes());
            return writer.ToArray();
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static unsafe ReliablePacket Deserialize(BufferedStreamReader reader)
        {
            var packet = new ReliablePacket();
            packet.Flags = reader.Read<HasData>();
            if (packet.Flags.HasAllFlags(HasData.HasSyncStartReady)) packet.HasSyncStartReady = true;
            if (packet.Flags.HasAllFlags(HasData.HasIncrementLapCounter)) packet.HasIncrementLapCounter = true;

            reader.SetValueIfHasFlags(ref packet.Random, packet.Flags, HasData.HasRand);
            if (packet.Flags.HasAllFlags(HasData.HasGameData)) Reliable.Structs.Gameplay.GameData.FromCompressedBytes(reader);
            reader.SetValueIfHasFlags(ref packet.SyncStartGo, packet.Flags, HasData.HasSyncStartGo);
            reader.SetValueIfHasFlags(ref packet.SetLapCounters, packet.Flags, HasData.HasLapCounters);
            reader.SetValueIfHasFlags(ref packet.SetBoostTornadoAttack, packet.Flags, HasData.HasSetBoostTornadoAttack);
            reader.SetValueIfHasFlags(ref packet.BoostTornadoAttack, packet.Flags, HasData.HasBoostTornadoAttack);
            reader.SetValueIfHasFlags(ref packet.AntiCheatTriggered, packet.Flags, HasData.HasAntiCheatTriggered);
            reader.SetValueIfHasFlags(ref packet.AntiCheatGameData, packet.Flags, HasData.HasAntiCheatGameData);
            reader.SetValueIfHasFlags(ref packet.AntiCheatHeartbeat, packet.Flags, HasData.HasAntiCheatHeartbeat);
            if (packet.Flags.HasAllFlags(HasData.HasMenuSynchronizationCommand)) packet.SynchronizationCommand = MenuSynchronizationCommand.FromBytes(reader);
            if (packet.Flags.HasAllFlags(HasData.HasServerMessage)) packet.ServerMessage = Reliable.Structs.Server.ServerMessage.FromBytes(reader);
            
            return packet;
        }

        /// <summary>
        /// Gets the flags based off of hte contents of the current 
        /// </summary>
        /// <returns></returns>
        private HasData GetFlags()
        {
            var flags = new HasData();
            if (Random.HasValue) flags |= HasData.HasRand;
            if (GameData.HasValue) flags |= HasData.HasGameData;

            if (HasSyncStartReady) flags |= HasData.HasSyncStartReady;
            if (SyncStartGo.HasValue) flags |= HasData.HasSyncStartGo;
            if (HasIncrementLapCounter) flags |= HasData.HasIncrementLapCounter;

            if (SetLapCounters.HasValue) flags |= HasData.HasLapCounters;
            if (SetBoostTornadoAttack.HasValue) flags |= HasData.HasSetBoostTornadoAttack;
            if (BoostTornadoAttack.HasValue) flags |= HasData.HasBoostTornadoAttack;

            if (AntiCheatTriggered.HasValue) flags |= HasData.HasAntiCheatTriggered;
            if (AntiCheatGameData.HasValue) flags |= HasData.HasAntiCheatGameData;
            if (AntiCheatHeartbeat.HasValue) flags |= HasData.HasAntiCheatHeartbeat;
            if (SynchronizationCommand.HasValue) flags |= HasData.HasMenuSynchronizationCommand;
            if (ServerMessage.HasValue) flags |= HasData.HasServerMessage;

            return flags;
        }

        /// <summary>
        /// Declares whether the packet has a particular component of data.
        /// </summary>
        [Flags]
        public enum HasData : ushort
        {
            Null = 0,

            // Randomization
            HasRand = 1,                    // Host -> Client: RNG Seed

            // Integrity Synchronization
            HasGameData = 1 << 1,           // Host -> Client: Running, Gear Stats, Character Stats (Compressed)

            HasSyncStartReady   = 1 << 2,   // Client -> Host: Ready signal to tell host ready after intro cutscene.
            HasSyncStartGo      = 1 << 3,   // Host -> Client: Ready signal to tell clients to start race at a given time.

            HasIncrementLapCounter = 1 << 4,    // Client -> Host: Increment lap counter for the player.
            HasLapCounters         = 1 << 5,    // Host -> Client: Set Lap counters for each player.

            // Race Integrity Synchronization
            HasSetBoostTornadoAttack    = 1 << 6,  // Client -> Host: Inform host of boost, tornado, attack.
            HasBoostTornadoAttack       = 1 << 7,  // Host -> Client: Triggers boost, tornado & attack for clients.

            // Anti-Cheat
            HasAntiCheatTriggered   = 1 << 8,      // Host -> Client: Anti-cheat has been triggered, let all clients know.
            HasAntiCheatGameData    = 1 << 9,      // Client -> Host: Hash of game data
            HasAntiCheatHeartbeat   = 1 << 10,     // Client -> Host: Timestamp & frames elapsed

            // Menu & Server Synchronization
            // These messages are fairly infrequent and/or work outside the actual gameplay loop.
            // We can save a byte during regular gameplay here.
            HasMenuSynchronizationCommand   = 1 << 11, // [Struct] Menu State Synchronization Command.
            HasServerMessage                = 1 << 12, // [Struct] General Server Message (Set Name, Try Connect etc.)

            Unused0 = 1 << 13,
            Unused1 = 1 << 14,
            Unused2 = 1 << 15
        }
    }
}
