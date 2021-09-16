using System.Diagnostics;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Buffers;
using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using Void = Reloaded.Hooks.Definitions.Structs.Void;
namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    private static MemoryBufferHelper _memoryBufferHelper;
    private static unsafe Pinnable<BlittablePointer<Player>> _tempPlayerPointer = new Pinnable<BlittablePointer<Player>>(new BlittablePointer<Player>());

    public EventController(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _memoryBufferHelper = new MemoryBufferHelper(Process.GetCurrentProcess());

        /* DO NOT REORDER; THERE ARE STACKING HOOKS HERE. */
        InitRandom(hooks, utilities);
        InitMenu(hooks, utilities);

        // These are OK to reorder.
        InitSetSpawnLocationsStartOfRace();
        InitStartAttackTask();
        InitSetMovementFlagsOnInput();
        InitSetNewPlayerStateHandler();
        InitSetItemPickupTaskHandler();

        InitCheckIfPitSkipRenderGauge(hooks, utilities);
        InitCheckIfPlayerIsHumanInput(hooks, utilities);
        InitCheckIfPlayerIsHumanIndicator(hooks, utilities);
        InitOnStartRace(hooks, utilities);
        InitRaceSkipIntro(hooks, utilities);

        InitOnSetupRace(hooks, utilities);
        InitSetGoalRaceFinishTask(hooks, utilities);
        InitUpdateLapCounter(hooks, utilities);
        InitGoalRaceFinishTask(hooks, utilities);
        InitRemoveAllTasks(hooks, utilities);

        InitRunPhysicsSimulation(hooks, utilities);
        InitPauseGame(hooks, utilities);
        InitSetEndOfRaceDialog(hooks, utilities);
        InitShouldSpawnTurbulence(hooks, utilities);
        InitShouldKillTurbulence(hooks, utilities);

        InitForceTurbulenceType(hooks, utilities);
        InitSetDashPanelSpeed(hooks, utilities);
        InitSetSpeedshoesSpeed(hooks, utilities);
        InitSetRingsOnHit(hooks, utilities);
        InitSetRingsOnDeath(hooks, utilities);

        InitSetAirGainedLastFrame(hooks, utilities);
        InitSetAirFromTypes(hooks, utilities);
        InitOffroadSpeedLoss(hooks, utilities);
        InitSetShortcutSpeedCap(hooks, utilities);
        InitForceLegendEffect(hooks, utilities);
        InitSetDriftCap(hooks, utilities);
        InitSpeedLossFromWallHit(hooks, utilities);

        InitForceLevelUpInRace(hooks, utilities);
        InitBoostChainMultiplier(hooks, utilities);
        InitModifyBoostDuration(hooks, utilities);
        InitTrickLandSpeed(hooks, utilities);
        InitSetExhaustTrail(hooks, utilities);
        InitSetTornadoDeceleration(hooks, utilities);
        InitOnCollectRing(hooks, utilities);
        InitSetPitAirGain(hooks, utilities);
        InitSetRunStateSpeed(hooks, utilities);
        InitIgnoreTurbulenceCollision(hooks, utilities);
        InitCheckIfSetAfterBoostFunction(hooks, utilities);
    }

    // Common. Do not Move.
    [Function(CallingConventions.Stdcall)]
    public delegate Enum<AsmFunctionResult> PlayerAsmFunc(Player* player);

    [Function(CallingConventions.Stdcall)]
    public struct PlayerAsmFuncPtr { public FuncPtr<BlittablePointer<Player>, Enum<AsmFunctionResult>> Value; }

    /// <summary>
    /// Generic function that acts upon a player.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate void GenericPlayerFn(Player* player);

    [Function(CallingConventions.Stdcall)]
    public struct GenericPlayerFnPtr { public FuncPtr<BlittablePointer<Player>, Void> Value; }

    /// <summary>
    /// Generic function that modifies a float in memory.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate void GenericModifyFloatFn(ref float value);

    [Function(CallingConventions.Stdcall)]
    public struct GenericModifyFloatFnPtr { public FuncPtr<float, float> Value; }

    /// <summary>
    /// Generic function that modifies a float in memory for a given player.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate void GenericModifyPlayerFloatFn(ref float value, Player* player);

    [Function(CallingConventions.Stdcall)]
    public struct GenericModifyPlayerFloatFnPtr { public FuncPtr<float, BlittablePointer<Player>, float> Value; }

    /// <summary>
    /// Generic function that modifies a float in memory for a given player.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate void GenericModifyPlayerIntFn(ref int value, Player* player);

    [Function(CallingConventions.Stdcall)]
    public struct GenericModifyPlayerIntFnPtr { public FuncPtr<int, BlittablePointer<Player>, int> Value; }

    /// <summary>
    /// Generic function that modifies a float in memory for a given player.
    /// </summary>
    [Function(CallingConventions.Stdcall)]
    public delegate bool PerformOperationOnPlayerFn(Player* player);

    [Function(CallingConventions.Stdcall)]
    public struct PerformOperationOnPlayerFnPtr { public FuncPtr<BlittablePointer<Player>, byte> Value; }

    /// <summary/>
    public delegate byte ReturnByteFnHandler(IHook<Functions.CdeclReturnByteFnPtr> hook);

    /// <summary/>
    public delegate int ReturnIntFnHandler(IHook<Functions.CdeclReturnIntFnPtr> hook);
}
