using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;

namespace Riders.Tweakbox.Gearpack.Gears;

public class TurboStarDX : CustomGearBase, IExtremeGear
{
    private ExtendedLevelStats _extendedStats = new ExtendedLevelStats()
    {
        Enabled = true,
        ExtendedStats = new[]
        {
            new ExtendedExtremeGearLevelStats()
            {
                RingCount = 0,
                MaxAir = 100000,
                PassiveAirDrain = 0,
                DriftAirCost = 166,
                BoostCost = 25000,
                TornadoCost = 25000,
                SpeedGainedFromDriftDash = 0.300926f,
                BoostSpeed = 0.925926f
            },
            new ExtendedExtremeGearLevelStats()
            {
                RingCount = 30,
                MaxAir = 150000,
                PassiveAirDrain = 0,
                DriftAirCost = 250,
                BoostCost = 30000,
                TornadoCost = 30000,
                SpeedGainedFromDriftDash = 0.439815f,
                BoostSpeed = 1.043056f,
            },
            new ExtendedExtremeGearLevelStats()
            {
                RingCount = 60,
                MaxAir = 200000,
                PassiveAirDrain = 0,
                DriftAirCost = 333,
                BoostCost = 40000,
                TornadoCost = 40000,
                SpeedGainedFromDriftDash = 0.532407f,
                BoostSpeed = 1.135648f,
            },
            new ExtendedExtremeGearLevelStats()
            {
                RingCount = 90,
                MaxAir = 200000,
                PassiveAirDrain = 0,
                DriftAirCost = 333,
                BoostCost = 40000,
                TornadoCost = 40000,
                SpeedGainedFromDriftDash = 0.532407f,
                BoostSpeed = 1.203704f,
            }
        }
    };

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "TurboStar DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }


    public ExtendedLevelStats GetExtendedLevelStats() => _extendedStats;
}
