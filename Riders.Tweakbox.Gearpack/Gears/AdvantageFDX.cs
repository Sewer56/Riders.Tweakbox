using System;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Gearpack.Gears;

public class AdvantageFDX : CustomGearBase, IExtremeGear
{
    private TrickBehaviour _behaviour;

    public override string FolderName { get; set; } = "AdvantageF DX";

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void InitializeGear(string gearsFolder, Interfaces.ICustomGearApi gearApi)
    {
        _behaviour = new TrickBehaviour()
        {
            SetSpeedGain = SetSpeedGainFromTrick
        };
    }

    // IExtremeGear API Callbacks //
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