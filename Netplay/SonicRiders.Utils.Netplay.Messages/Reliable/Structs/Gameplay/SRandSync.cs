using System;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct SRandSync
    {
        /// <summary>
        /// The Date/Time to resume the game at, synced to an external FTP server.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// The seed to apply to the clients.
        /// </summary>
        public int Seed;
        
        public SRandSync(DateTime startTime, int seed)
        {
            StartTime = startTime;
            Seed = seed;
        }
    }
}
