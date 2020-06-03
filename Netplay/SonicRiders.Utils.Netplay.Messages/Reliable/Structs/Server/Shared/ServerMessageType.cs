namespace Riders.Netplay.Messages.Reliable.Structs.Server.Shared
{
    public enum ServerMessageType : byte
    {
        // Name Management
        ClientSetPlayerName = 0, // Client -> Host: Sets the name for the player.
        HostHasPlayerNames  = 1, // Host -> Client: Stores the names of the players.

        // Anti-cheat
        HasSetAntiCheatTypes = 2, // Host -> Client: Sets the enabled Anti-Cheat modules.
    }
}