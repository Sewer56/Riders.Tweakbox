using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class PlayerState
    {
        /// <summary>
        /// Client's last latency readout.
        /// </summary>
        public int Latency = 999;

        /// <summary>
        /// Client has skipped intro cutscene and is ready to start the race.
        /// </summary>
        public bool ReadyToStartRace = false;
    }
}
