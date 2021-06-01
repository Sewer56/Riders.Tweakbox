using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    public class StageSelectFromTagController : IController
    {
        public IAsmHook Hook { get; }
        private TweakboxConfig _config;

        public unsafe StageSelectFromTagController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utils, MenuReturnTargetService menuReturn)
        {
            _config = config;
            
            string[] returnToRaceTag = new[]
            {
                "use32",
                $"mov edx, dword [{(long)menuReturn.Current.Pointer}]",
                "cmp edx, 70",
                "jne exit",

                // Reset Return Menu Value
                $"mov edx, 0",
                $"mov dword [{(long)menuReturn.Current.Pointer}], edx",

                // Close Menu, Taken from 0x0046B161
                "movzx   eax, byte [esi+39h]",
                "push    esi",
                "call    dword [eax*4+0x005BC21C]" ,
                "add     esp, 4",

                $"{utils.GetAbsoluteJumpMnemonics((IntPtr) 0x0046B7C3, false)}",
                "exit:"
            };

            Hook = SDK.ReloadedHooks.CreateAsmHook(returnToRaceTag, 0x0046B08B).Activate();
            _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
        }

        private void PropertyUpdated(string propertyname)
        {
            if (propertyname == nameof(_config.Data.TagReturnToTrackSelect))
                Hook.Toggle(_config.Data.TagReturnToTrackSelect);
        }
    }
}
