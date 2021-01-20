namespace Riders.Netplay.Messages.Reliable.Structs.Server.Shared
{
    public enum ServerMessageType : byte
    {
        // Name Management
        ClientSetPlayerData = 0,  // Client -> Host: Sets the name for the player.
        HostSetPlayerData   = 1,  // Host -> Client: Stores the names of the players.

        // Ping Management
        HostUpdateClientPing = 2, // Update ping for individual players.

        // Anti-cheat
        HasSetAntiCheatTypes = 10, // Host -> Client: Sets the enabled Anti-Cheat modules.
    }
}