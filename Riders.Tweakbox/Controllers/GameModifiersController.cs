using System;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Reloaded.Hooks.Definitions;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Layout.Objects.ItemBox;
using Sewer56.SonicRiders.Parser.Layout.Enums;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.Configs;

namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Handles various game modifying features.
/// </summary>
public unsafe class GameModifiersController : IController
{
    /// <summary>
    /// The actual modifiers themselves.
    /// Please do not modify without calling 
    /// </summary>
    public ref GameModifiers Modifiers => ref _config.Data.Modifiers;

    /// <summary>
    /// Executed when the modifiers are modified.
    /// </summary>
    public event Action OnEditModifiers;

    private EventController _event;
    private ObjectLayoutController.ObjectLayoutController _layoutController;
    private TweakboxConfig _config;

    /// <summary>
    /// Creates the controller which controls game behaviour.
    /// </summary>

    public GameModifiersController(TweakboxConfig config)
    {
        _config = config;
        _event = IoC.GetSingleton<EventController>();
        _layoutController = IoC.GetSingleton<ObjectLayoutController.ObjectLayoutController>();

        _event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
        _event.ShouldSpawnTurbulence += ShouldSpawnTurbulence;
        _event.ShouldKillTurbulence += ShouldKillTurbulence;
        _event.ForceTurbulenceType += ForceTurbulenceType;
        _layoutController.OnLoadLayout += OnLoadLayout;
    }

    /// <summary>
    /// Updates the current set of game modifiers.
    /// </summary>
    /// <param name="modifiers">The new modifiers.</param>
    public void SetModifiers(in GameModifiers modifiers)
    {
        Modifiers = modifiers;
        InvokeOnEditModifiers();
    }

    /// <summary>
    /// Invokes an event indicating the modifiers have been edited.
    /// </summary>
    public void InvokeOnEditModifiers()
    {
        OnEditModifiers?.Invoke();
    }

    private unsafe Player* OnAfterSetMovementFlagsOnInput(Player* player)
    {
        // Remove Tornadoes if Modifier is set.
        if (Modifiers.DisableTornadoes)
            player->MovementFlags &= ~MovementFlags.Tornado;

        return player;
    }

    private void OnLoadLayout(ref InMemoryLayoutFile layout)
    {
        for (int x = 0; x < layout.Objects.Count; x++)
        {
            ref var obj = ref layout.Objects[x];
            if (obj.Type != ObjectId.oItemBox)
                continue;

            if (Modifiers.ReplaceAirMaxBox && obj.Attribute == (int)ItemBoxAttribute.AirMax)
                obj.Attribute = (int)Modifiers.AirMaxReplacement;

            if (Modifiers.ReplaceRing100Box && obj.Attribute == (int)ItemBoxAttribute.Ring100)
                obj.Attribute = (int)Modifiers.Ring100Replacement;
        }
    }

    private int ForceTurbulenceType(byte currentType)
    {
        if (Modifiers.DisableSmallTurbulence && currentType == 1)
            return 2;

        return currentType;
    }

    private unsafe bool ShouldKillTurbulence(Player* player, IHook<Functions.ShouldKillTurbulenceFn> hook)
    {
        if (Modifiers.NoTurbulence)
            return true;

        if (Modifiers.AlwaysTurbulence)
            return false;
        
        return hook.OriginalFunction(player);
    }

    private unsafe bool ShouldSpawnTurbulence(Player* player, IHook<Functions.ShouldGenerateTurbulenceFn> hook)
    {
        if (Modifiers.NoTurbulence)
            return false;

        if (Modifiers.AlwaysTurbulence)
            return true;

        return hook.OriginalFunction(player);
    }
}
