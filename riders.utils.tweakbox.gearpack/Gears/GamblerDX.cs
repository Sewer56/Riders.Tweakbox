using System;
using System.Drawing;
using Riders.Tweakbox.Gearpack.Gears.Common;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.IO;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Player = Sewer56.SonicRiders.API.Player;
using Colourful;
using Riders.Tweakbox.Gearpack.Utilities;

namespace Riders.Tweakbox.Gearpack.Gears;

public unsafe class GamblerDX : CustomGearBase, IExtremeGear
{
    private const int ExhaustCycleTime = 120;
    private ExtendedLevelStats _extendedLevelStats;
    private OverclockModeDX _overclockProperties = new OverclockModeDX()
    {
        TopSpeed = Utility.SpeedometerToFloat(228),
        BoostSpeed = Utility.SpeedometerToFloat(290),
        BoostCost = 25000,
        DriftCap = Utility.SpeedometerToFloat(280),
        DriftDashFrames = 45,
        DriftDashSpeed = Utility.SpeedometerToFloat(150),
        DriftCost = 10, // Unsure
        TornadoCost = 10000,
        AccelerationMultiplier = 1.2f,
        RingActivation = 90
    };

    private ExhaustProperties _exhaustProperties;
    private IColorConverter<RGBColor, LChabColor> _rgbToLchConverter = new ConverterBuilder().FromRGB().ToLChab().Build();
    private IColorConverter<LChabColor, RGBColor> _lchToRgbConverter = new ConverterBuilder().FromLChab().ToRGB().Build();

    /// <summary>
    /// Defines a true/false value for every player stating whether they are in overclock mode or not.
    /// </summary>
    private bool[] _isOverclocked = new bool[Player.MaxNumberOfPlayers];

    /// <summary>
    /// Initializes this custom gear.
    /// </summary>
    public override void Initialize(string gearsFolder, ICustomGearApi gearApi)
    {
        _extendedLevelStats = new ExtendedLevelStats()
        {
            SetPlayerStats = SetGearStats
        };

        _exhaustProperties = new ExhaustProperties()
        {
            GetExhaustTrailColour = GetExhaustTrailColour
        };

        var data = gearApi.ImportFromFolder(Path.Combine(gearsFolder, "Gambler DX"));
        data.Behaviour = this;
        gearApi.AddGear(data);
    }


    /// <summary>
    /// Executed every frame. Checks if should enter overclock mode.
    /// </summary>
    private void CheckIfShouldEnterOverclockMode(IntPtr playerPtr, int playerIndex, int playerLevel)
    {
        var player = (Sewer56.SonicRiders.Structures.Gameplay.Player*) playerPtr;
        if (!_isOverclocked[playerIndex] && player->Rings >= _overclockProperties.RingActivation)
        {
            _isOverclocked[playerIndex] = true;
            player->GearSpecialFlags |= ExtremeGearSpecialFlags.RingGearGainRingsOnAttack;
            player->GearSpecialFlags |= ExtremeGearSpecialFlags.GearOnRings;
            player->Level = 2;
            player->Air = (int)(player->LevelThreeStats.GearStats.MaxAir * (player->Rings / 100f));
        }
    }

    /// <summary>
    /// Executed every frame. Sets exhaust trail colour.
    /// </summary>
    private Color GetExhaustTrailColour(IntPtr playerPtr, int playerIndex, int playerLevel, Color value)
    {
        var player = (Sewer56.SonicRiders.Structures.Gameplay.Player*)playerPtr;
        if (_isOverclocked[playerIndex])
        {
            var lch = _rgbToLchConverter.Convert(new RGBColor(value));
            var time = (*State.TotalFrameCounter % ExhaustCycleTime) / (float) ExhaustCycleTime;
            lch = ColorInterpolator.GetRainbowColor((float)lch.C, (float)lch.L, time);
            return _lchToRgbConverter.Convert(lch).ToColor();
        }

        return value;
    }

    /// <summary>
    /// Executed every frame. Sets the gear stats.
    /// </summary>
    private void SetGearStats(IntPtr levelStatsPtr, IntPtr playerPtr, int playerIndex, int playerLevel)
    {
        var stats = (PlayerLevelStats*) levelStatsPtr;
        var player = (Sewer56.SonicRiders.Structures.Gameplay.Player*)playerPtr;

        if (_isOverclocked[playerIndex])
        {
            stats->AccelToSpeedCap1 *= _overclockProperties.AccelerationMultiplier;
            stats->AccelToSpeedCap2 *= _overclockProperties.AccelerationMultiplier;
            stats->AccelToSpeedCap3 *= _overclockProperties.AccelerationMultiplier;
            stats->SpeedCap3 = _overclockProperties.TopSpeed;
            stats->GearStats.BoostSpeed = _overclockProperties.BoostSpeed;
            stats->GearStats.BoostCost = _overclockProperties.BoostCost;
            stats->GearStats.TornadoCost = _overclockProperties.TornadoCost;
            stats->GearStats.DriftAirCost = _overclockProperties.DriftCost;
            stats->GearStats.SpeedGainedFromDriftDash = _overclockProperties.DriftDashSpeed;
        }
    }

    /// <summary>
    /// Executed when gear stats reset.
    /// </summary>
    private void OnResetImpl(IntPtr playerptr, int playerindex, int playerLevel) => _isOverclocked = new bool[Player.MaxNumberOfPlayers]; // Nobody is overclocked :/

    // IExtremeGear API Callbacks //
    public ExhaustProperties GetExhaustProperties() => _exhaustProperties;

    public ExtendedLevelStats GetExtendedLevelStats() => _extendedLevelStats;

    public ApiEventHandler OnFrame() => CheckIfShouldEnterOverclockMode;

    public ApiEventHandler OnReset() => OnResetImpl;
}

public struct OverclockModeDX
{
    /// <summary>
    /// Amount of rings needed to activate this state if <see cref="ActivationMode"/> is Ring.
    /// </summary>
    public int RingActivation;

    /// <summary>
    /// The top speed of the gear. Using speedometer value.
    /// </summary>
    public float TopSpeed;

    /// <summary>
    /// The gear's boost speed. Using speedometer value.
    /// </summary>
    public float BoostSpeed;

    /// <summary>
    /// The boost cost of the gear; in air.
    /// </summary>
    public int BoostCost;

    /// <summary>
    /// Amount of frames needed to generate a drift dash.
    /// </summary>
    public int DriftDashFrames;

    /// <summary>
    /// Speed added by performing a drift dash.
    /// </summary>
    public float DriftDashSpeed;

    /// <summary>
    /// Maximum speed achieved by performing a drift.
    /// </summary>
    public float DriftCap;

    /// <summary>
    /// Sets the drift cost for the gear.
    /// </summary>
    public int DriftCost;

    /// <summary>
    /// The amount it costs to perform a tornado.
    /// </summary>
    public int TornadoCost;

    /// <summary>
    /// Multiplier by which to increase accel
    /// </summary>
    public float AccelerationMultiplier;
}