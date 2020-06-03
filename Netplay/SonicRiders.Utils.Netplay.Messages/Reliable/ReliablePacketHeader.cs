using System;

namespace Riders.Netplay.Messages.Reliable
{
    /// <summary>
    /// Represents the header for an reliable packet.
    /// </summary>
    public struct ReliablePacketHeader
    {
        /*
           // Header (2 bytes)
           Data Bitfields    = 13 bits
           Number of Players = 3 bits 
        */

        /// <summary>
        /// Declares whether the packet has a particular component of data.
        /// </summary>
        [Flags]
        public enum HasData : ushort
        {
            Null                = 0,
            
            // Randomization
            HasRand             = 1,      // Host -> Client: RNG Seed

            // Integrity Synchronization
            HasGameData         = 1 << 1, // Host -> Client: Running, Gear Stats, Character Stats (Compressed)

            HasSyncStartReady   = 1 << 2, // Client -> Host: Ready signal to tell host ready after intro cutscene.
            HasSyncStartGo      = 1 << 3, // Host -> Client: Ready signal to tell clients to start race at a given time.
            
            HasIncrementLapCounter = 1 << 4, // Client -> Host: Increment lap counter for the player.
            HasLapCounters         = 1 << 5, // Host -> Client: Set Lap counters for each player.

            // Race Integrity Synchronization
            HasSetBoostTornadoAttack = 1 << 6, // Client -> Host: Inform host of boost, tornado, attack.
            HasBoostTornadoAttack    = 1 << 7, // Host -> Client: Triggers boost, tornado & attack for clients.

            // Anti-Cheat
            HasAntiCheatTriggered  = 1 << 8,  // Host -> Client: Anti-cheat has been triggered, let all clients know.
            HasAntiCheatGameData   = 1 << 9,  // Client -> Host: Hash of game data
            HasAntiCheatHeartbeat  = 1 << 10, // Client -> Host: Timestamp & frames elapsed

            // Menu & Server Synchronization
            // These messages are fairly infrequent and/or work outside the actual gameplay loop.
            // We can save a byte during regular gameplay here.
            HasMenuSynchronizationCommand = 1 << 11, // [Struct] Menu State Synchronization Command.
            HasServerMessage              = 1 << 12, // [Struct] General Server Message (Set Name, Try Connect etc.)

            // Last 3 bytes occupied by player count.
        }

    }
}
