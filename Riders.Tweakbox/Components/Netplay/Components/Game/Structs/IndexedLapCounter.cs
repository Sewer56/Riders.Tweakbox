using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;

namespace Riders.Tweakbox.Components.Netplay.Components.Game.Structs;

public struct IndexedLapCounter
{
    public int Index;
    public LapCounter Counter;

    public IndexedLapCounter(int index, LapCounter counter)
    {
        Index = index;
        Counter = counter;
    }
}