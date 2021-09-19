using System;
using Sewer56.SonicRiders.Structures.Gameplay;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Services.TextureGen;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Texture;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using ExtremeGear = Sewer56.SonicRiders.Structures.Enums.ExtremeGear;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using Sewer56.SonicRiders;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// This controller allows for extra gear slots in the game.
/// </summary>
public unsafe class CustomGearController : IController
{
    // Private members
    private Logger _log = new Logger(LogCategory.CustomGear);

    /// <summary>
    /// Executed when the gear list is being reloaded.
    /// </summary>
    public event Action AfterReset;

    /// <summary>
    /// Executed when the gear count changes.
    /// </summary>
    public event Action AfterGearCountChanged;

    internal CustomGearCodePatcher CodePatcher;
    internal CustomGearUiController UiController;
    internal CustomGearPatches Patches;

    internal Dictionary<string, CustomGearDataInternal> AvailableGears = new Dictionary<string, CustomGearDataInternal>();

    internal List<CustomGearDataInternal> LoadedGears = new List<CustomGearDataInternal>();

    public CustomGearController(IReloadedHooks hooks)
    {
        // DO NOT Reorder
        CodePatcher = new CustomGearCodePatcher();
        Patches = new CustomGearPatches(CodePatcher);
        UiController = new CustomGearUiController(CodePatcher);
    }

    /// <summary>
    /// Returns true if vanilla gears are enabled, else false.
    /// </summary>
    public bool VanillaGearsEnabled() => CodePatcher.OriginalGearCount == Sewer56.SonicRiders.API.Player.OriginalNumberOfGears;

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="request">The gear information.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    public CustomGearDataInternal AddGear(AddGearRequest request)
    {
        // If already loaded, ignore.
        if (string.IsNullOrEmpty(request.GearName) || IsGearLoaded(request.GearName))
            return null;

        // If hit gear limit, ignore.
        if (!CodePatcher.HasAvailableSlots)
            return null;

        // Get gear data from file (if needed).
        if (!request.LoadData())
            return null;

        var data = Mapping.Mapper.Map<CustomGearDataInternal>(request);
        _log.WriteLine($"[{nameof(CustomGearController)}] Adding Gear: {request.GearName}");
        AddGear_Internal(data);
        return data;
    }

    /// <summary>
    /// Retrieves the data of a given gear.
    /// </summary>
    /// <param name="index">The index of the gear.</param>
    /// <param name="data">Receives the custom gear.</param>
    /// <returns>True if the gear data was found, else false.</returns>
    public bool TryGetGearData(int index, out CustomGearDataInternal data)
    {
        if (!TryGetGearData_Internal(index, out data))
            return false;
        
        return true;
    }

    /// <summary>
    /// Retrieves the data of a given gear.
    /// </summary>
    /// <param name="name">The name of the gear.</param>
    /// <param name="data">
    ///     The object to receive the custom gear details.
    ///     Must not be null.
    /// </param>
    /// <returns>True if the gear data was found, else false.</returns>
    public bool TryGetGearData(string name, out CustomGearDataInternal data)
    {
        if (AvailableGears.TryGetValue(name, out data))
            return true;

        return false;
    }

    /// <summary>
    /// Retrieves the names of all custom gears.
    /// </summary>
    /// <param name="names">The span to receive the names of the custom gears.</param>
    public void GetCustomGearNames(Span<string> loadedNames, Span<string> unloadedNames)
    {
        for (var x = 0; x < LoadedGears.Count && x < loadedNames.Length; x++)
            loadedNames[x] = LoadedGears[x].GearName;

        // Populated Loaded Names
        var values = AvailableGears.Values;
        int unloadedIndex = 0;
        foreach (var value in values)
        {
            if (unloadedIndex >= unloadedNames.Length)
                break;

            if (!value.IsGearLoaded)
                unloadedNames[unloadedIndex++] = value.GearName;
        }
    }

    /// <summary>
    /// Retrieves the names of all custom gears.
    /// </summary>
    /// <param name="loadedGears">List of the names of currently loaded gears.</param>
    /// <param name="unloadedGears">List of the names of currently unloaded gears.</param>
    public void GetCustomGearNames(out string[] loadedGears, out string[] unloadedGears)
    {
        GetCustomGearCount(out int loadedGearCount, out int unloadedGearCount);
        loadedGears   = new string[loadedGearCount];
        unloadedGears = new string[unloadedGearCount];
        GetCustomGearNames(loadedGears, unloadedGears);
    }

    /// <summary>
    /// Retrieves the names of all loaded custom gears.
    /// </summary>
    public void GetCustomGearCount(out int loadedGears, out int unloadedGears)
    {
        loadedGears   = LoadedGears.Count;
        unloadedGears = AvailableGears.Count - LoadedGears.Count;
    }

    /// <summary>
    /// Retrieves the name of a given gear.
    /// </summary>
    /// <param name="index">The index of the gear.</param>
    /// <param name="isCustomGear">True if the gear is a custom gear, else false.</param>
    public string GetGearName(int index, out bool isCustomGear)
    {
        isCustomGear = false;
        if (index < CodePatcher.OriginalGearCount)
            return ((ExtremeGear)index).GetName();
        else
        {
            isCustomGear = true;
            if (TryGetGearData_Internal(index, out var data))
                return data.GearName;
            
            return "Unknown";
        }
    }

    /// <summary>
    /// Unloads a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool UnloadGear(string name) => RemoveGear(name, false);

    /// <summary>
    /// Removes a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <param name="clearGear">Removes the gear from the available/unloaded set of gears.</param>
    /// <returns>True on success, else false.</returns>
    public bool RemoveGear(string name, bool clearGear = true)
    {
        if (!AvailableGears.ContainsKey(name))
            return false;

        _log.WriteLine($"[{nameof(CustomGearController)}] Removing Gear: {name}");
        LoadedGears.Remove(AvailableGears[name]);
        if (clearGear)
            AvailableGears.Remove(name, out var value);

        Reload(LoadedGears.Select(x => x.GearName).ToArray());
        return true;
    }

    /// <summary>
    /// Removes a custom gear with a specific name.
    /// </summary>
    /// <param name="names">Names of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <param name="clearGear">Removes the gears from the available/unloaded set of gears.</param>
    /// <returns>True on success, else false.</returns>
    public void RemoveGears(IEnumerable<string> names, bool clearGear = true)
    {
        foreach (var name in names)
        {
            _log.WriteLine($"[{nameof(CustomGearController)}] Removing Gear: {name}");
            LoadedGears.Remove(AvailableGears[name]);
            if (clearGear)
                AvailableGears.Remove(name, out var value);
        }

        Reload(LoadedGears.Select(x => x.GearName).ToArray());
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
    public bool HasAllGears(IEnumerable<string> gearNames, out List<string> missingGears)
    {
        // If there's no gears, we have them all!
        if (gearNames == null)
        {
            missingGears = default;
            return true;
        }

        // Else actually do the check.
        missingGears = new List<string>();
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
    public void Reload(IEnumerable<string> gearNames)
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Reloading Gears");
        Reset(false);

        if (gearNames == null)
            return;

        foreach (var gearName in gearNames)
        {
            if (AvailableGears.TryGetValue(gearName, out var gear))
                AddGear_Internal(gear);
        }
    }

    /// <summary>
    /// Reloads all available gear data.
    /// </summary>
    public void ReloadAll() => Reload(AvailableGears.Select(x => x.Value.GearName));

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    /// <param name="clearGears">Removes all known gears if set to true.</param>
    /// <param name="removeVanillaGears">Removes all gears from the vanilla game.</param>
    public void Reset(bool clearGears = true, bool removeVanillaGears = false)
    {
        _log.WriteLine($"[{nameof(CustomGearController)}] Resetting Gears"); 
        CodePatcher.Reset(removeVanillaGears);
        UiController.Reset();
        ClearGearIndices();
        if (clearGears)
            AvailableGears.Clear();

        LoadedGears.Clear();
        AfterReset?.Invoke();
    }

    internal bool TryGetGearData_Internal(int index, out CustomGearDataInternal data)
    {
        var indexOffset = index - CodePatcher.OriginalGearCount;
        data = default;

        if (indexOffset < 0 || indexOffset >= LoadedGears.Count)
            return false;

        data = LoadedGears[indexOffset];
        return true;
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear data.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    private void AddGear_Internal(CustomGearDataInternal data)
    {
        // Invoke sub-handlers
        CodePatcher.AddGear(data);
        UiController.AddGear(data);

        // Always add to loaded gears.
        // and/or add/replace dictionary items.
        AvailableGears[data.GearName] = data;
        LoadedGears.Add(data);

        AfterGearCountChanged?.Invoke();
    }

    private void ClearGearIndices()
    {
        foreach (var gear in AvailableGears.Values)
            gear.SetGearIndex(-1);
    }
}
