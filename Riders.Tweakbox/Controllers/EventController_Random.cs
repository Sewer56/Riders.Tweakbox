using System;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Functions;
namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController
{
    /// <summary>
    /// Replaces the game's rand function if set. (Random number generator)
    /// </summary>
    public event RandFn Random;

    /// <summary>
    /// Replaces the game's srand function if set. (Seed random number generator)
    /// </summary>
    public event SRandFn SeedRandom;

    /// <summary>
    /// Replaces the game's rand function call for determining what item to give on pickup.
    /// </summary>
    public event RandFn ItemPickupRandom;

    /// <summary>
    /// Queries the user whether the character select menu should be left.
    /// </summary>
    public event AsmFunc OnCheckIfGiveAiRandomItems;

    /// <summary>
    /// Checks if a specific player should be randomized inside character select after pressing start/enter and
    /// before loading the race.
    /// </summary>
    public event PlayerAsmFunc OnCheckIfRandomizePlayer;

    private IReverseWrapper<Functions.CdeclReturnIntFn> _randItemPickupWrapper;
    private IHook<Functions.SRandFn> _srandHook;
    private IHook<Functions.RandFn> _randHook;

    private IAsmHook _onGetRandomDoubleInPlayerFunctionHook;
    private IAsmHook _onCheckIfGiveAiRandomItemsHook;
    private IAsmHook _checkIfRandomizePlayerHook;
    private IAsmHook _alwaysSeedRngOnIntroSkipHook;

    private Patch _randItemPickupPatch;
    private Random _random = new Random();

    private void Constructor_Random(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        // Do not move below onCheckIfSkipIntroAsm because both overwrite same regions of code. You want the other to capture this one. 
        _alwaysSeedRngOnIntroSkipHook = hooks.CreateAsmHook(new[] { $"use32", $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, false)}" }, 0x00415F33, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
        _srandHook = Functions.SRand.Hook(SRandHandler).Activate();
        _randHook = Functions.Rand.Hook(RandHandler).Activate();

        var onGetRandomDoubleInPlayerFunctionAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall<GetRandomDouble>(TempNextDouble, out _)}" };
        _onGetRandomDoubleInPlayerFunctionHook = hooks.CreateAsmHook(onGetRandomDoubleInPlayerFunctionAsm, 0x004E1FA7, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        var ifGiveAiRandomItems = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004C721F, false) };
        var onCheckIfAiRandomItemsAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfGiveAiRandomItemsHook, out _, ifGiveAiRandomItems, null, null)}" };
        _onCheckIfGiveAiRandomItemsHook = hooks.CreateAsmHook(onCheckIfAiRandomItemsAsm, 0x004C71F9).Activate();

        _randItemPickupWrapper = hooks.CreateReverseWrapper<Functions.CdeclReturnIntFn>(ItemPickupRandImpl);
        _randItemPickupPatch = new Patch(0x004C714C, AsmHelpers.AssembleRelativeCall(0x004C714C, (long)_randItemPickupWrapper.WrapperPointer)).ChangePermission().Enable();

        var ifNotRandomizePlayer = new[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004639DC, Environment.Is64BitProcess)}" };
        var ifRandomizePlayer = new[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004638E0, Environment.Is64BitProcess)}" };
        var onCheckIfRandomizePlayer = new[]
        {
            "use32",
            "push eax",
            "lea eax, dword [edi - 0xBA]",
            $"mov [{(int)_tempPlayerPointer.Pointer}], eax",
            "pop eax",
            $"{utilities.AssembleAbsoluteCall(OnCheckIfRandomizePlayerHook, out _, ifRandomizePlayer, ifNotRandomizePlayer, null)}"
        };
        _checkIfRandomizePlayerHook = hooks.CreateAsmHook(onCheckIfRandomizePlayer, 0x004638D1, AsmHookBehaviour.ExecuteFirst).Activate();
    }

    /// <summary>
    /// Invokes the random number seed generator. (Original Function)
    /// </summary>
    public void InvokeSeedRandom(int seed) => _srandHook.OriginalFunction((uint)seed);

    private double TempNextDouble() => _random.NextDouble() * -600.0;
    private int ItemPickupRandImpl() => ItemPickupRandom?.Invoke(_randHook) ?? RandHandler();
    private int RandHandler() => Random?.Invoke(_randHook) ?? _randHook.OriginalFunction();

    private void SRandHandler(uint seed)
    {
        if (SeedRandom != null)
        {
            SeedRandom.Invoke(seed, _srandHook);
            return;
        }

        _srandHook.OriginalFunction(seed);
    }

    private Enum<AsmFunctionResult> OnCheckIfGiveAiRandomItemsHook() =>
        OnCheckIfGiveAiRandomItems != null && OnCheckIfGiveAiRandomItems.Invoke();

    private Enum<AsmFunctionResult> OnCheckIfRandomizePlayerHook() =>
        OnCheckIfRandomizePlayer?.Invoke(Sewer56.SonicRiders.API.Player.Players.Pointer) ?? AsmFunctionResult.Indeterminate;

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate double GetRandomDouble();

    public unsafe delegate void SRandFn(uint seed, IHook<Functions.SRandFn> hook);
    public unsafe delegate int RandFn(IHook<Functions.RandFn> hook);
}
