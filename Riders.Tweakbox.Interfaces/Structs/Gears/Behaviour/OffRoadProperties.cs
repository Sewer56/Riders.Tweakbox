using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Enums;

namespace Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

public class OffRoadProperties
{
    /// <summary>
    /// True if the gear should ignore off-road speed loss.
    /// </summary>
    public bool? IgnoreSpeedLoss = true;

    /// <summary>
    /// Checks if the gear should ignore speed loss.
    /// </summary>
    public QueryValueHandler<QueryResult> CheckIfIgnoreSpeedLoss;
}
