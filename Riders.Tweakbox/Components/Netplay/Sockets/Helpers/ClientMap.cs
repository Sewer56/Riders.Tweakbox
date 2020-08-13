using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Maps individual connected peers to individual players.
    /// </summary>
    /// <typeparam name="T">The type of additional data to store for the player.</typeparam>
    public class ClientMap<T> where T : class, new()
    {
        private Dictionary<int, HostPlayerData> _dictionary = new Dictionary<int, HostPlayerData>();
        private Dictionary<int, T> _dictionaryUserData = new Dictionary<int, T>();

        /// <summary>
        /// True if there are empty slots available, else false.
        /// </summary>
        public bool HasEmptySlots() => GetEmptySlot() != -1;

        /// <summary>
        /// Gets the player data for a peer.
        /// </summary>
        public HostPlayerData GetPlayerData(NetPeer peer) => _dictionary.ContainsKey(peer.Id) ? _dictionary[peer.Id] : null;

        /// <summary>
        /// Gets the custom data of a peer.
        /// </summary>
        public T GetCustomData(NetPeer peer) => _dictionaryUserData.ContainsKey(peer.Id) ? _dictionaryUserData[peer.Id] : null;

        /// <summary>
        /// Gets the custom data for all peers.
        /// </summary>
        public T[] GetCustomData() => _dictionaryUserData.Values.ToArray();

        /// <summary>
        /// Adds a peer ID to the dictionary.
        /// </summary>
        /// <returns>The slot of the peer. This is -1 if there are no slots available.</returns>
        public int AddPeer(NetPeer peer)
        {
            int emptySlot = GetEmptySlot();
            if (emptySlot != -1)
                _dictionary[peer.Id] = new HostPlayerData() { Name = "Unknown", ClientType = ClientKind.Client, PlayerIndex = emptySlot };
            else
                _dictionary[peer.Id] = new HostPlayerData() { Name = "Unknown", ClientType = ClientKind.Spectator, PlayerIndex = emptySlot };
            
            _dictionaryUserData[peer.Id] = new T();
            return emptySlot;
        }

        /// <summary>
        /// Sets the player data for a peer.
        /// </summary>
        public void AddOrUpdatePlayerData(NetPeer peer, HostPlayerData data)
        {
            if (!_dictionary.ContainsKey(peer.Id))
                AddPeer(peer);

            _dictionary[peer.Id].UpdateFromClient(data);
        }

        /// <summary>
        /// Gets the player data for all peers.
        /// </summary>
        public HostPlayerData[] GetPlayerData() => _dictionary.Values.ToArray();
        
        /// <summary>
        /// Gets the data for all peers.
        /// </summary>
        public DataCustomTuple[] GetData()
        {
            var custom = _dictionaryUserData.Values.ToArray();
            var host   = _dictionary.Values.ToArray();
            var tuples = new DataCustomTuple[custom.Length];
            for (int x = 0; x < tuples.Length; x++)
                tuples[x] = new DataCustomTuple(custom[x], host[x]);

            return tuples;
        }

        /// <summary>
        /// Removes a given peer from the player map.
        /// </summary>
        /// <param name="peer"></param>
        public void RemovePeer(NetPeer peer)
        {
            if (_dictionary.ContainsKey(peer.Id))
            {
                _dictionary.Remove(peer.Id);
                _dictionaryUserData.Remove(peer.Id);
            }
        }

        /// <summary>
        /// Converts the current dictionary to a message to send to the players.
        /// </summary>
        public HostSetPlayerData ToMessage(NetPeer excludePeer, HostPlayerData hostInfo)
        {
            var excludeIndex = GetPlayerData(excludePeer).PlayerIndex;
            return ToMessage(excludeIndex, hostInfo);
        }

        /// <summary>
        /// Converts the current dictionary to a message to send to the players.
        /// </summary>
        public HostSetPlayerData ToMessage(int excludeIndex, HostPlayerData hostInfo)
        {
            var values = new List<HostPlayerData>();
            values.Add(hostInfo);
            values.AddRange(_dictionary.Values.ToArray());
            return new HostSetPlayerData(values.Where(x => x.PlayerIndex != excludeIndex).ToArray(), excludeIndex);
        }

        /// <summary>
        /// Gets the first available empty slot, otherwise -1 if doesn't exist.
        /// </summary>
        private int GetEmptySlot()
        {
            // Index 0 reserved for host.
            for (int x = 1; x < Constants.MaxNumberOfPlayers; x++)
            {
                if (!_dictionary.Values.Any(y => y.PlayerIndex == x))
                    return x;
            }

            return -1;
        }

        public class DataCustomTuple
        {
            public T Custom;
            public HostPlayerData Host;

            public DataCustomTuple(T custom, HostPlayerData host)
            {
                Custom = custom;
                Host = host;
            }
        }
    }
}
