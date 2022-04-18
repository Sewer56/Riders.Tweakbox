using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe partial class EventController : TaskEvents, IController
{
    /// <summary>
    /// Sets the player's speed cap for a given rail. Executed every frame.
    /// </summary>
    public static event ModifyPlayerRailSpeedFn SetRailSpeedCap;

    /// <summary>
    /// Executed after the rail speed cap is set.
    /// </summary>
    public static event ModifyPlayerRailSpeedFn AfterSetRailSpeedCap;

    /// <summary>
    /// Sets the player's initial speed cap for a given rail. Executed once per rail.
    /// </summary>
    public static event ModifyPlayerRailSpeedFn SetRailInitialSpeedCap;

    /// <summary>
    /// Set before the fly speed for a player is set as they hit a fly ring.
    /// </summary>
    public static event GenericModifyPlayerFloatFn OnSetFlyRingSpeed;

    /// <summary>
    /// Sets the player's horizontal speed for a fly ring.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetFlyRingSpeedX;

    /// <summary>
    /// Sets the player's vertical speed for a fly ring.
    /// </summary>
    public static event GenericModifyPlayerFloatFn SetFlyRingSpeedY;

    private static IAsmHook _setGrindRailSpeedCapHook;
    private static IAsmHook _setRailSpeedCapHook;
    private static IAsmHook _setFlyHoopSpeedXHook;
    private static IAsmHook _setFlyHoopSpeedYHook;

    private static int[] _playerRailTimeSpentFrames = new int[Sewer56.SonicRiders.API.Player.MaxNumberOfPlayers];

    public void InitSetShortcutSpeedCap(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _setGrindRailSpeedCapHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.PushXmmRegisterFloat("xmm1")}",
            $"push esi",
            $"{utilities.PushXmmRegisterFloat("xmm0")}",
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetPlayerGrindInitialSpeedCap), false)}",
            $"{utilities.PopFromX87ToXmm()}",
            $"{utilities.PopXmmRegisterFloat("xmm1")}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",

        }, 0x4E686F, AsmHookBehaviour.ExecuteFirst).Activate();

        _setRailSpeedCapHook = hooks.CreateAsmHook(new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"push esi",
            $"{utilities.AssembleAbsoluteCall<GenericPlayerFnPtr>(typeof(EventController), nameof(OnGrinding), false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}"
        }, 0x4E69D2, AsmHookBehaviour.ExecuteFirst).Activate();

        const int fpuAlignment = 16;
        const int fpuBytes = 108;
        var fpuBackupAllocation = _memoryBufferHelper.AllocateAligned(fpuBytes, fpuAlignment);
        _setFlyHoopSpeedXHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",

            $"fnsave [{fpuBackupAllocation}]", // Store FPU State
            $"push ebx",                // Push Player Ptr
            $"push dword [esi + 0x44]", // Float Value
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetFlySpeedXHook), false)}",
            $"{utilities.PopFromX87ToRegister()}",
            $"frstor [{fpuBackupAllocation}]",                   // Restore FPU State
            $"{utilities.MultiplyFromRegisterToX87()}",   // Original Code: Override multiply
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            
            // Original Code
            "fstp dword [ebx+0BE0h]"
        }, 0x4C7F72, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        string[] savedXmmRegisters = new[] { "xmm0", "xmm1" };
        _setFlyHoopSpeedYHook = hooks.CreateAsmHook(new string[]
        {
            "use32",

            // Note: Original Code to set Air Gained for Frame Omitted.
            // We are replicating it in the hook.
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.PushXmmRegisters(savedXmmRegisters)}", // Save SSE Registers

            $"fnsave [{fpuBackupAllocation}]", // Store FPU State
            $"push ebx",                // Push Player Ptr
            $"push dword [esi + 0x44]", // Float Value
            $"{utilities.AssembleAbsoluteCall<GenericModifyPlayerFloatFnPtr>(typeof(EventController), nameof(SetFlySpeedYHook), false)}",
            $"{utilities.PopFromX87ToRegister()}",
            $"frstor [{fpuBackupAllocation}]",          // Restore FPU State
            $"{utilities.MultiplyFromRegisterToX87()}", // Original Code: Override multiply
            
            $"{utilities.PopXmmRegisters(savedXmmRegisters)}", // Restore SSE Registers
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            
            // Original Code
            "lea edi, [ebx+0x250]"
        }, 0x4C7FA0, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    [UnmanagedCallersOnly]
    private static void OnGrinding(Player* player)
    {
        ref var speedCap = ref player->SpeedCap;

        // Update time spent on rails
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        var timeOnRail  = _playerRailTimeSpentFrames[playerIndex] + 1;
        _playerRailTimeSpentFrames[playerIndex] = timeOnRail;

        SetRailSpeedCap?.Invoke(ref speedCap, player, timeOnRail);
        AfterSetRailSpeedCap?.Invoke(ref speedCap, player, timeOnRail);
    }

    [UnmanagedCallersOnly]
    private static float SetPlayerGrindInitialSpeedCap(float value, Player* player)
    {
        // Set frame count for grinding to 0.
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        _playerRailTimeSpentFrames[playerIndex] = 0;

        var copy = value;
        SetRailInitialSpeedCap?.Invoke(ref copy, player, 0);
        return value;
    }

    [UnmanagedCallersOnly]
    private static float SetFlySpeedXHook(float value, Player* player)
    {
        var copy = value;
        OnSetFlyRingSpeed?.Invoke(ref copy, player);
        SetFlyRingSpeedX?.Invoke(ref value, player);
        return value;
    }

    [UnmanagedCallersOnly]
    private static float SetFlySpeedYHook(float value, Player* player)
    {
        SetFlyRingSpeedY?.Invoke(ref value, player);
        return value;
    }

    /// <summary>
    /// Used to modify rail speed for a given player.
    /// </summary>
    public delegate void ModifyPlayerRailSpeedFn(ref float value, Player* player, int numFramesOnRail);
}