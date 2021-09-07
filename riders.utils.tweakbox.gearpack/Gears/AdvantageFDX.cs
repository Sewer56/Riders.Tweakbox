using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AdvantageFDX : CustomGearBase, IExtremeGear
{
    private TrickBehaviour _behaviour;

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        _behaviour = new TrickBehaviour()
        {
            Enabled = true,
            SetSpeedGain = SetSpeedGainFromTrick
        };

        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "AdvantageF DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }

    public TrickBehaviour GetTrickBehaviour() => _behaviour;

    private unsafe float SetSpeedGainFromTrick(IntPtr playerPtr, int playerIndex, int playerLevel, float speed)
    {
        var player = (Player*) playerPtr;

        switch (player->Level)
        {
            case 0:
                speed *= 1.1f;
                break;

            case 1:
                speed *= 1.2f;
                break;

            case 2:
                speed *= 1.3f;
                break;
        }

        return speed;
    }
}