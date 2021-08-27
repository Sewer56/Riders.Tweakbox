using Sewer56.SonicRiders.Structures.Gameplay;
using System;
using Reloaded.Memory;
using Sewer56.SonicRiders.Parser.Archive.Structs;
using File = System.IO.File;

namespace Riders.Tweakbox.Controllers.CustomGearController.Structs;

/// <summary>
/// Parameters used in the AddGear methods of <see cref="CustomGearController"/>.
/// </summary>
public class AddGearRequest
{
    /// <summary>
    /// Name of the gear, typically derived from folder name.
    /// MUST BE UNIQUE.
    /// </summary>
    public string GearName;

    /// <summary>
    /// The gear data to add to the game.
    /// </summary>
    public ExtremeGear? GearData;

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
    /// This is called when the index of the gear changes, for example 
    /// after joining a netplay session. Use this if you have custom code
    /// tied to a gear.
    /// 
    /// If the value passed is -1; the gear is unloaded. This usually happens
    /// at the beginning of a reset before the gear is re-assigned.
    /// </summary>
    public Action<int> OnIndexChanged;
}

/// <summary>
/// Note: This is separated from <see cref="AddGearRequest"/> because in the future Custom Gears will be an
/// API accessible from other mods.
/// </summary>
public static class AddGearRequestExtensions
{
    /// <summary>
    /// Populates the <see cref="AddGearRequest.GearData"/> from file if it is set to null.
    /// </summary>
    /// <returns>False if the operation fails or gear data cannot be loaded.</returns>
    public static bool LoadData(this AddGearRequest request)
    {
        if (request.GearData != null) 
            return true;

        if (string.IsNullOrEmpty(request.GearDataLocation))
            return false;

        try
        {
            var bytes = File.ReadAllBytes(request.GearDataLocation);
            Struct.FromArray(bytes, out request.GearData);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}