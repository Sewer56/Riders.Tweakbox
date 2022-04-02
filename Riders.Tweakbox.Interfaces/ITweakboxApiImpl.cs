using Riders.Tweakbox.Interfaces.Structs;

namespace Riders.Tweakbox.Interfaces;

/// <summary>
/// Provides access to all other sub APIs available from Tweakbox.
/// </summary>
public interface ITweakboxApi
{
    /// <summary>
    /// Obtains access to the tweakbox API.
    /// </summary>
    /// <param name="modName">Concise unique name for the mod. Shows in server browser.</param>
    public ITweakboxApiImpl Register(string modName);

    /// <summary>
    /// Removes your mod from the list of loaded mods.
    /// </summary>
    /// <param name="modName">Concise unique name for the mod. Shows in server browser.</param>
    /// <remarks>Use if your mod supports unloading and you are unloading the mod.</remarks>
    public void Unregister(string modName);
}

/// <summary>
/// Provides access to all other sub APIs available from Tweakbox.
/// </summary>
public interface ITweakboxApiImpl
{
    /// <summary>
    /// Retrieves the API that allows you to interact with custom gears.
    /// </summary>
    public ICustomGearApi GetCustomGearApi();

    /// <summary>
    /// Retrieves the API that allows you to modify character behaviour.
    /// </summary>
    public ICustomCharacterApi GetCustomCharacterApi();

    /// <summary>
    /// Retrieves the API that allows you to interact with physics.
    /// </summary>
    public IPhysicsApi GetPhysicsApi();

    /// <summary>
    /// Retrieves new pointers for data such as gears.
    /// When writing your own mods, please copy these pointers to <see cref="Sewer56.SonicRiders.API.Players.Gears"/> and other relevant
    /// APIs.
    /// </summary>
    public ApiPointers GetPointers();
}
