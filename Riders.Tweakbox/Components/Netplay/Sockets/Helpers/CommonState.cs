using System.Collections.Generic;
using System.Linq;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using StructLinq;
namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers;

public class CommonState
{
    public CommonState(ClientData selfInfo, Socket owner)
    {
        SelfInfo = selfInfo;
        Owner = owner;
    }

    /// <summary>
    /// The socket that owns this state object.
    /// </summary>
    public Socket Owner;

    /// <summary>
    /// Contains information about own player.
    /// </summary>
    public ClientData SelfInfo;

    /// <summary>
    /// Current frame counter for the client/server.
    /// Can be reset by modules or other code as needed.
    /// </summary>
    public int FrameCounter;

    /// <summary>
    /// Packets older than this will be discarded.
    /// </summary>
    public int MaxLatency = 1000;

    /// <summary>
    /// Timeout before a client is disconnected.
    /// Also used for various handshakes such as initial exchange of game/gear data or start line synchronization.
    /// </summary>
    public int DisconnectTimeout = 8000;

    /// <summary>
    /// Contains information about other clients.
    /// For client, index 0 contains host, and then other players.
    /// For host, index 0 will contain first client, and others.
    /// </summary>
    public ClientData[] ClientInfo { get; private set; } = new ClientData[0];

    /// <summary>
    /// Maps client indices to the corresponding info.
    /// </summary>
    public Dictionary<int, ClientData> ClientIndexToDataMap { get; private set; } = new Dictionary<int, ClientData>();

    /// <summary>
    /// Number of local players.
    /// Also index of first non-local player.
    /// </summary>
    public int NumLocalPlayers => SelfInfo.NumPlayers;

    /// <summary>
    /// Returns the total count of players.
    /// </summary>
    public int GetPlayerCount()
    {
        if (ClientInfo.Length > 0)
            return SelfInfo.NumPlayers + ClientInfo.ToStructEnumerable().Sum(x => x.NumPlayers, x => x);

        return SelfInfo.NumPlayers;
    }

    /// <summary>
    /// Gets the <see cref="ClientData"/> belonging to the host.
    /// </summary>
    public virtual ClientData GetHostData() => ClientInfo[0];

    /// <summary>
    /// Converts a local player index to an index on the host's end.
    /// </summary>
    public virtual int GetHostPlayerIndex(int playerIndex)
    {
        // Check if the player is a local player.
        // If it is, we offset from our own index.
        if (IsLocal(playerIndex))
        {
            // Player number between 0 to NumLocalPlayers
            int playerNo = (NumLocalPlayers - 1) - playerIndex;
            return SelfInfo.PlayerIndex + playerNo;
        }

        // If the player is non-local, then consider the following.
        // Host:    0 1 2 3 4
        // Client:  1 0 2 3 4

        // Get offset from host's start of the list
        int offset = playerIndex - NumLocalPlayers;

        // If the offset intersects with our own index, then it means it belongs
        // to a player after us, so we should add our own player count.
        if (offset >= SelfInfo.PlayerIndex && offset < SelfInfo.PlayerIndex + NumLocalPlayers)
            offset += NumLocalPlayers;

        return offset;
    }

    /// <summary>
    /// Translates a host player index into a local player index. 
    /// </summary>
    public virtual int GetLocalPlayerIndex(int playerIndex)
    {
        var selfIndex = SelfInfo.PlayerIndex;

        // If remote player index is our own, it's a simple subtraction.
        if (playerIndex >= selfIndex && playerIndex < selfIndex + NumLocalPlayers)
            return playerIndex - selfIndex;

        // If remote player index is not our own, then consider it
        // an offset from our first remote player (at index NumLocalPlayers).

        // If player is after our index, shift for the spaces we occupied.
        if (playerIndex >= selfIndex + NumLocalPlayers)
            playerIndex -= NumLocalPlayers;

        return NumLocalPlayers + playerIndex;
    }

    /// <summary>
    /// True if a player index is a human, else false.
    /// </summary>
    public bool IsHuman(int playerIndex)
    {
        // Compare against highest player index.
        if (ClientInfo.Length > 0)
        {
            if (IsSpectator(playerIndex))
                return false;

            if (IsRemote(playerIndex))
                return true;
        }

        return IsLocal(playerIndex);
    }

    /// <summary>
    /// True if the player is a spectator, else false.
    /// </summary>
    public bool IsSpectator(int playerIndex) => playerIndex == Constants.MaxNumberOfClients - 1;

    /// <summary>
    /// Determines if the player is a local player (on this machine).
    /// </summary>
    /// <param name="playerIndex">The index of the player.</param>
    public bool IsLocal(int playerIndex)
    {
        return playerIndex < NumLocalPlayers;
    }

    /// <summary>
    /// True if the player is a remote human player.
    /// </summary>
    public bool IsRemote(int playerIndex) => GetRemoteClientInfo(playerIndex, out _) != null;

    /// <summary>
    /// Retrieves the data for a particular client given an index.
    /// </summary>
    /// <param name="playerIndex">The local player index.</param>
    /// <param name="playerOffset">Offset of the player. If this value is 1, it indicates the 2nd player on that local machine is being used.</param>
    public ClientData GetClientInfo(int playerIndex, out int playerOffset)
    {
        if (IsLocal(playerIndex))
        {
            playerOffset = NumLocalPlayers - playerIndex;
            return SelfInfo;
        }

        return GetRemoteClientInfo(playerIndex, out playerOffset);
    }

    /// <summary>
    /// Retrieves the data for a particular remote client given an index.
    /// </summary>
    /// <param name="playerIndex">The local player index.</param>
    /// <param name="playerOffset">Offset of the player. If this value is 1, it indicates the 2nd player on that local machine is being used.</param>
    public ClientData GetRemoteClientInfo(int playerIndex, out int playerOffset)
    {
        playerOffset = default;
        for (int x = 0; x < ClientInfo.Length; x++)
        {
            var player   = ClientInfo[x];
            var minIndex = GetLocalPlayerIndex(player.PlayerIndex);
            var maxIndex = minIndex + player.NumPlayers;

            if (playerIndex >= minIndex && playerIndex < maxIndex)
            {
                playerOffset = playerIndex - minIndex;
                return player;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the client info stored by the state tracker.
    /// </summary>
    public void SetClientInfo(ClientData[] data)
    {
        ClientInfo = data;
        ClientIndexToDataMap = data.ToDictionary(x => x.ClientIndex);
    }
}
