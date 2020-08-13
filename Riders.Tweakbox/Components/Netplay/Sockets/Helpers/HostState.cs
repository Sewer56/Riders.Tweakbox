using LiteNetLib;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class HostState : CommonState
    {
        public HostState(HostPlayerData selfInfo) : base(selfInfo) { }

        /// <summary>
        /// Stores a mapping of peers to players.
        /// </summary>
        public ClientMap<ClientState> ClientMap = new ClientMap<ClientState>();

        /// <summary>
        /// Translates a host player index into a local player index. 
        /// </summary>
        public override byte GetLocalPlayerIndex(int hostIndex) => (byte)hostIndex;

        /// <summary>
        /// Translates a host player index into a local player index. 
        /// </summary>
        public byte GetLocalPlayerIndex(NetPeer peer) => (byte)ClientMap.GetPlayerData(peer).PlayerIndex;

        /// <summary>
        /// Gets the index of a remote (on the host's end) player.
        /// </summary>
        public override int GetHostPlayerIndex(int localPlayerIndex) => localPlayerIndex;
    }
}
