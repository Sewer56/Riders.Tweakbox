using Sewer56.SonicRiders.Structures.Gameplay;
using System;

namespace Riders.Tweakbox.Controllers.CustomGearController.Structs;

/// <summary>
/// Parameters used in the AddGear methods of <see cref="CustomGearController"/>.
/// </summary>
public class AddGearData
{
    /// <summary>
    /// Name of the gear, typically derived from folder name.
    /// MUST BE UNIQUE. DO NOT MODIFY AFTER ADDING.
    /// </summary>
    public string GearName;

    /// <summary>
    /// The gear data to add to the game.
    /// </summary>
    public ExtremeGear GearData;

    /// <summary>
    /// [Optional] Path to the icon texture used by the gear.
    /// </summary>
    public string IconPath;

    /// <summary>
    /// [Optional] Path to the name texture used by the gear.
    /// </summary>
    public string NamePath;

    /// <summary>
    /// This is called when the index of the gear changes, for example 
    /// after joining a netplay session. Use this if you have custom code
    /// tied to a gear.
    /// 
    /// If the value passed is -1; the gear is unloaded. This usually happens
    /// at the beginning of a reset before the gear is re-assigned.
    /// </summary>
    public Action<int> OnIndexChanged;
}
