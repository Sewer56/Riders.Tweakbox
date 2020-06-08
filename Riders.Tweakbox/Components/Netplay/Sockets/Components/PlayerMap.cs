using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using Constants = Riders.Netplay.Messages.Constants;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Components
{
    /// <summary>
    /// Maps individual connected peers to individual players.
    /// </summary>
    public class PlayerMap
    {
        private Dictionary<int, int> _dictionary = new Dictionary<int, int>();

        /// <summary>
        /// Adds a peer ID to the dictionary.
        /// </summary>
        /// <returns>The slot of the peer. This is -1 if there are no slots available.</returns>
        public int AddPeer(NetPeer peer)
        {
            int emptySlot = GetEmptySlot();
            if (emptySlot != -1)
                _dictionary[peer.Id] = emptySlot;

            return emptySlot;
        }

        /// <summary>
        /// Gets the player index of a peer.
        /// </summary>
        public int GetPlayerIndex(NetPeer peer)
        {
            if (_dictionary.ContainsKey(peer.Id))
                return _dictionary[peer.Id];

            return -1;
        }

        /// <summary>
        /// Removes a given peer from the player map.
        /// </summary>
        /// <param name="peer"></param>
        public void RemovePlayer(NetPeer peer)
        {
            if (_dictionary.ContainsKey(peer.Id))
                _dictionary.Remove(peer.Id);
        }

        /// <summary>
        /// True if there are empty slots available, else false.
        /// </summary>
        public bool HasEmptySlots() => GetEmptySlot() != -1;

        /// <summary>
        /// Gets the first available empty slot, otherwise -1 if doesn't exist.
        /// </summary>
        private int GetEmptySlot()
        {
            for (int x = 0; x < Constants.NumberOfPeerPlayers; x++)
            {
                if (!_dictionary.Values.Contains(x))
                    return x;
            }

            return -1;
        }
    }
}
