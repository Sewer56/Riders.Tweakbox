using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    public class SinglePlayerObjectLayoutController : IController
    {
        /// <summary>
        /// If true, informs the game the player pressed left in the Quicktime event.
        /// </summary>
        public event AsmFunc CheckIfForceSinglePlayerObjectLayout;

        private TweakboxConfig _config;
        private IAsmHook _initializeObjectLayoutHook;

        public SinglePlayerObjectLayoutController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utilities)
        {
            _config = config;

            var checkIfLoadSinglePlayerLayout = new[]
            {
                $"use32\n" +
                $"{utilities.AssembleAbsoluteCall(() => CheckIfForceSinglePlayerObjectLayout.InvokeIfNotNull(), out _, new []{ "mov eax, 1" }, null, null, "je")}"
            };

            _initializeObjectLayoutHook = SDK.ReloadedHooks.CreateAsmHook(checkIfLoadSinglePlayerLayout, 0x004196E0, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
            CheckIfForceSinglePlayerObjectLayout += CheckIfLoadSinglePlayerObjectLayout;
        }

        private Enum<AsmFunctionResult> CheckIfLoadSinglePlayerObjectLayout() => _config.Data.SinglePlayerStageData;
    }
}
