using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Services.TextureGen;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using System.Collections.Generic;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// This controller allows for extra gear slots in the game.
/// </summary>
public unsafe class CustomGearController : IController
{
    // Private members
    private Logger _log = new Logger(LogCategory.CustomGear);

    internal CustomGearCodePatcher CodePatcher;
    internal CustomGearUiController UiController;
    internal Dictionary<string, AddGearData> AddedGears = new Dictionary<string, AddGearData>();

    public CustomGearController()
    {
        CodePatcher = new CustomGearCodePatcher();
        UiController = new CustomGearUiController(CodePatcher);
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear information.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    public AddGearDataResult AddGear(AddGearData data)
    {
        if (AddedGears.ContainsKey(data.GearName))
            return null;

        _log.WriteLine($"[{nameof(CustomGearController)}] Adding Gear: {data.GearName}");
        var result = AddGear_Internal(data);
        AddedGears[data.GearName] = data;
        return result;
    }

    /// <summary>
    /// Removes a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearData.GearName"/> when the gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool RemoveGear(string name)
    {
        if (!AddedGears.ContainsKey(name))
            return false;

        _log.WriteLine($"[{nameof(CustomGearController)}] Removing Gear: {name}");
        AddedGears.Remove(name);
        Reload();
        return true;
    }
    
    /// <summary>
    /// Checks if the user has all gears from a given list of names.
    /// </summary>
    /// <param name="gearNames">List of names of each gear.</param>
    public bool HasAllGears(List<string> gearNames, out List<string> missingGears)
    {
        missingGears = new List<string>(gearNames.Count);

        foreach (var gearName in gearNames)
        {
            if (!AddedGears.ContainsKey(gearName))
                missingGears.Add(gearName);
        }

        return missingGears.Count <= 0;
    }
    
    /// <summary>
    /// Reloads the gears and adds only the gear names in the given list.
    /// </summary>
    public void Reload(List<string> gearNames)
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Reloading Specific Set of Gears");
        Reset(false);

        foreach (var gearName in gearNames)
        {
            if (AddedGears.TryGetValue(gearName, out var gear))
            {
                var result = AddGear_Internal(gear);
                gear.OnIndexChanged?.Invoke(result.GearIndex);
            }
        }
    }

    /// <summary>
    /// Reloads all gear data.
    /// Used e.g. when entering Netplay.
    /// </summary>
    public void Reload()
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Reloading Gears");
        Reset(false);
        foreach (var gear in AddedGears.Values)
        {
            var result = AddGear_Internal(gear);
            gear.OnIndexChanged?.Invoke(result.GearIndex);
        }
    }

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    public void Reset(bool clearGears = true)
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Resetting Gears"); 
        CodePatcher.Reset();
        UiController.Reset();
        if (clearGears)
            AddedGears.Clear();

        foreach (var gear in AddedGears.Values)
            gear.OnIndexChanged?.Invoke(-1);
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear information.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    private AddGearDataResult AddGear_Internal(AddGearData data)
    {
        var result = new AddGearDataResult();
        CodePatcher.AddGear(data, result);
        UiController.AddGear(data, result.GearIndex, result);
        return result;
    }
}
