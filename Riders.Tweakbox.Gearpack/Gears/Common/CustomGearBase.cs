using System.IO;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears;

namespace Riders.Tweakbox.Gearpack.Gears.Common;

public abstract class CustomGearBase : IExtremeGear
{
    public abstract string FolderName { get; set; }

    public AddGearRequest Request { get; set; }

    /// <summary>
    /// Initializes this extreme gear.
    /// </summary>
    /// <param name="gearsFolder">The folder containing the custom gear.</param>
    public void Initialize(string gearsFolder, Interfaces.ICustomGearApi gearApi)
    {
        InitializeGear(gearsFolder, gearApi);
        Request = gearApi.ImportFromFolder(Path.Combine(gearsFolder, FolderName));
        Request.Behaviour = this;
        gearApi.AddGear(Request);
    }

    public virtual void InitializeGear(string gearsFolder, Interfaces.ICustomGearApi gearApi) { }
}
