using System;
using System.IO;
using EnumsNET;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;
using Riders.Netplay.Messages.Unreliable;
using MenuSynchronizationCommand = Riders.Netplay.Messages.Reliable.Structs.Menu.MenuSynchronizationCommand;

namespace Riders.Netplay.Messages
{
    public unsafe class ReliablePacket : IPacket<ReliablePacket>, IPacket
    {
        public PacketKind GetPacketType() => PacketKind.Reliable;

        /// <summary>
        /// Flags attacked to the original packet.
        /// </summary>
        public HasData Flags { get; private set; }

        /// <summary>
        /// Random seed and time to resume the game at.
        /// </summary>
        public SRandSync? Random;

        /// <summary>
        /// Gear and physics data.
        /// </summary>
        public GameData? GameData;
        public bool HasSyncStartReady;
        public bool HasSyncStartSkip = false;

        /// <summary>
        /// Set if client demands to set boost/tornado/attack for them.
        /// </summary>
        public MovementFlagsMsg? SetMovementFlags;

        /// <summary>
        /// Sets boost/tornado/attack if sent to players.
        /// </summary>
        public MovementFlagsPacked? MovementFlags;

        /// <summary>
        /// Set lap counter if client demands.
        /// </summary>
        public LapCounter? SetLapCounter;

        /// <summary>
        /// Sends an updated copy of all lap counters to the clients.
        /// </summary>
        public LapCounters? LapCounters;

        /// <summary>
        /// Sets an attack to be performed between 2 players.
        /// </summary>
        public SetAttack? SetAttack;

        /// <summary>
        /// Informs people an attack has been performed between players.
        /// </summary>
        public AttackPacked? Attack;

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
        public MenuSynchronizationCommand? MenuSynchronizationCommand;

        /// <summary>
        /// Server message.
        /// </summary>
        public ServerMessage? ServerMessage;

        public ReliablePacket() { }
        public ReliablePacket(IMenuSynchronizationCommand command) => MenuSynchronizationCommand = new MenuSynchronizationCommand(command);
        public ReliablePacket(IServerMessage message) => ServerMessage = new ServerMessage(message);

        /// <summary>
        /// Converts this message to an set of bytes.
        /// </summary>
        public byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(sizeof(UnreliablePacketHeader));
            writer.Write(GetFlags());
            writer.WriteNullable(Random);
            if (GameData.HasValue) writer.Write(GameData.Value.ToCompressedBytes());

            writer.WriteNullable(SetMovementFlags);
            if (MovementFlags.HasValue) writer.Write(MovementFlags.Value.AsInterface().Serialize());

            writer.WriteNullable(SetLapCounter);
            if (LapCounters.HasValue) writer.Write(LapCounters.Value.AsInterface().Serialize());

            writer.WriteNullable(SetAttack);
            if (Attack.HasValue) writer.Write(Attack.Value.AsInterface().Serialize());

            writer.WriteNullable(AntiCheatTriggered);
            writer.WriteNullable(AntiCheatGameData);
            writer.WriteNullable(AntiCheatHeartbeat);

            if (MenuSynchronizationCommand.HasValue) writer.Write(MenuSynchronizationCommand.Value.ToBytes());
            if (ServerMessage.HasValue) writer.Write(ServerMessage.Value.ToBytes());
            return writer.ToArray();
        }

        /// <summary>
        /// Deserializes an instance of the packet.
        /// </summary>
        public unsafe void Deserialize(Span<byte> data)
        {
            using var memStream = new MemoryStream(data.ToArray());
            using var reader = new BufferedStreamReader(memStream, (int)memStream.Length);
            Flags = reader.Read<HasData>();
            if (Flags.HasAllFlags(HasData.HasSyncStartReady)) HasSyncStartReady = true;
            if (Flags.HasAllFlags(HasData.HasSyncStartSkip)) HasSyncStartSkip = true;

            reader.ReadIfHasFlags(ref Random, Flags, HasData.HasSRand);
            if (Flags.HasAllFlags(HasData.HasGameData)) GameData = Reliable.Structs.Gameplay.GameData.FromCompressedBytes(reader);

            reader.ReadIfHasFlags(ref SetMovementFlags, Flags, HasData.HasSetMovementFlags);
            if (Flags.HasAllFlags(HasData.HasMovementFlags)) MovementFlags = new MovementFlagsPacked().AsInterface().Deserialize(reader);

            reader.ReadIfHasFlags(ref SetLapCounter, Flags, HasData.HasSetLapCounter);
            if (Flags.HasAllFlags(HasData.HasLapCounters)) LapCounters = new LapCounters().AsInterface().Deserialize(reader);

            reader.ReadIfHasFlags(ref SetAttack, Flags, HasData.HasSetAttack);
            if (Flags.HasAllFlags(HasData.HasAttack)) Attack = new AttackPacked().AsInterface().Deserialize(reader);

            reader.ReadIfHasFlags(ref AntiCheatTriggered, Flags, HasData.HasAntiCheatTriggered);
            reader.ReadIfHasFlags(ref AntiCheatGameData, Flags, HasData.HasAntiCheatGameData);
            reader.ReadIfHasFlags(ref AntiCheatHeartbeat, Flags, HasData.HasAntiCheatHeartbeat);
            if (Flags.HasAllFlags(HasData.HasMenuSynchronizationCommand)) MenuSynchronizationCommand = Reliable.Structs.Menu.MenuSynchronizationCommand.FromBytes(reader);
            if (Flags.HasAllFlags(HasData.HasServerMessage)) ServerMessage = Reliable.Structs.Server.ServerMessage.FromBytes(reader);
        }

        /// <summary>
        /// Gets the flags based off of hte contents of the current 
        /// </summary>
        /// <returns></returns>
        private HasData GetFlags()
        {
            var flags = new HasData();
            if (Random.HasValue) flags |= HasData.HasSRand;
            if (GameData.HasValue) flags |= HasData.HasGameData;

            if (HasSyncStartReady) flags |= HasData.HasSyncStartReady;
            if (HasSyncStartSkip) flags |= HasData.HasSyncStartSkip;

            if (SetAttack.HasValue) flags |= HasData.HasSetAttack;
            if (Attack.HasValue) flags |= HasData.HasAttack;

            if (SetLapCounter.HasValue) flags |= HasData.HasSetLapCounter;
            if (LapCounters.HasValue) flags |= HasData.HasLapCounters;

            if (SetMovementFlags.HasValue) flags |= HasData.HasSetMovementFlags;
            if (MovementFlags.HasValue) flags |= HasData.HasMovementFlags;

            if (AntiCheatTriggered.HasValue) flags |= HasData.HasAntiCheatTriggered;
            if (AntiCheatGameData.HasValue) flags |= HasData.HasAntiCheatGameData;
            if (AntiCheatHeartbeat.HasValue) flags |= HasData.HasAntiCheatHeartbeat;
            if (MenuSynchronizationCommand.HasValue) flags |= HasData.HasMenuSynchronizationCommand;
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

            // Randomization & Time Sync
            HasSRand = 1,  // Host -> Client: RNG Seed and Time to Resume Game synced with external NTP source

            // Integrity Synchronization
            HasGameData = 1 << 1,           // Host -> Client: Running, Gear Stats, Character Stats (Compressed)

            HasSyncStartReady   = 1 << 2,   // Client -> Host: Ready signal to tell host ready after intro cutscene.
            Unused              = 1 << 3,   // [Removed, Currently Unused] | Old function: `Host -> Client: Ready signal to tell clients to start race at a given time.`
            HasSyncStartSkip    = 1 << 4,   // Informs Host/Client to skip the stage intro cutscene.

            HasSetLapCounter    = 1 << 5,  // Client -> Host: Set lap counter for the player.
            HasLapCounters      = 1 << 6,  // Host -> Client: Set Lap counters for each player.

            // Race Integrity Synchronization
            HasSetMovementFlags    = 1 << 7,  // Client -> Host: Inform host of boost, tornado.
            HasMovementFlags       = 1 << 8,  // Host -> Client: Triggers boost, tornado for clients.
            HasSetAttack           = 1 << 9,  // Client -> Host: Inform host to attack a player.
            HasAttack              = 1 << 10, // Host -> Client: Inform client of an impending attack.

            // Anti-Cheat
            HasAntiCheatTriggered   = 1 << 11, // Host -> Client: Anti-cheat has been triggered, let all clients know.
            HasAntiCheatGameData    = 1 << 12, // Client -> Host: Hash of game data
            HasAntiCheatHeartbeat   = 1 << 13, // Client -> Host: Timestamp & frames elapsed

            // Menu & Server Synchronization
            // These messages are fairly infrequent and/or work outside the actual gameplay loop.
            // We can save a byte during regular gameplay here.
            HasMenuSynchronizationCommand   = 1 << 14, // [Struct] Menu State Synchronization Command.
            HasServerMessage                = 1 << 15, // [Struct] General Server Message (Set Name, Try Connect etc.)
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MovementFlags?.AsInterface().Dispose();
            Attack?.AsInterface().Dispose();
            LapCounters?.AsInterface().Dispose();
        }
    }
}
