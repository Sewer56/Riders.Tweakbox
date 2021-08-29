using System;
using Sewer56.SonicRiders.Structures.Gameplay;
namespace Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;

public class CustomGearData
{
    /// <summary>
    /// Name of the gear, typically derived from folder name.
    /// MUST BE UNIQUE.
    /// </summary>
    public string GearName;

    /// <summary>
    /// The gear data to add to the game.
    /// </summary>
    public ExtremeGear GearData;

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
    public int GearIndex { get; private set; }

    /// <summary>
    /// True if the gear is loaded, else false.
    /// </summary>
    public bool IsGearLoaded => GearIndex != -1;

    /// <summary>
    /// This is called when the index of the gear changes, for example 
    /// after joining a netplay session. Use this if you have custom code
    /// tied to a gear.
    /// 
    /// If the value passed is -1; the gear is unloaded. This usually happens
    /// at the beginning of a reset before the gear is re-assigned.
    /// </summary>
    public Action<int> OnIndexChanged;

    /// <summary>
    /// Sets a new gear index and notifies any subscribers of the changed.
    /// </summary>
    internal void SetGearIndex(int index)
    {
        if (index != GearIndex)
        {
            GearIndex = index;
            OnIndexChanged?.Invoke(index);
        }
    }

    /// <summary>
    /// Resets the state of the object.
    /// </summary>
    public void Reset()
    {
        GearName = default;
        GearData = default;
        GearDataLocation = default;
        IconPath = default;
        NamePath = default;
        AnimatedIconFolder = default;
        AnimatedNameFolder = default;
        GearIndex = default;
        OnIndexChanged = default;
    }
}