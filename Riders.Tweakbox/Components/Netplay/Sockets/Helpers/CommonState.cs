using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using System;
using System.Linq;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class CommonState
    {
        public CommonState(HostPlayerData selfInfo)
        {
            SelfInfo = selfInfo;
        }

        /// <summary>
        /// Contains information about own player.
        /// </summary>
        public HostPlayerData SelfInfo;

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
        /// The currently enabled anti-cheat settings.
        /// </summary>
        public CheatKind AntiCheatMode;

        /// <summary>
        /// Contains information about other players.
        /// </summary>
        public HostPlayerData[] PlayerInfo = new HostPlayerData[0];

        /// <summary>
        /// Returns the total count of players.
        /// </summary>
        public int GetPlayerCount()
        {
            if (PlayerInfo.Length > 0)
                return Math.Max(PlayerInfo.Max(x => x.PlayerIndex) + 1, SelfInfo.PlayerIndex + 1);

            return 1;
        }

        /// <summary>
        /// Gets the index of a remote (on the host's end) player.
        /// </summary>
        public virtual int GetHostPlayerIndex(int localPlayerIndex)
        {
            if (localPlayerIndex == 0)
                return SelfInfo.PlayerIndex;

            return PlayerInfo[localPlayerIndex - 1].PlayerIndex;
        }

        /// <summary>
        /// Translates a host player index into a local player index. 
        /// </summary>
        public virtual byte GetLocalPlayerIndex(int hostIndex)
        {
            var selfIndex = SelfInfo.PlayerIndex;

            // e.g. Client 1 : Host 0
            // e.g. Client Index 1 | Host: 1, Client 0
            //      Client Index 1 | Host: 2, Client
            if (hostIndex == selfIndex)
                return 0;
            if (hostIndex < selfIndex)
                return (byte) (hostIndex + 1);
            
            return (byte) hostIndex;
        }

        /// <summary>
        /// True if a player index is a human, else false.
        /// </summary>
        public bool IsHuman(int playerIndex)
        {
            if (playerIndex == SelfInfo.PlayerIndex)
                return true;

            return PlayerInfo.Any(x => x.PlayerIndex == playerIndex);
        }
    }
}
