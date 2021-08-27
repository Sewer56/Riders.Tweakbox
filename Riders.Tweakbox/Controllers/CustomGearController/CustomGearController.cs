using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Services.TextureGen;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using System.Collections.Generic;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;

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

    internal Dictionary<string, CustomGearData> AvailableGears = new Dictionary<string, CustomGearData>();
    internal List<CustomGearData> LoadedGears = new List<CustomGearData>();

    public CustomGearController()
    {
        CodePatcher = new CustomGearCodePatcher();
        UiController = new CustomGearUiController(CodePatcher);
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="request">The gear information.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    public AddGearResult AddGear(AddGearRequest request)
    {
        // If already loaded, ignore.
        if (IsGearLoaded(request.GearName))
            return null;

        var data = Mapping.Mapper.Map<CustomGearData>(request);
        _log.WriteLine($"[{nameof(CustomGearController)}] Adding Gear: {request.GearName}");
        return AddGear_Internal(data);
    }

    /// <summary>
    /// Removes a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool RemoveGear(string name)
    {
        if (!AvailableGears.ContainsKey(name))
            return false;

        _log.WriteLine($"[{nameof(CustomGearController)}] Removing Gear: {name}");
        AvailableGears.Remove(name, out var value);
        LoadedGears.Remove(value);
        Reload();
        return true;
    }

    /// <summary>
    /// Returns true if the gear is loaded, else false.
    /// </summary>
    /// <param name="name">The name of the gear.</param>
    public bool IsGearLoaded(string name) => LoadedGears.FindIndex(x => x.GearName == name) != -1;

    /// <summary>
    /// Checks if the user has all gears from a given list of names.
    /// </summary>
    /// <param name="gearNames">List of names of each gear.</param>
    public bool HasAllGears(List<string> gearNames, out List<string> missingGears)
    {
        missingGears = new List<string>(gearNames.Count);

        foreach (var gearName in gearNames)
        {
            if (!AvailableGears.ContainsKey(gearName))
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
            if (AvailableGears.TryGetValue(gearName, out var gear))
                AddGear_Internal(gear);
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
        foreach (var gear in AvailableGears.Values)
            AddGear_Internal(gear);
    }

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    /// <param name="clearGears">Removes all known gears if set to true.</param>
    public void Reset(bool clearGears = true)
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Resetting Gears"); 
        CodePatcher.Reset();
        UiController.Reset();
        if (clearGears)
        {
            ClearGearIndices();
            AvailableGears.Clear();
        }

        LoadedGears.Clear();
        ClearGearIndices();
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear data.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    private AddGearResult AddGear_Internal(CustomGearData data)
    {
        var result = new AddGearResult();

        // Always add to loaded gears.
        // and/or add/replace dictionary items.
        AvailableGears[data.GearName] = data;
        LoadedGears.Add(data);
        
        // Invoke sub-handlers
        CodePatcher.AddGear(data, result);
        UiController.AddGear(data, result);
        return result;
    }

    private void ClearGearIndices()
    {
        foreach (var gear in AvailableGears.Values)
            gear.SetGearIndex(-1);
    }
}
