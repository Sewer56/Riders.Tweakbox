using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using StructLinq;
using Constants = Riders.Netplay.Messages.Misc.Constants;
namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers;

/// <summary>
/// Maps individual connected peers to individual players.
/// </summary>
public class ClientMap
{
    private Dictionary<int, ClientData> _dictionary = new Dictionary<int, ClientData>();
    private bool[] _clientSlotsUsed = new bool[Constants.MaxNumberOfClients];
    private int _lastClientSlot = -1;

    /// <summary>
    /// Initializes a client map given information about the host, then other players.
    /// </summary>
    /// <param name="selfData">Data about the host.</param>
    public ClientMap(ClientData selfData)
    {
        selfData.ClientIndex = AssignNextClientSlot();
        _dictionary[-1] = selfData;
    }

    /// <summary>
    /// Gets the player data for a peer.
    /// </summary>
    public ClientData GetPlayerData(NetPeer peer) => _dictionary.ContainsKey(peer.Id) ? _dictionary[peer.Id] : null;

    /// <summary>
    /// Tries to get the data for an individual peer.
    /// </summary>
    /// <param name="peer">The peer.</param>
    /// <param name="data">The data.</param>
    public bool TryGetPlayerData(NetPeer peer, out ClientData data) => _dictionary.TryGetValue(peer.Id, out data);

    /// <summary>
    /// Returns true if there is data for a given peer, else false.
    /// </summary>
    public bool Contains(NetPeer peer) => TryGetPlayerData(peer, out _);

    /// <summary>
    /// Sets the player data for a peer.
    /// </summary>
    public bool TryAddOrUpdatePeer(NetPeer peer, ClientData data, out string failureReason)
    {
        failureReason = null;

        // Compact internal client map.
        CompactPlayerIndices();

        // Add if doesn't exist.
        if (!_dictionary.ContainsKey(peer.Id))
        {
            // Get new slot for player.
            int emptySlot = GetNextPlayerSlot();
            if (emptySlot == -1 && data.NumPlayers > 0)
            {
                failureReason = "All player slots are used up. You can still join as spectator.";
                return false;
            }

            if (GetRemainingNumClients() <= 0)
            {
                failureReason = "No client slots left!";
                return false;
            }

            _dictionary[peer.Id] = new ClientData()
            {
                PlayerIndex = emptySlot,
                ClientIndex = AssignNextClientSlot()
            };
        }

        // Check if there are enough remaining players.
        var remainingPlayers = GetRemainingNumPlayers();
        if (data.NumPlayers > remainingPlayers)
        {
            RemovePeer(peer);
            failureReason = "Not enough player slots.";
            return false;
        }

        _dictionary[peer.Id].UpdateFromClient(data);
        return true;
    }

    /// <summary>
    /// Removes a given peer from the player map.
    /// </summary>
    /// <param name="peer"></param>
    public void RemovePeer(NetPeer peer)
    {
        if (_dictionary.ContainsKey(peer.Id))
            _dictionary.Remove(peer.Id);
    }

    /// <summary>
    /// Converts the current dictionary to a message to send to the players.
    /// </summary>
    public HostSetPlayerData ToMessage(NetPeer excludePeer)
    {
        var excludeIndex = int.MaxValue;
        var clientIndex = int.MaxValue;
        if (TryGetPlayerData(excludePeer, out var playerData))
        {
            excludeIndex = playerData.PlayerIndex;
            clientIndex = playerData.ClientIndex;
        }

        return ToMessage(excludeIndex, clientIndex);
    }

    /// <summary>
    /// Converts the current dictionary to a message to send to the players.
    /// </summary>
    public HostSetPlayerData ToMessage(int excludePlayerIndex, int excludeClientIndex)
    {
        var values = new List<ClientData>();
        values.AddRange(_dictionary.Values.ToArray());
        return new HostSetPlayerData(values.ToStructEnumerable().Where(x => x.PlayerIndex != excludePlayerIndex, x => x).ToArray(), excludePlayerIndex, excludeClientIndex);
    }

    /// <summary>
    /// Gets the remaining available number of player slots.
    /// </summary>
    private int GetRemainingNumPlayers()
    {
        var numPlayers = _dictionary.Values.Sum(x => x.NumPlayers);
        return Constants.MaxRidersNumberOfPlayers - numPlayers;
    }

    /// <summary>
    /// Gets the remaining available number of client slots.
    /// </summary>
    private int GetRemainingNumClients() => Constants.MaxNumberOfClients - _dictionary.Values.Count;

    /// <summary>
    /// Gets the first available empty slot, otherwise -1 if doesn't exist.
    /// </summary>
    private int GetNextPlayerSlot()
    {
        var numPlayers = _dictionary.Values.Sum(x => x.NumPlayers);
        if (numPlayers < Constants.MaxRidersNumberOfPlayers)
            return numPlayers;

        return -1;
    }

    /// <summary>
    /// Removes gaps in internal player indexes.
    /// </summary>
    private void CompactPlayerIndices()
    {
        var sorted = _dictionary.OrderBy(x => x.Value.PlayerIndex);
        int currentPlayerIndex = 0;

        foreach (var sort in sorted)
        {
            if (sort.Value.NumPlayers < 1)
            {
                sort.Value.PlayerIndex = -1;
            }
            else
            {
                sort.Value.PlayerIndex = currentPlayerIndex;
                currentPlayerIndex += sort.Value.NumPlayers;
            }
        }
    }

    /// <summary>
    /// Assigns the first available empty slot, otherwise -1 if doesn't exist.
    /// </summary>
    private int AssignNextClientSlot()
    {
        int currentSlot  = 0;
        var originalSlot = currentSlot;

        do
        {
            currentSlot = IncrementClientSlot(currentSlot);
            if (!_clientSlotsUsed[currentSlot])
                return AssignClientSlot(currentSlot);

        } 
        while (currentSlot != originalSlot);

        return -1;
    }

    private int AssignClientSlot(int slot)
    {
        _lastClientSlot = slot;
        _clientSlotsUsed[slot] = true;
        return slot;
    }

    private int IncrementClientSlot(int slot) => (slot + 1) % _clientSlotsUsed.Length;
}
