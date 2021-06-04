using System;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using static Riders.Tweakbox.Misc.Native;
using Task = System.Threading.Tasks.Task;

// ReSharper disable once RedundantUsingDirective
using Microsoft.Windows.Sdk;
using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Configs
{
    public class TweakboxConfig : JsonConfigBase<TweakboxConfig, TweakboxConfig.Internal>
    {
        #region Internal
        public class Internal : NotifyPropertyChangedBase
        {
            public bool BootToMenu = true;
            public bool FramePacing = true;
            public bool FramePacingSpeedup = true; // Speed up game to compensate for lag.
            public float DisableYieldThreshold = 80;
            public bool D3DDeviceFlags = true;
            public bool DisableVSync = true;
            public bool AutoQTE = true;
            public int ResolutionX = 1280;
            public int ResolutionY = 720;
            public bool Fullscreen = false;
            public bool Blur = false;
            public bool WidescreenHack = false;
            public bool Borderless = false;
            public bool SinglePlayerStageData = true;
            public bool SinglePlayerModels = true;

            public bool NormalRaceReturnToTrackSelect = true;
            public bool TagReturnToTrackSelect = true;
            public bool SurvivalReturnToTrackSelect = true;
            public float MaxSpeedupTimeMillis = 2000f;

            public bool IncludeVanillaMusic = true;
            public bool AllowBackwardsDriving = true;

            public bool BootToRace = false;
            public Levels BootToRaceLevel = Levels.MetalCity;
            public Characters BootToRaceCharacter = Characters.Sonic;
            public ExtremeGear BootToRaceGear = ExtremeGear.Default;
        }
        #endregion
    }
}
