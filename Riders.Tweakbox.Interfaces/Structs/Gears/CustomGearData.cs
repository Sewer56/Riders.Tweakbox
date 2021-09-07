using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Gears;

public class CustomGearData
{
    /// <summary>
    /// Name of the gear, typically derived from folder name.
    /// MUST BE UNIQUE.
    /// </summary>
    public string GearName;

    /// <summary>
    /// Represents custom gear behaviour.
    /// </summary>
    public IExtremeGear Behaviour;

    /// <summary>
    /// The gear data to add to the game.
    /// </summary>
    public byte[] GearData;

    /// <summary>
    /// [Optional] File path from which to load the extreme gear data.
    /// Data will be read from this path if <see cref="GearData"/> is null.
    /// </summary>
    public string GearDataLocation;

    /// <summary>
    /// [Optional] Path to the icon texture used by the gear.
    /// </summary>
    public string IconPath;

    /// <summary>
    /// [Optional] Path to the name texture used by the gear.
    /// </summary>
    public string NamePath;

    /// <summary>
    /// [Optional] Path to the animated texture folder for the icon.
    /// </summary>
    public string AnimatedIconFolder;

    /// <summary>
    /// [Optional] Path to the animated texture folder for the name.
    /// </summary>
    public string AnimatedNameFolder;

    /// <summary>
    /// Current index assigned to the gear.
    /// </summary>
    public int GearIndex;

    /// <summary>
    /// True if the gear is loaded, else false.
    /// </summary>
    public bool IsGearLoaded => GearIndex != -1;
}
