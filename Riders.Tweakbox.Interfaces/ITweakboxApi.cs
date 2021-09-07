namespace Riders.Tweakbox.Interfaces;

/// <summary>
/// Provides access to all other sub APIs available from Tweakbox.
/// </summary>
public interface ITweakboxApi
{
    /// <summary>
    /// Retrieves the API that allows you to interact with custom gears.
    /// </summary>
    public ICustomGearApi GetCustomGearApi();

    /// <summary>
    /// Retrieves the API that allows you to interact with physics.
    /// </summary>
    public IPhysicsApi GetPhysicsApi();
}
