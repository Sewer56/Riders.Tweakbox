using System;
using System.Linq;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
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
        public IAsmHook ReturnToCourseSelectSurvival { get; }
        public IAsmHook ReturnToCourseSelectNormalRace { get; }
        public IAsmHook ReturnToCourseSelectTag { get; }

        private IHook<Functions.CdeclReturnIntFn> _readConfigHook;
        private IHook<Functions.CdeclReturnIntFn> _loadWorldAssetsHook;
        private IAsmHook _initializeObjectLayoutHook;

        // Hooks Persistent Data
        private IAsmHook _getReturnMenuHook;
        private Pinnable<int> _menuReturn = new Pinnable<int>(22);

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

            // Return to Course Select Hook
            var utils = SDK.ReloadedHooks.Utilities;

            // Setup Return to Course Select
            var getReturnMenu = new string[]
            {
                "use32",
                $"mov dword [{(long)_menuReturn.Pointer}], eax",  // Set Menu to Return To
            };

            _getReturnMenuHook = SDK.ReloadedHooks.CreateAsmHook(getReturnMenu, 0x0046AC73, AsmHookBehaviour.ExecuteFirst).Activate();

            var returnToRaceNormal = new string[]
            {
                "use32",
                
                // Get Menu to Return To
                $"mov ecx, dword [{(long)_menuReturn.Pointer}]",
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
                    $"{utilities.GetAbsoluteCallMnemonics((IntPtr) 0x00527E00, false)}",
                    "mov dword [esi], eax", 
                    "add esp, 0Ch",
                
                    // Free Race
                    $"mov ecx, dword [{(long)_menuReturn.Pointer}]", // restore trashed register.
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
                $"mov dword [{(long)_menuReturn.Pointer}], ecx",
                "mov byte [0x00692B88], 1",
                "pop ebp",
                "pop edi",
                "pop esi",
                "pop ebx",
                "add esp, 8",
                "ret",

                "exit:"
            };

            ReturnToCourseSelectNormalRace = SDK.ReloadedHooks.CreateAsmHook(returnToRaceNormal, 0x0046B1D2, AsmHookBehaviour.ExecuteFirst).Activate();

            string[] returnToRaceTag = new[]
            {
                "use32",
                $"mov edx, dword [{(long)_menuReturn.Pointer}]",
                "cmp edx, 70",
                "jne exit",

                // Reset Return Menu Value
                $"mov edx, 0",
                $"mov dword [{(long)_menuReturn.Pointer}], edx",

                // Close Menu, Taken from 0x0046B161
                "movzx   eax, byte [esi+39h]",
                "push    esi",
                "call    dword [eax*4+0x005BC21C]" ,
                "add     esp, 4",

                $"{utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x0046B7C3, false)}",
                "exit:"
            };

            ReturnToCourseSelectTag = SDK.ReloadedHooks.CreateAsmHook(returnToRaceTag, 0x0046B08B, AsmHookBehaviour.ExecuteFirst).Activate();

            var returnToRaceSurvival = new string[]
            {
                "use32",
                
                // Get Menu to Return To
                $"mov ecx, dword [{(long)_menuReturn.Pointer}]",
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
                    $"{utilities.GetAbsoluteCallMnemonics((IntPtr) 0x00527E00, false)}",
                    "mov [esi], eax", 
                    "add esp, 0Ch",

                    // Race
                    $"mov ecx, dword [{(long)_menuReturn.Pointer}]",
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
                $"mov dword [{(long)_menuReturn.Pointer}], ecx",
                "mov byte [0x00692B88], 1",

                "pop ebp",
                "pop edi",
                "pop esi",
                "pop ebx",
                "add esp, 8",
                "ret",

                "exit:"
            };

            ReturnToCourseSelectSurvival = SDK.ReloadedHooks.CreateAsmHook(returnToRaceSurvival, 0x0046B80B, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        // Interface
        public void Disable()
        {
            _bootToMenu?.Disable();
            _readConfigHook?.Disable();
            _loadWorldAssetsHook?.Disable();
            _initializeObjectLayoutHook?.Disable();
            ReturnToCourseSelectNormalRace?.Disable();
            _getReturnMenuHook?.Disable();
            ReturnToCourseSelectSurvival?.Disable();
            ReturnToCourseSelectTag?.Disable();
        }

        public void Enable()
        {
            _bootToMenu?.Enable();
            _readConfigHook?.Enable();
            _loadWorldAssetsHook?.Enable();
            _initializeObjectLayoutHook?.Enable();
            ReturnToCourseSelectNormalRace?.Enable();
            _getReturnMenuHook?.Enable();
            ReturnToCourseSelectSurvival?.Enable();
            ReturnToCourseSelectTag?.Enable();
        }

        // Hook Implementation
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressRight() => _config.Data.AutoQTE;
        private Enum<AsmFunctionResult> EventOnOnCheckIfQtePressLeft() => _config.Data.AutoQTE;
        private int ReadConfigFile()
        {
            var originalResult = _readConfigHook.OriginalFunction();
            _config.ApplyStartup();
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
            for (int x = 0; x < Player.MaxNumberOfPlayers; x++)
            {
                // We omit setting player pointers and mission mode doesn't set it,
                // so we need to do it in here in case player goes straight for mission mode.
                Player.Players[x].PlayerInput = (Player.Inputs.Pointer) + x; 
            }

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

                _bootToMenu = SDK.ReloadedHooks.CreateAsmHook(bootToMain, 0x46AD01, AsmHookBehaviour.ExecuteFirst).Activate();
            }

            ReturnToCourseSelectTag.Toggle(_config.Data.TagReturnToTrackSelect);
            ReturnToCourseSelectNormalRace.Toggle(_config.Data.NormalRaceReturnToTrackSelect);
            ReturnToCourseSelectSurvival.Toggle(_config.Data.NormalRaceReturnToTrackSelect);
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
