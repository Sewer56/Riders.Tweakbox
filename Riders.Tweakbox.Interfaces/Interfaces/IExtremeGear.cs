using Riders.Tweakbox.Interfaces.Structs.Gears.Behaviour;
using System.Collections.Generic;

namespace Riders.Tweakbox.Interfaces.Interfaces;

/// <summary>
/// Provides an implementation/template for a custom extreme gear.
/// </summary>
public interface IExtremeGear
{
    /// <summary>
    /// Gets level information for each of the gear levels.
    /// If this list is not null, you can have any amount of custom levels per gear.
    /// </summary>
    public List<ExtremeGearLevelStats> GetExtendedLevelStats() => default;

    /// <summary>
    /// Defines properties of the overclock mode for this gear.
    /// </summary>
    public OverclockModeDX GetOverclockMode() => default;

    /// <summary>
    /// If true, uses Riders DX style berserker mode where the air depletes if the gauge
    /// is above a certain point.
    /// </summary>
    public BerserkerPropertiesDX GetBerserkerProperties() => default;

    /* IMPLEMENTING */

    /// <summary>
    /// Returns custom properties for the boost behaviour of this gear.
    /// </summary>
    public BoostProperties GetBoostProperties() => default;

    /// <summary>
    /// Returns custom properties for the cruising behaviour of this gear.
    /// </summary>
    public CruisingProperties GetCruisingProperties() => default;

    /* IMPLEMENTED */

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
}
