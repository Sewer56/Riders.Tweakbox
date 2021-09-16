using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.Hooks.Utilities.Enums;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Controllers;

public unsafe class NormalizeBoostDurationController : IController
{
    private TweakboxConfig _config;
    private EventController _eventController;

    public NormalizeBoostDurationController(TweakboxConfig config)
    {
        _config = config;
        _eventController = IoC.GetSingleton<EventController>();
        
        EventController.CheckIfDisableAfterBoostFunction += CheckIfDisableAfterBoost;
    }

    private unsafe Enum<AsmFunctionResult> CheckIfDisableAfterBoost(Player* player) => _config.Data.Modifiers.NormalizedBoostDurations;
}
