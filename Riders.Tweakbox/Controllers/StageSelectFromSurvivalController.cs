using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;
namespace Riders.Tweakbox.Controllers;

public class StageSelectFromSurvivalController : IController
{
    public IAsmHook Hook { get; }
    private TweakboxConfig _config;

    public unsafe StageSelectFromSurvivalController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utils, MenuReturnTargetService menuReturn)
    {
        _config = config;

        var returnToRaceSurvival = new string[]
        {
            "use32",
                
            // Get Menu to Return To
            $"mov ecx, dword [{(long)menuReturn.Current.Pointer}]",
            $"mov [0x006A21D8], ecx", // Set return menu

            // Jump if Below Minimum
            "cmp ecx, 82",
            "jge exit",

            // Normal Race
            "cmp ecx, 80",
            "jl exit",

            "mov  byte [edi+14h], 22",
            "push 1",
            "push 7530h",
            "push dword 0x00465070",
            $"{utils.GetAbsoluteCallMnemonics((IntPtr) 0x00527E00, false)}",
            "mov [esi], eax",
            "add esp, 0Ch",

            // Race
            $"mov ecx, dword [{(long)menuReturn.Current.Pointer}]",
            "cmp ecx, 80",
            "jne battle",

            "mov eax, dword [0x017BE87C]",
            "mov dword [0x005F8758], eax",
            "mov byte [esi+3Ch], 4",
            "jmp return",

            // Battle
            "battle:",
            "mov eax, dword [0x017BE880]",
            "mov dword [0x005F8758], eax",
            "mov byte [esi+3Ch], 5",
            
            "return:",
            $"mov ecx, 0",
            $"mov dword [{(long)menuReturn.Current.Pointer}], ecx",
            "mov byte [0x00692B88], 1",

            "pop ebp",
            "pop edi",
            "pop esi",
            "pop ebx",
            "add esp, 8",
            "ret",

            "exit:"
        };

        Hook = hooks.CreateAsmHook(returnToRaceSurvival, 0x0046B80B).Activate();
        _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
    }

    private void PropertyUpdated(string propertyname)
    {
        if (propertyname == nameof(_config.Data.SurvivalReturnToTrackSelect))
            Hook.Toggle(_config.Data.SurvivalReturnToTrackSelect);
    }
}
