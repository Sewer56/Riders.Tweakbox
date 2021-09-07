using System;
using System.Linq;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
namespace Riders.Tweakbox.Controllers;

public class BootToMenuController : IController
{
    private TweakboxConfig _config;
    private IAsmHook _bootToMenu;
    private EventController _event;

    public unsafe BootToMenuController(TweakboxConfig config, IReloadedHooks hooks, IReloadedHooksUtilities utils)
    {
        _config = config;
        _event = IoC.GetSingleton<EventController>();

        var bootToMain = new string[]
        {
            "use32",
            "mov dword [esi+8], 40",
            $"{utils.AssembleAbsoluteCall(UnlockAllAndDisableBootToMenu)}",
            $"{utils.GetAbsoluteJumpMnemonics((IntPtr) 0x0046AF9D, false)}",
        };

        _bootToMenu = hooks.CreateAsmHook(bootToMain, 0x46AD01).Activate();
        _config.ConfigUpdated += OnConfigUpdated;
        _config.Data.AddPropertyUpdatedHandler(OnPropertyUpdated);
    }

    private void OnPropertyUpdated(string propertyname)
    {
        if (propertyname == nameof(_config.Data.BootToMenu))
            _bootToMenu.Toggle(_config.Data.BootToMenu);
    }

    private void OnConfigUpdated() => _bootToMenu.Toggle(_config.Data.BootToMenu);

    private unsafe void UnlockAllAndDisableBootToMenu()
    {
        // Unlock All
        for (var x = 0; x < State.UnlockedStages.Count; x++)
            State.UnlockedStages[x] = true;

        for (var x = 0; x < State.UnlockedCharacters.Count; x++)
            State.UnlockedCharacters[x] = true;

        var defaultModels = Enums.GetMembers<ExtremeGearModel>();
        for (var x = 0; x < State.UnlockedGearModels.Count; x++)
            State.UnlockedGearModels[x] = true;

        *State.IsBabylonCupUnlocked = true;
        for (int x = 0; x < Player.MaxNumberOfPlayers; x++)
        {
            // We omit setting player pointers and mission mode doesn't set it,
            // so we need to do it in here in case player goes straight for mission mode.
            Player.Players[x].PlayerInput = (Player.Inputs.Pointer) + x;
        }

        // Return to free race submenu if chaining this with another modifier (e.g. boot to race).
        *State.MenuToReturnToFromRace = 40;

        if (_config.Data.BootToRace)
            _event.AfterTitleSequence += AfterTitleBootToRace;

        _bootToMenu.Disable();
    }

    private unsafe void AfterTitleBootToRace(Task<TitleSequence, TitleSequenceTaskState>* task)
    {
        var data = _config.Data;
        *State.Level = data.BootToRaceLevel;
        *State.RaceMode = ActiveRaceMode.NormalRace;
        *State.NumberOfCameras = 1;
        *State.NumberOfRacers = 1;
        Player.Players[0].Character = data.BootToRaceCharacter;
        Player.Players[0].ExtremeGear = data.BootToRaceGear;

        task->TaskStatus = TitleSequenceTaskState.LoadRace;
        _event.AfterTitleSequence -= AfterTitleBootToRace;
    }
}
