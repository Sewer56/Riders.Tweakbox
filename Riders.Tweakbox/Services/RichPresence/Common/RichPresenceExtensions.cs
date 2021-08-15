using System;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
namespace Riders.Tweakbox.Services.RichPresence.Common;

public static class RichPresenceExtensions
{
    public static string AsString(this ActiveRaceMode mode)
    {
        return mode switch
        {
            ActiveRaceMode.NormalRace => "Free Race",
            ActiveRaceMode.TimeTrial => "Time Trial",
            ActiveRaceMode.GrandPrix => "Grand Prix",
            ActiveRaceMode.Story => "Story Mode",
            ActiveRaceMode.RaceStage => "Race Stage",
            ActiveRaceMode.BattleStage => "Battle Stage",
            ActiveRaceMode.Mission => "Mission Mode",
            ActiveRaceMode.TagMode => "Tag Mode",
            _ => String.Empty
        };
    }

    public static string AsString(this RaceMode mode)
    {
        return mode switch
        {
            RaceMode.FreeRace => "Free Race",
            RaceMode.TimeTrial => "Time Trial",
            RaceMode.GrandPrix => "Grand Prix",
            RaceMode.StoryMode => "Story Mode",
            RaceMode.RaceStage => "Race Stage",
            RaceMode.BattleStage => "Battle Stage",
            RaceMode.MissionMode => "Mission Mode",
            RaceMode.TagMode => "Tag Mode",
            RaceMode.Demo => "Demo",
            _ => String.Empty
        };
    }

    public static string AsString(this TitleSequenceTaskState state)
    {
        return state switch
        {
            TitleSequenceTaskState.LoadRace => "Racing",
            TitleSequenceTaskState.Race => "Racing",
            TitleSequenceTaskState.TitleScreen => "Title Screen",
            TitleSequenceTaskState.MainMenu => "Main Menu",
            TitleSequenceTaskState.NormalRaceSubmenu => "Selecting a Race Mode",
            TitleSequenceTaskState.StorySubmenu => "Selecting a Story",
            TitleSequenceTaskState.MissionSubmenu => "Selecting a Mission",
            TitleSequenceTaskState.LoadTagSubmenu => "Setting up Tag Race",
            TitleSequenceTaskState.SurvivalSubmenu => "Setting up Survival Mode",
            TitleSequenceTaskState.CourseSelect => "Stage Select",
            TitleSequenceTaskState.CharacterSelect => "Character Select",
            TitleSequenceTaskState.Shop => "Shopping",
            TitleSequenceTaskState.ExtrasSubmenus => "Extras",
            TitleSequenceTaskState.OptionsSubmenus => "Options",
            TitleSequenceTaskState.MissionSelect => "Selecting a Mission",
            TitleSequenceTaskState.SetSplashScreen => "Splash Screen",
            TitleSequenceTaskState.CheckHeapAndSetSplashScreen => "Setting Up",
            TitleSequenceTaskState.SplashScreen => "Splash Screen",
            TitleSequenceTaskState.LoadShopSubmenu => "Shopping",
            TitleSequenceTaskState.TimeTrialSaving => "Saving: Time Trial",
            TitleSequenceTaskState.LoadExtras => "Extras",
            TitleSequenceTaskState.LoadOptions => "Options",
            TitleSequenceTaskState.LoadTitleScreen => "Title Screen",
            TitleSequenceTaskState.LoadSubmenuAfterReturnToTitleSequence => "",
            TitleSequenceTaskState.LoadMainMenu => "Main Menu",
            _ => "Unknown State"
        };
    }
}
