using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    public class SinglePlayerCharacterModelController : IController
    {
        public IAsmHook Hook;
        private TweaksConfig _config;

        public SinglePlayerCharacterModelController(TweaksConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utils)
        {
            _config = config;

            var loadSinglePlayerCharModel = new string[]
            {
                "use32",

                // Check if story mode.
                "push eax",
                "mov eax, [0x00692B88]",
                "cmp eax, 100",
                "pop eax",
                $"je story",

                // Not story mode, we can load SP models. Insert null terminator.
                $"mov [esp+0x29], bl",
                $"jmp complete",
                $"story:",

                // Original code for story mode.
                $"mov [esp+0x28], byte 0x4D",
                $"mov [esp+0x29], bl",
                $"complete:"
            };

            Hook = SDK.ReloadedHooks.CreateAsmHook(loadSinglePlayerCharModel, 0x00408E87, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
            _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
        }

        private void PropertyUpdated(string propertyname)
        {
            if (propertyname == nameof(_config.Data.SinglePlayerModels))
                Hook.Toggle(_config.Data.SinglePlayerModels);
        }
    }
}
