using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Misc;

namespace Riders.Tweakbox.Controllers;

public unsafe class TrailColourController : IController
{
    private TweakboxConfig _config;
    private IgnoreTurbulenceController _ignoreTurbulenceController;

    public TrailColourController(TweakboxConfig config)
    {
        _config = config;
        _ignoreTurbulenceController = IoC.GetSingleton<IgnoreTurbulenceController>();
        EventController.SetExhaustTrailColour += SetExhaustTrailColour;
    }

    private unsafe void SetExhaustTrailColour(ColorRGBA* value, Player* player)
    {
        // Turbulence Colour
        if (_ignoreTurbulenceController.ShouldIgnoreTurbulence(player))
            *value = _config.Data.IgnoreTurbulenceColour;

        // Drift Charge Colour
        if (_config.Data.DriftChargeColour.HasValue && player->CurrentDriftFrames >= player->FramesToGenerateDriftDash)
            *value = _config.Data.DriftChargeColour.Value;
    }
}
