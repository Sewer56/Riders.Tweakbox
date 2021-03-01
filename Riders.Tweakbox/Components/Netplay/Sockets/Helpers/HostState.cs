using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class HostState : CommonState
    {
        public HostState(PlayerData selfInfo, Socket owner) : base(selfInfo, owner)
        {
            ClientMap = new ClientMap(selfInfo);
        }

        /// <summary>
        /// Stores a mapping of peers to players.
        /// </summary>
        public ClientMap ClientMap;
        
        /// <summary>
        /// Gets the <see cref="PlayerData"/> belonging to the host.
        /// </summary>
        public override PlayerData GetHostData() => SelfInfo;

        /// <summary>
        /// Translates a host player index into a local player index. 
        /// </summary>
        public override int GetLocalPlayerIndex(int playerIndex) => playerIndex;

        /// <summary>
        /// Converts a local player index to an index on the host's end.
        /// </summary>
        public override int GetHostPlayerIndex(int playerIndex) => playerIndex;
    }
}
