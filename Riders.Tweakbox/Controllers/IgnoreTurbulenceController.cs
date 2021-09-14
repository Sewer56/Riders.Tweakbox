using Riders.Tweakbox.Controllers.Interfaces;
using EnumsNET;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Input.Enums;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Api;

namespace Riders.Tweakbox.Controllers;

public unsafe class IgnoreTurbulenceController : IController
{
    private bool[] _toggleState = new bool[Sewer56.SonicRiders.API.Player.MaxNumberOfPlayers];
    private TweakboxConfig _config;

    public IgnoreTurbulenceController(TweakboxConfig config)
    {
        _config = config;
        EventController.CheckIfIgnoreTurbulence += CheckIfIgnoreTurbulence;
        EventController.AfterSetMovementFlagsOnInput += AfterSetMovementFlagsOnInput;
        ApiGearImplementation.OnInitGearStats += ResetToggleStates;
    }

    private Player* AfterSetMovementFlagsOnInput(Player* player)
    {
        if (!_config.Data.Modifiers.IgnoreTurbulenceOnToggle)
            return player;

        // Toggle State
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        if (player->PlayerInput->ButtonsPressed.HasAllFlags(Buttons.DPadUp))
            _toggleState[playerIndex] = !_toggleState[playerIndex];

        return player;
    }

    private void ResetToggleStates() => _toggleState = new bool[Sewer56.SonicRiders.API.Player.MaxNumberOfPlayers];

    public bool ShouldIgnoreTurbulence(Player* player)
    {
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        return _toggleState[playerIndex];
    }

    private unsafe Enum<AsmFunctionResult> CheckIfIgnoreTurbulence(Player* player)
    {
        // Check if feature enabled.
        if (!_config.Data.Modifiers.IgnoreTurbulenceOnToggle)
            return false;

        // Check for Guardian/Garden Turb
        if (*State.NumberOfRacers == player->MaybeClosestTurbulenceIndex)
            return false;

        return ShouldIgnoreTurbulence(player);
    }
}
