namespace Riders.Netplay.Messages.Reliable.Structs
{
    /// <summary>
    /// Declares whether the packet has a particular component of data.
    /// </summary>
    public enum MessageType : byte
    {
        None,

        // Server
        Disconnect,         // Host -> Client: Reason for disconnection.

        // Randomization & Time Sync
        SRand,              // Host -> Client: RNG Seed and Time to Resume Game synced with external NTP source

        // Integrity Synchronization
        GameData,           // Host -> Client: Running, Gear Stats, Character Stats (Compressed)
        StartSync,          // Client -> Host && Host -> Client: Ready signal to tell other clients to skip cutscene OR to start the game.

        // Race Integrity Synchronization
        BoostTornado,       // Host -> Client && Client -> Host: Boost or tornado was activated by a player with a given index.
        LapCounters,        // Host -> Client && Client -> Host: Set Lap counters for each player.
        Attack,             // Host -> Client && Client -> Host: Inform client/host of 1 or more attacks.

        // Anti-Cheat
        SetAntiCheatTypes,  // Host -> Client: Sets the enabled Anti-Cheat modules.
        AntiCheatTriggered, // Host -> Client: Anti-cheat has been triggered, let all clients know.
        AntiCheatDataHash,  // Client -> Host: Hash of game data
        AntiCheatHeartbeat, // Client -> Host: Timestamp & frames elapsed

        // Server Messages
        ClientSetPlayerData,   // Client -> Host: Sets info of the client (e.g. Name).
        HostSetPlayerData,     // Host -> Client: Redistributes info of each client (e.g. Name).
        HostUpdateClientPing,  // Host -> Client: Redistribute ping of each client.

        // Anti-cheat

        // Menu Synchronization
        CourseSelectLoop,     // Client -> Host
        CourseSelectSync,     // Host   -> Client
        CourseSelectSetStage, // [Battle Mode Only] Client -> Host & Host -> Client.

        RuleSettingsLoop,     // Client -> Host
        RuleSettingsSync,     // Host   -> Client

        CharaSelectLoop,      // Client -> Host
        CharaSelectSync,      // Host   -> Client
        CharaSelectExit,      // Client -> Host & Host -> Client.
    }
}