using System;
using System.Diagnostics;
using System.Linq;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Microsoft.Windows.Sdk;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class FixesController : IController
    {
        // Internal
        private EventController _event = IoC.GetConstant<EventController>();

        // Settings
        private TweaksEditorConfig _config = IoC.Get<TweaksEditorConfig>();

        // Hooks
        private IAsmHook _bootToMenu;
        private IHook<Functions.CdeclReturnIntFn> _readConfigHook;
        private IHook<Functions.CdeclReturnIntFn> _loadWorldAssetsHook;
        private IAsmHook _initializeObjectLayoutHook;

        public FixesController()
        {
            var utilities = SDK.ReloadedHooks.Utilities;

            // Now for our hooks.
            _readConfigHook = Functions.ReadConfigFile.Hook(ReadConfigFile).Activate();
            _loadWorldAssetsHook = Functions.LoadWorldAssets.Hook(LoadWorldAssetsHook).Activate();

            var checkIfLoadSinglePlayerLayout = new[] { $"use32\n{utilities.AssembleAbsoluteCall(CheckIfLoadSinglePlayerObjectLayout, out _, new []{ "mov eax, 1" }, null, null, "je")}" };
            _initializeObjectLayoutHook = SDK.ReloadedHooks.CreateAsmHook(checkIfLoadSinglePlayerLayout, 0x004196E0, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

            _config.ConfigUpdated += OnConfigUpdated;
            _event.OnCheckIfQtePressLeft += EventOnOnCheckIfQtePressLeft;
            _event.OnCheckIfQtePressRight += EventOnOnCheckIfQtePressRight;
        }

        // Interface
        public void Disable()
        {
            _bootToMenu?.Disable();
            _readConfigHook?.Disable();
            _loadWorldAssetsHook?.Disable();
            _initializeObjectLayoutHook?.Disable();
        }

        public void Enable()
        {
            _bootToMenu?.Enable();
            _readConfigHook?.Enable();
            _loadWorldAssetsHook?.Enable();
            _initializeObjectLayoutHook?.Enable();
        }

        // Hook Implementation
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressRight() => _config.Data.AutoQTE;
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressLeft() => _config.Data.AutoQTE;
        private int ReadConfigFile()
        {
            var originalResult = _readConfigHook.OriginalFunction();
            _config.Apply();
            return originalResult;
        }

        private void UnlockAllAndDisableBootToMenu()
        {
            // Unlock All
            for (var x = 0; x < State.UnlockedStages.Count; x++)
                State.UnlockedStages[x] = true;

            for (var x = 0; x < State.UnlockedCharacters.Count; x++)
                State.UnlockedCharacters[x] = true;

            var defaultModels = Enums.GetMembers<ExtremeGearModel>();
            for (var x = 0; x < State.UnlockedGearModels.Count; x++)
                if (defaultModels.Any(z => (int)z.Value == x))
                    State.UnlockedGearModels[x] = true;

            *State.IsBabylonCupUnlocked = true;
            _bootToMenu.Disable();
        }

        private void OnConfigUpdated()
        {
            if (_bootToMenu == null && _config.Data.BootToMenu)
            {
                var utils = SDK.ReloadedHooks.Utilities;
                var bootToMain = new string[]
                {
                    "use32",
                    $"{utils.AssembleAbsoluteCall(UnlockAllAndDisableBootToMenu, out _)}",
                    $"{utils.GetAbsoluteJumpMnemonics((IntPtr) 0x0046AF9D, false)}",
                };

                _bootToMenu = SDK.ReloadedHooks.CreateAsmHook(bootToMain, 0x0046AEE9, AsmHookBehaviour.ExecuteFirst).Activate();
            }
        }

        private int LoadWorldAssetsHook()
        {
            var forceSinglePlayer = _config.Data.SinglePlayerStageData;
            int originalNumCameras = *State.NumberOfCameras;

            if (forceSinglePlayer)
                *State.NumberOfCameras = 1;

            var result = _loadWorldAssetsHook.OriginalFunction();

            *State.NumberOfCameras = originalNumCameras;
            return result;
        }

        private Enum<AsmFunctionResult> CheckIfLoadSinglePlayerObjectLayout() => _config.Data.SinglePlayerStageData;
    }
}
