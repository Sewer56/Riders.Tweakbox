﻿using System;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using static Riders.Tweakbox.Misc.Native;
using Task = System.Threading.Tasks.Task;

// ReSharper disable once RedundantUsingDirective
using Windows.Win32;
using Riders.Tweakbox.Configs.Json;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.Imgui.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Sewer56.SonicRiders.Structures.Misc;

namespace Riders.Tweakbox.Configs;

public class TweakboxConfig : JsonConfigBase<TweakboxConfig, TweakboxConfig.Internal>
{
    public class Internal : NotifyPropertyChangedBase
    {
        public bool BootToMenu = true;
        public bool FramePacing = true;
        public bool FramePacingSpeedup = true; // Speed up game to compensate for lag.
        public float DisableYieldThreshold = 80;
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
        public bool HardwareVertexProcessing = true;
        public bool DisablePsgpThreading = true;
        public bool NoParticles = false;

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
        public MemoryLimit MemoryLimit = MemoryLimit.MB64;

        public bool RemoveFpsCap = false;
        public VoiceLanguage VoiceLanguage = VoiceLanguage.English;
        public MessageLanguage Language = MessageLanguage.English;

        public GameInput InputMode = GameInput.WhenNoWindowActive;
        public VK InputToggleKey = VK.F10;

        public Definitions.Nullable<ColorRGBA> DriftChargeColour = new Definitions.Nullable<ColorRGBA>(new ColorRGBA()
        {
            Red = 248,
            Green = 243,
            Blue = 10,
            Alpha = 255
        }, false);

        public ColorRGBA IgnoreTurbulenceColour = new ColorRGBA()
        {
            Red = 176,
            Green = 10,
            Blue = 248,
            Alpha = 255
        };

        public GameModifiers Modifiers = new GameModifiers();
    }

    public enum MemoryLimit : int
    {
        MB64 = 0x3D09000,
        MB128 = 0x7A12000,
        MB256 = 0xF424000,
        MB512 = 0x1E848000
    }

    public enum GameInput
    {
        Always,
        Toggle,
        WhenNoWindowActive
    }
}
