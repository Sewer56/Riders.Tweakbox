using System;
using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;

namespace Riders.Tweakbox.Interfaces.Interfaces;

public interface ICustomStats
{
    /// <summary>
    /// Modifies the behaviour of running for the gear.
    /// </summary>
    public RunningProperties GetRunningProperties() => default;

    /// <summary>
    /// Returns custom properties for changing behaviour on interacting with items.
    /// </summary>
    public ItemProperties GetItemProperties() => default;

    /// <summary>
    /// Returns custom properties for modifying tornado behaviour.
    /// </summary>
    public TornadoProperties GetTornadoProperties() => default;

    /// <summary>
    /// Allows you to modify the exhaust trail colour.
    /// </summary>
    public ExhaustProperties GetExhaustProperties() => default;

    /// <summary>
    /// If true, uses Riders DX style berserker mode where the air depletes if the gauge
    /// is above a certain point.
    /// </summary>
    public BerserkerPropertiesDX GetBerserkerProperties() => default;

    /// <summary>
    /// Retrieves the dash panel properties.
    /// </summary>
    public DashPanelGearProperties GetDashPanelProperties() => default;

    /// <summary>
    /// Returns custom properties for the boost behaviour of this gear.
    /// </summary>
    public BoostProperties GetBoostProperties() => default;

    /// <summary>
    /// Changes properties of how tricks are handled for this gear.
    /// </summary>
    public TrickBehaviour GetTrickBehaviour() => default;

    /// <summary>
    /// Gets level information for each of the gear levels. Starting from level 1.
    /// </summary>
    public ExtendedLevelStats GetExtendedLevelStats() => default;

    /// <summary>
    /// Returns custom properties for the cruising behaviour of this gear.
    /// </summary>
    public CruisingProperties GetCruisingProperties() => default;

    /// <summary>
    /// Gets properties which change how the gear handles handling.
    /// </summary>
    public HandlingProperties GetHandlingProperties() => default;

    /// <summary>
    /// Gets individual properties which affect how air gain works.
    /// </summary>
    public AirProperties GetAirProperties() => default;

    /// <summary>
    /// Changes how the gear handles in being off-road.
    /// </summary>
    public OffRoadProperties GetOffroadProperties() => default;

    /// <summary>
    /// Defines how shortcuts behave differently on this gear.
    /// </summary>
    public ShortcutBehaviour GetShortcutBehaviour() => default;

    /// <summary>
    /// Modifies the behaviour of shortcuts if this gear grants a new shortcut type or an existing type.
    /// </summary>
    public MonoTypeShortcutBehaviourDX GetMonoTypeShortcutBehaviour() => default;

    /// <summary>
    /// Defines properties which override how the "legend" effect works.
    /// </summary>
    public LegendProperties GetLegendProperties() => default;

    /// <summary>
    /// Defines modifiers for the drift dash.
    /// </summary>
    public DriftDashProperties GetDriftDashProperties() => default;

    /// <summary>
    /// Modifies the behaviour of hitting walls.
    /// </summary>
    public WallHitBehaviour GetWallHitBehaviour() => default;

    /// <summary>
    /// Executed at the end of every frame.
    /// </summary>
    public ApiEventHandler OnFrame() => default;

    /// <summary>
    /// Executed when the state of the gear is reset.
    /// e.g. Map restart, gear stat recalculation.
    /// </summary>
    public ApiEventHandler OnReset() => default;

    /// <summary>
    /// Returns the player's current extended level.
    /// </summary>
    /// <param name="rings">The amount of rings in the player's posession.</param>
    /// <returns>Null if the custom levels feature is unused, else the custom level.</returns>
    public byte? TryGetPlayerLevel(int rings)
    {
        var stats = GetExtendedLevelStats();
        return stats != null ? stats.TryGetPlayerLevel(rings) : null;
    }
}

public static class ICustomStatsExtensions
{
    /// <summary/>
    /// <param name="value">The value to be modified.</param>
    public static void InvokeIfNotNull<T>(this SetValueHandler<T> handler, ref T value, IntPtr playerPtr, int playerIndex, int playerLevel)
    {
        if (handler != null)
            value = handler(playerPtr, playerIndex, playerLevel, value);
    }

    /// <summary/>
    /// <param name="value">The value to be modified.</param>
    public static T? QueryIfNotNull<T>(this QueryValueHandler<T> handler, IntPtr playerPtr, int playerIndex, int playerLevel) where T : unmanaged
    {
        if (handler != null)
            return handler(playerPtr, playerIndex, playerLevel);

        return null;
    }
}


/// <summary>
/// Allows you to modify a value.
/// </summary>
/// <param name="playerPtr">Pointer to the player struct.</param>
/// <param name="playerIndex">Index of the player in the player struct.</param>
/// <param name="playerLevel">The current player level (normally 0-2). Note: Includes custom levels; so value can be above 2.</param>
/// <param name="value">
///     The value being passed in.
///     This may for example be "speed", "acceleration" or "other".
/// </param>
public delegate T SetValueHandler<T>(IntPtr playerPtr, int playerIndex, int playerLevel, T value);

/// <summary>
/// Allows you to query an external source about its opinion on something.
/// </summary>
/// <param name="playerPtr">Pointer to the player struct.</param>
/// <param name="playerIndex">Index of the player in the player struct.</param>
/// <param name="playerLevel">The current player level (normally 0-2). Note: Includes custom levels; so value can be above 2.</param>
public delegate T QueryValueHandler<T>(IntPtr playerPtr, int playerIndex, int playerLevel);

/// <summary>
/// Allows you to add additional code to an event.
/// </summary>
/// <param name="playerPtr">Pointer to the player struct.</param>
/// <param name="playerIndex">Index of the player in the player struct.</param>
/// <param name="playerLevel">The current player level (normally 0-2). Note: Includes custom levels; so value can be above 2.</param>
public delegate void ApiEventHandler(IntPtr playerPtr, int playerIndex, int playerLevel);