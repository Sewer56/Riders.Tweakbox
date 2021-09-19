using Riders.Tweakbox.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using System.Linq;

namespace Riders.Tweakbox.Services
{
    /// <summary>
    /// Service which automatically imports custom gears from other Reloaded mods.
    /// </summary>
    public class CustomGearImporterService : ISingletonService
    {
        private IModLoader _modLoader;
        private Logger _log = new Logger(LogCategory.Default);
        private Dictionary<string, List<AddGearRequest>> _requests = new Dictionary<string, List<AddGearRequest>>();
        private CustomGearService _customGearService = IoC.GetSingleton<CustomGearService>();
        private CustomGearController _customGearController; 

        public CustomGearImporterService(IModLoader modLoader)
        {
            _modLoader = modLoader;
            _modLoader.ModLoading += OnModLoading;
            _modLoader.ModUnloading += OnModUnloading;
        }

        private void Add(IModConfigV1 config)
        {
            EnsureControllerAvailable();
            var folder = GetGearFolder(config.ModId);
            if (!Directory.Exists(folder))
                return;

            var requests = new List<AddGearRequest>();
            var directories = Directory.GetDirectories(folder);
            foreach (var subDirectory in directories)
            {
                try
                {
                    var name = Path.GetFileName(subDirectory);
                    _log.WriteLine($"Loading Custom Gear: {name}");

                    var request = _customGearService.ImportFromFolder(subDirectory);
                    _customGearController.AddGear(request);
                    requests.Add(request);
                }
                catch (Exception e)
                {
                    _log.WriteLine($"Failed to Import Custom Gear\n{e.Message}\n{e.StackTrace}");
                }
            }

            _requests[config.ModId] = requests;
        }

        private void Remove(IModConfigV1 config)
        {
            EnsureControllerAvailable();
            if (_requests.Remove(config.ModId, out var list))
                _customGearController.RemoveGears(list.Select(x => x.GearName));
        }

        private void OnModUnloading(IModV1 arg1, IModConfigV1 arg2) => Remove(arg2);
        private void OnModLoading(IModV1 arg1, IModConfigV1 arg2) => Add(arg2);
        private string GetGearFolder(string modId) => _modLoader.GetDirectoryForModId(modId) + @"/Tweakbox/Gears";

        private void EnsureControllerAvailable()
        {
            _customGearController ??= IoC.GetSingleton<CustomGearController>();
        }
    }
}
