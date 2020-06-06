using Sewer56.SonicRiders.API;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public unsafe struct LapCounters
    {
        public fixed byte Counters[Player.MaxNumberOfPlayers - 1];
    }
}