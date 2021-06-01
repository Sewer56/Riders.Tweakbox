using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;

namespace Riders.Tweakbox.Controllers
{
    public class StageSelectFromRaceController : IController
    {
        public IAsmHook Hook { get; }
        private TweakboxConfig _config;
        
        public unsafe StageSelectFromRaceController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utils, MenuReturnTargetService menuReturn)
        {
            _config = config;

            var returnToRaceNormal = new string[]
            {
                "use32",
                
                // Get Menu to Return To
                $"mov ecx, dword [{(long)menuReturn.Current.Pointer}]",
                $"mov [0x006A21D8], ecx", // Set return menu

                // Jump if Below Minimum
                "cmp ecx, 40",
                "jl exit",

                // Normal Race
                "cmp ecx, 43",
                "jge exit",

                    // Shared Between Free Race, Time Trial, World Grand Prix
                    "mov  byte [edi+14h], 22",
                    "push 1",
                    "push 7530h",
                    "push dword 0x00465070",
                    $"{utils.GetAbsoluteCallMnemonics((IntPtr) 0x00527E00, false)}",
                    "mov dword [esi], eax", 
                    "add esp, 0Ch",
                
                    // Free Race
                    $"mov ecx, dword [{(long)menuReturn.Current.Pointer}]", // restore trashed register.
                    "cmp ecx, 40",
                    "jne timetrial",

                    "mov eax, dword [0x017BE86C]",
                    "mov dword [0x005F8758], eax",
                    "mov byte [esi+3Ch], 0",
                    "mov byte [esi+0x0E], 0",

                    "jmp return",

                    // TimeTrial
                    "timetrial:",
                    "cmp ecx, 41",
                    "jne grandprix",

                    "mov eax, dword [0x017BE870]",
                    "mov dword [0x005F8758], eax",
                    "mov byte [esi+3Ch], 1",
                    "mov dword [0x17DF3C4], 0",
                    "mov byte [esi+0x0E], 1",

                    "jmp return",

                    // WGP
                    "grandprix:",
                    "mov eax, dword [0x017BE874]",
                    "mov dword [0x005F8758], eax",
                    "mov byte [esi+3Ch], 2",

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

            Hook = hooks.CreateAsmHook(returnToRaceNormal, 0x0046B1D2).Activate();
            _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
        }

        private void PropertyUpdated(string propertyname)
        {
            if (propertyname == nameof(_config.Data.NormalRaceReturnToTrackSelect))
                Hook.Toggle(_config.Data.NormalRaceReturnToTrackSelect);
        }
    }
}
