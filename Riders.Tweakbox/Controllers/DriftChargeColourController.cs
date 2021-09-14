using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Misc;

namespace Riders.Tweakbox.Controllers;

public unsafe class DriftChargeColourController : IController
{
    private TweakboxConfig _config;

    public DriftChargeColourController(TweakboxConfig config)
    {
        _config = config;
        EventController.SetExhaustTrailColour += SetExhaustTrailColour;
    }

    private unsafe void SetExhaustTrailColour(ColorRGBA* value, Player* player)
    {
        if (_config.Data.DriftChargeColour.HasValue && player->CurrentDriftFrames >= player->FramesToGenerateDriftDash)
            *value = _config.Data.DriftChargeColour.Value;
    }
}
