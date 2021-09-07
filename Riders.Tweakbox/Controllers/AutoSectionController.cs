using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
namespace Riders.Tweakbox.Controllers;

public class AutoSectionController : IController
{
    /// <summary>
    /// If true, informs the game the player pressed left in the Quicktime event.
    /// </summary>
    public event AsmFunc OnCheckIfQtePressLeft;

    /// <summary>
    /// If true, informs the game the player pressed left in the Quicktime event.
    /// </summary>
    public event AsmFunc OnCheckIfQtePressRight;

    private TweakboxConfig _config;
    private IAsmHook _onCheckIfQtePressLeftHook;
    private IAsmHook _onCheckIfQtePressRightHook;

    public AutoSectionController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utilities)
    {
        _config = config;

        var ifQtePressLeftAsm = new string[]
        {
            $"mov eax,[edx+0xB3C]",
            utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4B3721, Environment.Is64BitProcess)
        };

        var onCheckIfQtePressLeft = new[]
        {
            $"use32\n" +
            $"{utilities.AssembleAbsoluteCall(() => OnCheckIfQtePressLeft.InvokeIfNotNull(), ifQtePressLeftAsm, null, null)}"
        };

        _onCheckIfQtePressLeftHook = hooks.CreateAsmHook(onCheckIfQtePressLeft, 0x4B3716).Activate();

        var ifQtePressRightAsm = new[]
        {
            $"mov ecx,[edx+0xB3C]",
            utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4B3746, Environment.Is64BitProcess)
        };

        var onCheckIfQtePressRight = new[]
        {
            $"use32\n" +
            $"{utilities.AssembleAbsoluteCall(() => OnCheckIfQtePressRight.InvokeIfNotNull(), ifQtePressRightAsm, null, null)}"
        };

        _onCheckIfQtePressRightHook = hooks.CreateAsmHook(onCheckIfQtePressRight, 0x4B373B).Activate();

        OnCheckIfQtePressLeft += EventOnOnCheckIfQtePressLeft;
        OnCheckIfQtePressRight += EventOnOnCheckIfQtePressRight;
    }

    // Hook Implementation
    private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressRight() => _config.Data.AutoQTE;
    private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressLeft() => _config.Data.AutoQTE;
}
