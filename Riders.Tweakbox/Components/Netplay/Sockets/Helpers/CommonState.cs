using System;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using System.Linq;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using StructLinq;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class CommonState
    {
        public CommonState(PlayerData selfInfo, Socket owner)
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
        public PlayerData SelfInfo;

        /// <summary>
        /// Current frame counter for the client/server.
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// Packets older than this will be discarded.
        /// </summary>
        public int MaxLatency = 1000;

        /// <summary>
        /// Timeout for various handshakes such as initial exchange of game/gear data or start line synchronization.
        /// </summary>
        public int HandshakeTimeout = 8000;

        /// <summary>
        /// Contains information about other players.
        /// For client, index 0 is guaranteed to contain host.
        /// For host, index 0 will contain player 1.
        /// </summary>
        public PlayerData[] PlayerInfo = new PlayerData[0];

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
            if (PlayerInfo.Length > 0)
                return SelfInfo.NumPlayers + PlayerInfo.ToStructEnumerable().Sum(x => x.NumPlayers, x => x);

            return SelfInfo.NumPlayers;
        }

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
            if (playerIndex > selfIndex + NumLocalPlayers)
                playerIndex -= NumLocalPlayers;

            return NumLocalPlayers + playerIndex;
        }

        /// <summary>
        /// True if a player index is a human, else false.
        /// </summary>
        public bool IsHuman(int playerIndex)
        {
            // Compare against highest player index.
            if (PlayerInfo.Length > 0)
            {
                var highestRemoteIndex = PlayerInfo.ToStructEnumerable().Select(x => x.PlayerIndex + x.NumPlayers).Max();
                var highestLocalIndex = SelfInfo.PlayerIndex + SelfInfo.NumPlayers;
                    
                return playerIndex < Math.Max(highestLocalIndex, highestRemoteIndex);
            }

            return IsLocal(playerIndex);
        }

        /// <summary>
        /// Determines if the player is a local player (on this machine).
        /// </summary>
        /// <param name="playerIndex">The index of the player.</param>
        public bool IsLocal(int playerIndex)
        {
            return playerIndex < NumLocalPlayers;
        }
    }
}
