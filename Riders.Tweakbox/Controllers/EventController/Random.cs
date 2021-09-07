using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Functions;
using static Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController
{
    /// <summary>
    /// Replaces the game's rand function if set. (Random number generator)
    /// </summary>
    public static event RandFnHandler Random;

    /// <summary>
    /// Replaces the game's srand function if set. (Seed random number generator)
    /// </summary>
    public static event SRandFnHandler SeedRandom;

    /// <summary>
    /// Replaces the game's rand function call for determining what item to give on pickup.
    /// </summary>
    public static event RandFnHandler ItemPickupRandom;

    /// <summary>
    /// Queries the user whether the character select menu should be left.
    /// </summary>
    public static event AsmFunc OnCheckIfGiveAiRandomItems;

    /// <summary>
    /// Checks if a specific player should be randomized inside character select after pressing start/enter and
    /// before loading the race.
    /// </summary>
    public static event PlayerAsmFunc OnCheckIfRandomizePlayer;

    private static IReverseWrapper<CdeclReturnIntFn> _randItemPickupWrapper;
    private static IHook<Functions.SRandFnPtr> _srandHook;
    private static IHook<Functions.RandFnPtr> _randHook;

    private static IAsmHook _onGetRandomDoubleInPlayerFunctionHook;
    private static IAsmHook _onCheckIfGiveAiRandomItemsHook;
    private static IAsmHook _checkIfRandomizePlayerHook;
    private static IAsmHook _alwaysSeedRngOnIntroSkipHook;

    private static Patch _randItemPickupPatch;
    private static Random _random = new Random();

    private void InitRandom(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        // Do not move below onCheckIfSkipIntroAsm because both overwrite same regions of code. You want the other to capture this one. 
        _alwaysSeedRngOnIntroSkipHook = hooks.CreateAsmHook(new[]
        {
            $"use32",
            $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, false)}"
        }, 0x00415F33, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        _srandHook = Functions.SRand.HookAs<SRandFnPtr>(typeof(EventController), nameof(SRandHandler)).Activate();
        _randHook = Functions.Rand.HookAs<RandFnPtr>(typeof(EventController), nameof(RandHandler)).Activate();

        // Get Random Double in Player Fn
        var onGetRandomDoubleInPlayerFunctionAsm = new[]
        {
            $"use32", 
            $"{utilities.AssembleAbsoluteCall<GetRandomDoublePtr>(typeof(EventController), nameof(TempNextDouble))}"
        };
        _onGetRandomDoubleInPlayerFunctionHook = hooks.CreateAsmHook(onGetRandomDoubleInPlayerFunctionAsm, 0x004E1FA7, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        // Random Items AI
        var ifGiveAiRandomItems = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004C721F, false) };
        var onCheckIfAiRandomItemsAsm = new[]
        {
            $"use32", 
            $"{utilities.AssembleAbsoluteCall(OnCheckIfGiveAiRandomItemsHook, ifGiveAiRandomItems, null, null)}"
        };
        _onCheckIfGiveAiRandomItemsHook = hooks.CreateAsmHook(onCheckIfAiRandomItemsAsm, 0x004C71F9).Activate();

        // Item Pickup
        _randItemPickupWrapper = hooks.CreateReverseWrapper<CdeclReturnIntFn>(ItemPickupRandImpl);
        _randItemPickupPatch = new Patch(0x004C714C, AsmHelpers.AssembleRelativeCall(0x004C714C, (long)_randItemPickupWrapper.WrapperPointer)).ChangePermission().Enable();

        // Randomize Player
        var ifNotRandomizePlayer = new[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004639DC, Environment.Is64BitProcess)}" };
        var ifRandomizePlayer = new[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004638E0, Environment.Is64BitProcess)}" };
        var onCheckIfRandomizePlayer = new[]
        {
            "use32",
            "push eax",
            "lea eax, dword [edi - 0xBA]",
            $"mov [{(int)_tempPlayerPointer.Pointer}], eax",
            "pop eax",
            $"{utilities.AssembleAbsoluteCall(OnCheckIfRandomizePlayerHook, ifRandomizePlayer, ifNotRandomizePlayer, null)}"
        };

        _checkIfRandomizePlayerHook = hooks.CreateAsmHook(onCheckIfRandomizePlayer, 0x004638D1, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    /// <summary>
    /// Invokes the random number seed generator. (Original Function)
    /// </summary>
    public void InvokeSeedRandom(int seed) => _srandHook.OriginalFunction.Value.Invoke((uint)seed);

    private int ItemPickupRandImpl() => ItemPickupRandom?.Invoke(_randHook) ?? RandHandlerImpl();

    private static int RandHandlerImpl() => Random?.Invoke(_randHook) ?? _randHook.OriginalFunction.Value.Invoke();


    [UnmanagedCallersOnly]
    private static double TempNextDouble() => _random.NextDouble() * -600.0;

    [UnmanagedCallersOnly]
    private static int RandHandler() => RandHandlerImpl();

    [UnmanagedCallersOnly]
    private static void SRandHandler(uint seed)
    {
        if (SeedRandom != null)
        {
            SeedRandom.Invoke(seed, _srandHook);
            return;
        }

        _srandHook.OriginalFunction.Value.Invoke(seed);
    }

    private Enum<AsmFunctionResult> OnCheckIfGiveAiRandomItemsHook() =>
        OnCheckIfGiveAiRandomItems != null && OnCheckIfGiveAiRandomItems.Invoke();

    private Enum<AsmFunctionResult> OnCheckIfRandomizePlayerHook() =>
        OnCheckIfRandomizePlayer?.Invoke(Sewer56.SonicRiders.API.Player.Players.Pointer) ?? AsmFunctionResult.Indeterminate;

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate double GetRandomDouble();

    [Function(CallingConventions.Cdecl)]
    public struct GetRandomDoublePtr { public FuncPtr<double> Value; }

    public unsafe delegate void SRandFnHandler(uint seed, IHook<Functions.SRandFnPtr> hook);
    public unsafe delegate int RandFnHandler(IHook<Functions.RandFnPtr> hook);
}
