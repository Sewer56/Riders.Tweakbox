using System;
using System.Diagnostics;
using System.Numerics;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Layout.Objects.ItemBox;
using Sewer56.SonicRiders.Parser.Layout.Enums;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.Configs;
using PlayerAPI = Sewer56.SonicRiders.API.Player;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Controllers.Modifiers;
using Sewer56.SonicRiders.API;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using Riders.Tweakbox.Misc.Log;

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

    /// <summary>
    /// Provides the slipstream implementation.
    /// </summary>
    public SlipstreamModifier Slipstream;

    private EventController _event;
    private ObjectLayoutController.ObjectLayoutController _layoutController;
    private TweakboxConfig _config;
    private IAsmHook _noBerserkerOnTurbulence;

    private IAsmHook _setBreakableObjectMinRespawnRadius;
    private Pinnable<float> _newRespawnRadius = new Pinnable<float>(20.0f);

    /// <summary>
    /// Creates the controller which controls game behaviour.
    /// </summary>

    public GameModifiersController(TweakboxConfig config, IReloadedHooks hooks)
    {
        _config = config;
        _event = IoC.GetSingleton<EventController>(); // Ensure load order.
        _layoutController = IoC.GetSingleton<ObjectLayoutController.ObjectLayoutController>();
        Slipstream = new SlipstreamModifier(this);

        _noBerserkerOnTurbulence = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"cmp byte [eax+0x11BC], 0x10", // Check player state; null if turbulence.
            $"jne exit",
            "push esi",
            $"{hooks.Utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4CFDAE, false)}", // Dip out of speed loss func.
            $"exit:"
        }, 0x4CFD74, new AsmHookOptions() { Behaviour = AsmHookBehaviour.ExecuteFirst, MaxOpcodeSize = 6 }).Activate();

        _setBreakableObjectMinRespawnRadius = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"movss xmm0, [{(IntPtr)_newRespawnRadius.Pointer}]"
        }, 0x4A2C05, new AsmHookOptions() { Behaviour = AsmHookBehaviour.DoNotExecuteOriginal }).Activate();

        EventController.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
        EventController.ShouldSpawnTurbulence += ShouldSpawnTurbulence;
        EventController.ShouldKillTurbulence += ShouldKillTurbulence;
        EventController.ForceTurbulenceType += ForceTurbulenceType;
        EventController.AfterRunPhysicsSimulation += OnAfterRunPhysicsSimulation;
        EventController.OnShouldRejectAttackTask += OnShouldRejectAttackTask;
        EventController.SetRingsOnHit += SetRingsOnHit;
        EventController.SetRingsOnDeath += SetRingsOnDeath;
        EventController.OnShouldRejectAttackTask += ShouldRejectAttack;
        EventController.SetItemboxRespawnTimer += SetItemboxRespawnTimer;
        _layoutController.OnLoadLayout += OnLoadLayout;
    }

    // Note: Do not subscribe to Slipstream directly as that creates a struct copy.
    private int OnAfterRunPhysicsSimulation() => Slipstream.OnAfterRunPhysicsSimulation();

    private int ShouldRejectAttack(Player* playerone, Player* playertwo, int a3)
    {
        if (State.StageTimer->ToTimeSpan() <= Modifiers.GetDisableAttackDuration())
            return 1;

        return 0;
    }

    /// <summary>
    /// Updates the current set of game modifiers.
    /// </summary>
    /// <param name="modifiers">The new modifiers.</param>
    public void SetModifiers(GameModifiers modifiers)
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
        if (Modifiers.BerserkerTurbulenceFix)
            _noBerserkerOnTurbulence.Enable();
        else
            _noBerserkerOnTurbulence.Disable();

        // Apply breakable item properties.
        if (Modifiers.BreakableItemSettings != null)
        {
            ref var settings = ref Modifiers.BreakableItemSettings;
            _newRespawnRadius.Value = settings.NoRespawnRadius;
            ApplyBreakableItemProps(ref settings.PutBreak00_03, (byte*)0x497A5C, (int*)0x497AFC);
            ApplyBreakableItemProps(ref settings.PutBreak01, (byte*)0x497B78, (int*)0x497B99);
            ApplyBreakableItemProps(ref settings.PutBreak02, (byte*)0x497C12, (int*)0x497C6E);
            ApplyBreakableItemProps(ref settings.PutBreak04, (byte*)0x4A22CA, (int*)0x4A26CA);

            void ApplyBreakableItemProps(ref BreakableItemSettings.PutBreakItemSettings putBreakSettings, byte* vanishAnimationFramesPtr, int* respawnFramesPtr)
            {
                *vanishAnimationFramesPtr = putBreakSettings.VanishAnimationFrames;
                *respawnFramesPtr = putBreakSettings.RespawnTimerFrames;
            }
        }
    }

    /// <summary>
    /// Returns true if a client side attack should be rejected.
    /// </summary>
    public bool ShouldRejectAttack()
    {
        if (State.StageTimer->ToTimeSpan() <= Modifiers.GetDisableAttackDuration())
            return true;

        if (Modifiers.DisableAttacks)
            return true;

        return false;
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

            if (Modifiers.ReplaceMaxAirSettings.Enabled && obj.Attribute == (int)ItemBoxAttribute.AirMax)
                obj.Attribute = (int)Modifiers.ReplaceMaxAirSettings.Replacement;

            if (Modifiers.ReplaceRing100Settings.Enabled && obj.Attribute == (int)ItemBoxAttribute.Ring100)
                obj.Attribute = (int)Modifiers.ReplaceRing100Settings.Replacement;
        }
    }

    private void SetRingsOnHit(Player* player) => SetRingsOnEvent(player, Modifiers.HitRingLoss);

    private void SetRingsOnDeath(Player* player) => SetRingsOnEvent(player, Modifiers.DeathRingLoss);

    private void SetRingsOnEvent(Player* player, in RingLossBehaviour behaviour)
    {
        if (!behaviour.Enabled)
        {
            player->Rings = 0;
            return;
        }

        player->Rings = CalcRingLoss(player->Rings, behaviour);
    }

    private int CalcRingLoss(int originalRings, in RingLossBehaviour behaviour)
    {
        var ringLossMultiplier = (100f - behaviour.RingLossPercentage) / 100f;
        var rings = originalRings - behaviour.RingLossBefore;
        rings = (int)(rings * ringLossMultiplier);
        rings -= behaviour.RingLossAfter;

        if (rings < 0)
            rings = 0;

        return rings;
    }

    /// <summary>
    /// Reject attacks if necessary.
    /// </summary>
    private unsafe int OnShouldRejectAttackTask(Player* playerone, Player* playertwo, int a3)
    {
        if (Modifiers.DisableAttacks)
            return 1;

        return 0;
    }

    private int ForceTurbulenceType(byte currentType)
    {
        if (Modifiers.DisableSmallTurbulence && currentType == 1)
            return 2;

        return currentType;
    }

    private unsafe bool ShouldKillTurbulence(Player* player, IHook<Functions.ShouldKillTurbulenceFnPtr> hook)
    {
        if (IsNonPlayerTurbulence(player))
            return Convert.ToBoolean(hook.OriginalFunction.Value.Invoke(player));

        if (Modifiers.NoTurbulence)
            return true;

        if (Modifiers.AlwaysTurbulence)
            return false;
        
        return Convert.ToBoolean(hook.OriginalFunction.Value.Invoke(player));
    }

    private unsafe bool ShouldSpawnTurbulence(Player* player, IHook<Functions.ShouldGenerateTurbulenceFnPtr> hook)
    {
        if (IsNonPlayerTurbulence(player))
            return Convert.ToBoolean(hook.OriginalFunction.Value.Invoke(player));

        if (Modifiers.NoTurbulence)
            return false;

        if (Modifiers.AlwaysTurbulence)
            return true;

        return Convert.ToBoolean(hook.OriginalFunction.Value.Invoke(player));
    }

    private int SetItemboxRespawnTimer()
    {
        return _config.Data.Modifiers.ItemBoxProperties.RespawnTimerFrames;
    }

    private bool IsNonPlayerTurbulence(Player* player)
    {
        var index = PlayerAPI.GetPlayerIndex(player);
        return index >= *State.NumberOfRacers || index < 0;
    }
}
