using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using System;
using System.Diagnostics;
using System.Linq;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class CommonState
    {
        public CommonState(HostPlayerData selfInfo)
        {
            SelfInfo = selfInfo;
            ResetRace();
        }

        public void ResetRace()
        {
            Array.Fill(RaceSync, new Timestamped<UnreliablePacketPlayer>());
            Array.Fill(MovementFlagsSync, new Timestamped<MovementFlagsMsg>());
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
        public int HandshakeTimeout = 5000;

        /// <summary>
        /// The currently enabled anti-cheat settings.
        /// </summary>
        public CheatKind AntiCheatMode;

        /// <summary>
        /// Contains information about other players.
        /// </summary>
        public HostPlayerData[] PlayerInfo = new HostPlayerData[0];

        /// <summary>
        /// Sync data for races.
        /// It is applied to the game at the start of the race event if not null.
        /// </summary>
        public Timestamped<UnreliablePacketPlayer>[] RaceSync = new Timestamped<UnreliablePacketPlayer>[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Contains movement flags for each client.
        /// </summary>
        public MovementFlagsMsg[] MovementFlagsSync = new MovementFlagsMsg[Constants.MaxNumberOfPlayers];

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
        /// Applies the current race state obtained from clients/host to the game.
        /// </summary>
        public void ApplyRaceSync()
        {
            // Apply data of all players.
            for (int x = 1; x < RaceSync.Length; x++)
            {
                var sync = RaceSync[x];
                if (sync.IsDiscard(MaxLatency))
                    continue;

                if (sync.Value.IsDefault())
                {
                    Trace.WriteLine("Discarding Race Packet due to Default Comparison");
                    continue;
                }

                sync.Value.ToGame(x);
            }
        }

        /// <summary>
        /// Handles all Boost/Tornado/Attack tasks received from the clients.
        /// </summary>
        public unsafe Sewer56.SonicRiders.Structures.Gameplay.Player* OnAfterSetMovementFlags(Sewer56.SonicRiders.Structures.Gameplay.Player* player)
        {
            var index = Player.GetPlayerIndex(player);

            if (index == 0)
                return player;

            MovementFlagsSync[index].ToGame(player);
            return player;
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

        /// <summary>
        /// Swaps spawn position of player 0 and the player's real index.
        /// </summary>
        public void SwapSpawns() => Sewer56.SonicRiders.API.Misc.SwapSpawnPositions(0, SelfInfo.PlayerIndex);

        public void OnSetSpawnLocationsStartOfRace(int value) => SwapSpawns();
        public unsafe Enum<AsmFunctionResult> OnCheckIfPlayerIsHuman(Sewer56.SonicRiders.Structures.Gameplay.Player* player) => IsHuman(Player.GetPlayerIndex(player));
    }
}
