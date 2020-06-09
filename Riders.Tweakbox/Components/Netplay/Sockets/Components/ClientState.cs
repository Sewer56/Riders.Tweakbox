using System;
using System.Collections.Generic;
using System.Text;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Components
{
    public class ClientState
    {
        public int FrameCounter;
        public CheatKind AntiCheatMode;
        public HostPlayerData[] PlayerInfo = new HostPlayerData[0];
    }
}
