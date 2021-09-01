using System;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
namespace Riders.Netplay.Messages.Reliable.Structs;

/// <summary>
/// Declares whether the packet has a particular component of data.
/// </summary>
public enum MessageType : byte
{
    None,

    // Server
    Disconnect,         // Host -> Client: Reason for disconnection [backwards compatible].
    Version,            // Client -> Host: Version information [backwards compatible].
    VersionEx,          // Client -> Host: Version information [current + newer versions].

    // Randomization & Time Sync
    SRand,              // Host -> Client: RNG Seed and Time to Resume Game synced with external NTP source

    // Integrity Synchronization
    GameData,           // Host -> Client: Running, Gear Stats, Character Stats (Compressed)
    StartSync,          // Client -> Host && Host -> Client: Ready signal to tell other clients to skip cutscene OR to start the game.

    // Race Integrity Synchronization
    BoostTornado,       // Host -> Client && Client -> Host: Boost or tornado was activated by a player with a given index.
    LapCounters,        // Host -> Client && Client -> Host: Set Lap counters for each player.
    Attack,             // Host -> Client && Client -> Host: Inform client/host of 1 or more attacks.
    EndGame,            // Host -> Client: Ends or restarts the current race.

    // Anti-Cheat
    SetAntiCheatTypes,  // Host -> Client: Sets the enabled Anti-Cheat modules.
    AntiCheatTriggered, // Host -> Client: Anti-cheat has been triggered, let all clients know.
    AntiCheatDataHash,  // Client -> Host: Hash of game data
    AntiCheatHeartbeat, // Client -> Host: Timestamp & frames elapsed

    // Server Messages
    ClientSetPlayerData,   // Client -> Host: Sets info of the client (e.g. Name).
    HostSetPlayerData,     // Host -> Client: Redistributes info of each client (e.g. Name).
    HostUpdateClientPing,  // Host -> Client: Redistribute ping of each client.
    ServerGameModifiers,   // Host -> Client: Game specific settings that live in the Netplay layer as opposed to game layer.
                           // (i.e. game code does not directly read/write these values).

    // Menu Synchronization
    CourseSelectLoop,     // Client -> Host
    CourseSelectSync,     // Host   -> Client
    CourseSelectSetStage, // [Battle Mode Only] Client -> Host & Host -> Client.

    RuleSettingsLoop,     // Client -> Host
    RuleSettingsSync,     // Host   -> Client

    CharaSelectLoop,      // Client -> Host
    CharaSelectSync,      // Host   -> Client
    CharaSelectExit,      // Client -> Host & Host -> Client.

    // ServerEx: Version 0.6.1
    ChatMessage, // Client -> Host & Host -> Client.
}

public static class MessageTypeExtensions
{
    public static IReliableMessage Get(this MessageType type)
    {
        return type switch
        {
            MessageType.None => null,

            // Base
            MessageType.Disconnect => new Disconnect(),
            MessageType.Version => new VersionInformation(),
            MessageType.VersionEx => new VersionInformationEx(),

            // Gameplay
            MessageType.SRand => new SRandSync(),
            MessageType.GameData => new GameData(),
            MessageType.StartSync => new StartSync(),
            MessageType.BoostTornado => new BoostTornadoPacked(),
            MessageType.LapCounters => new LapCountersPacked(),
            MessageType.Attack => new AttackPacked(),
            MessageType.EndGame => new EndNetplayGame(),

            // Cheat
            MessageType.SetAntiCheatTypes => new SetAntiCheat(),
            MessageType.AntiCheatTriggered => new AntiCheatTriggered(),
            MessageType.AntiCheatDataHash => new GameData(),
            MessageType.AntiCheatHeartbeat => new AntiCheatHeartbeat(),

            // Server
            MessageType.ClientSetPlayerData => new ClientSetPlayerData(),
            MessageType.HostSetPlayerData => new HostSetPlayerData(),
            MessageType.HostUpdateClientPing => new HostUpdateClientLatency(),
            MessageType.ServerGameModifiers => new ServerGameModifiers(),

            // Menus
            MessageType.CourseSelectLoop => new CourseSelectLoop(),
            MessageType.CourseSelectSync => new CourseSelectSync(),
            MessageType.CourseSelectSetStage => new CourseSelectSetStage(),
            MessageType.RuleSettingsLoop => new RuleSettingsLoop(),
            MessageType.RuleSettingsSync => new RuleSettingsSync(),
            MessageType.CharaSelectLoop => new CharaSelectLoop(),
            MessageType.CharaSelectSync => new CharaSelectSync(),
            MessageType.CharaSelectExit => new CharaSelectExit(),

            // ServerEx,
            MessageType.ChatMessage => new ChatMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
