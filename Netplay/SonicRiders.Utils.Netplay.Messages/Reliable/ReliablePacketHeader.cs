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
            Null            = 0,
            
            // Randomization
            HasRand             = 1, 
            HasSRand            = 1 << 1,

            // Integrity Synchronization
            HasGameData         = 1 << 2, // Running, Gear Stats, Character Stats (Compressed)
            HasSyncStartReady   = 1 << 3, // Ready signal from client to host after race intro cutscene skip.
            HasSyncStartGo      = 1 << 4, // Ready signal from host to tell clients to start race at a given time.
            HasLapCounters      = 1 << 5, // Lap counters for each player.

            // Race Integrity Synchronization
            HasSetBoostTornadoAttack = 1 << 6, // Client -> Host: Inform host of boost, tornado, attack.
            HasBoostTornadoAttack    = 1 << 7, // Host -> Client: Triggers boost, tornado & attack for clients.

            // Anti-Cheat
            HasAntiCheatTriggered  = 1 << 8,  // Anti-cheat has been triggered, let all clients know.
            HasAntiCheatGameData   = 1 << 9,  // Hash of game data
            HasAntiCheatHeartbeat  = 1 << 10, // Timestamp & frames elapsed

            // Menu Synchronization
            HasMenuSynchronizationCommand = 1 << 11, // Menu state synchronization commands.
            HasUnused6                    = 1 << 12,
            
            // Last 3 bytes occupied by player count.
        }

    }
}
