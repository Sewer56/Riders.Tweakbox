using Riders.Tweakbox.Gearpack.Gears.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Riders.Tweakbox.Gearpack;

public class CustomGearPack
{
    /// <summary>
    /// Contains a list of all loaded gears with custom code.
    /// </summary>
    public List<CustomGearBase> Gears = new List<CustomGearBase>();

    public CustomGearPack(string modFolder, Interfaces.ITweakboxApi api)
    {
        var gearApi = api.GetCustomGearApi();
        var gearPath = Path.Combine(modFolder, "Tweakbox/Custom");

        // Get all implemented gears via reflection
        var types = Assembly.GetExecutingAssembly().GetTypes();

        // Initialize all configs.
        var configTypes = types.Where(x => typeof(CustomGearBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        foreach (var type in configTypes)
        {
            var gear = (CustomGearBase) Activator.CreateInstance(type);
            gear.Initialize(gearPath, gearApi);
            Gears.Add(gear);
        }
    }
}
