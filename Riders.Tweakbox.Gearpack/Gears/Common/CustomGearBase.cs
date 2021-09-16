using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Gearpack.Gears.Common;

public abstract class CustomGearBase : IExtremeGear
{
    /// <summary>
    /// Initializes this extreme gear.
    /// </summary>
    /// <param name="gearsFolder">The folder containing the custom gear.</param>
    public abstract void Initialize(string gearsFolder, Interfaces.ICustomGearApi gearApi);
}
