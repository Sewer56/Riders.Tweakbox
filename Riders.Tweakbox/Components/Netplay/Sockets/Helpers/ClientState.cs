using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class ClientState
    {
        public int FrameCounter;
        public CheatKind AntiCheatMode;
        public HostPlayerData[] PlayerInfo = new HostPlayerData[0];
        public int PlayerIndex;

        /// <summary>
        /// A start of the race has been requested.
        /// </summary>
        public bool StartRequested = false;

        /// <summary>
        /// Character select synchronization data for client.
        /// </summary>
        public ConcurrentQueue<CharaSelectSync> CharaSelectSync = new ConcurrentQueue<CharaSelectSync>();

        /// <summary>
        /// Stage intro cutscene skip requested by host.
        /// </summary>
        public bool SkipRequested = false;

        /// <summary>
        /// Gets the go command from the host for syncing start time.
        /// </summary>
        public SyncStartGo? StartSyncGo = null;
    }
}
