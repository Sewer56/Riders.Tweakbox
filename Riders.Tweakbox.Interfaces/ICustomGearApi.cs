using Riders.Tweakbox.Interfaces.Structs.Gears;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Riders.Tweakbox.Interfaces;

public interface ICustomGearApi
{
    /// <summary>
    /// Imports custom gear data from a folder.
    /// </summary>
    AddGearRequest ImportFromFolder(string folderPath);

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="request">The gear information.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    CustomGearData AddGear(AddGearRequest request);

    /// <summary>
    /// Retrieves the data of a given gear.
    /// </summary>
    /// <param name="name">The index of the gear.</param>
    /// <param name="data">The custom gear details.</param>
    /// <returns>True if the gear data was found, else false.</returns>
    bool TryGetGearData(int index, out CustomGearData data);

    /// <summary>
    /// Retrieves the data of a given gear.
    /// </summary>
    /// <param name="name">The name of the gear.</param>
    /// <param name="data">The custom gear details.</param>
    /// <returns>True if the gear data was found, else false.</returns>
    bool TryGetGearData(string name, out CustomGearData data);

    /// <summary>
    /// Retrieves the names of all custom gears.
    /// </summary>
    /// <param name="names">The span to receive the names of the custom gears.</param>
    void GetCustomGearNames(Span<string> loadedNames, Span<string> unloadedNames);

    /// <summary>
    /// Retrieves the names of all custom gears.
    /// </summary>
    /// <param name="loadedGears">List of the names of currently loaded gears.</param>
    /// <param name="unloadedGears">List of the names of currently unloaded gears.</param>
    void GetCustomGearNames(out string[] loadedGears, out string[] unloadedGears);

    /// <summary>
    /// Retrieves the names of all loaded custom gears.
    /// </summary>
    void GetCustomGearCount(out int loadedGears, out int unloadedGears);

    /// <summary>
    /// Retrieves the name of a given gear.
    /// </summary>
    /// <param name="index">The index of the gear.</param>
    /// <param name="isCustomGear">True if the gear is a custom gear, else false.</param>
    string GetGearName(int index, out bool isCustomGear);

    /// <summary>
    /// Unloads a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <returns>True on success, else false.</returns>
    bool UnloadGear(string name);

    /// <summary>
    /// Removes a custom gear with a specific name.
    /// </summary>
    /// <param name="name">Name of the gear used in <see cref="AddGearRequest.GearName"/> when the gear was added.</param>
    /// <param name="clearGear">Removes the gear from the available/unloaded set of gears.</param>
    /// <returns>True on success, else false.</returns>
    bool RemoveGear(string name, bool clearGear = true);

    /// <summary>
    /// Returns true if the gear is loaded, else false.
    /// </summary>
    /// <param name="name">The name of the gear.</param>
    bool IsGearLoaded(string name);

    /// <summary>
    /// Checks if the user has all gears from a given list of names.
    /// </summary>
    /// <param name="gearNames">List of names of each gear.</param>
    bool HasAllGears(IEnumerable<string> gearNames, out List<string> missingGears);

    /// <summary>
    /// Reloads the gears and adds only the gear names in the given list.
    /// </summary>
    void Reload(IEnumerable<string> gearNames);

    /// <summary>
    /// Reloads all available gear data.
    /// </summary>
    void ReloadAll();

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    /// <param name="clearGears">Removes all known gears if set to true.</param>
    void Reset(bool clearGears = true);

    /// <summary>
    /// Removes all vanilla gears from the game.
    /// This action cannot be reversed.
    /// </summary>
    void RemoveVanillaGears();
}
