using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Controllers;

public unsafe class PitAirGainMultiplayerController : IController
{
    private TweakboxConfig _config;

    public PitAirGainMultiplayerController(TweakboxConfig config)
    {
        _config = config;
        EventController.SetPitAirGain += SetPitAirGain;
    }

    private unsafe void SetPitAirGain(ref int value, Player* player) => value = (int)(value * (_config.Data.Modifiers.PitAirGainMultiplier + 1));
}
