using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Enums;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using Riders.Tweakbox.Services.Placeholder;

namespace Riders.Tweakbox.Services
{
    /// <summary>
    /// Tools for operating with custom gears.
    /// </summary>
    public unsafe class CustomGearService : ISingletonService
    {
        private const string _iconFileName  = "icon.png";
        private const string _titleFileName = "title.png";
        private const string _dataFileName = "data.bin";

        private IO _io = IoC.GetSingleton<IO>();
        private Logger _log = new Logger(LogCategory.Default);
        private PlaceholderTextureService _placeholderService = IoC.GetSingleton<PlaceholderTextureService>();

        /// <summary>
        /// Exports a full gear (incl. Icons) to a new folder.
        /// </summary>
        /// <param name="gear">The gear data to write to the folder.</param>
        /// <param name="data">Information about the added gear.</param>
        /// <returns>The output folder.</returns>
        public void ExportToFolder(ExtremeGear* gear, string gearName)
        {
            ExportToFolder(gear, new CustomGearData()
            {
                GearName = gearName
            });
        }

        /// <summary>
        /// Exports a full gear (incl. Icons) to a new folder.
        /// </summary>
        /// <param name="gear">The gear data to write to the folder.</param>
        /// <param name="data">Information about the added gear.</param>
        /// <returns>The output folder.</returns>
        public void ExportToFolder(ExtremeGear* gear, CustomGearData data)
        {
            var exportFolder = Path.Combine(_io.ExportFolder, data.GearName);
            UpdateTexturePaths(data);
            Directory.CreateDirectory(exportFolder);

            File.Copy(data.IconPath, Path.Combine(exportFolder, _iconFileName), true);
            File.Copy(data.NamePath, Path.Combine(exportFolder, _titleFileName), true);
            UpdateRawGearData(gear, Path.Combine(exportFolder, _dataFileName));
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{exportFolder}\"") { CreateNoWindow = true });
            _log.WriteLine($"Created Custom Gear in {exportFolder}");
        }

        /// <summary>
        /// Exports raw gear data to a specified path.
        /// </summary>
        /// <param name="gear">The raw data to save.</param>
        /// <param name="data">The custom gear data.</param>
        public void UpdateRawGearData(ExtremeGear* gear, CustomGearData data)
        {
            if (UpdateRawGearData(gear, data.GearDataLocation))
                data.GearData = *gear;
        }

        /// <summary>
        /// Exports raw gear data to a specified path.
        /// </summary>
        /// <param name="gear">The raw data to save.</param>
        /// <param name="filePath">The file path to save the raw data to.</param>
        /// <returns>True on success, else false.</returns>
        public bool UpdateRawGearData(ExtremeGear* gear, string filePath)
        {
            try
            {
                var bytes = Struct.GetBytes(Unsafe.AsRef<ExtremeGear>(gear));
                File.WriteAllBytes(filePath, bytes);
                return true;
            }
            catch (Exception e)
            {
                _log.WriteLine($"Failed to Update Gear Data: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a placeholder title for a specific gear.
        /// </summary>
        /// <param name="gearIndex">The index of the gear.</param>
        /// <returns>Path to a placeholder title texture.</returns>
        public string GetPlaceholderTitle(int gearIndex) => _placeholderService.TitleIconPlaceholders[gearIndex % _placeholderService.TitleIconPlaceholders.Length];

        /// <summary>
        /// Retrieves a placeholder icon for a specific gear.
        /// </summary>
        /// <param name="gear">The gear to get the placeholder icon for.</param>
        /// <param name="gearIndex">The index of the gear.</param>
        /// <returns>Path to a placeholder icon texture.</returns>
        public string GetPlaceholderIcon(ref ExtremeGear gear, int gearIndex) => GetPlaceholderIcon(gear.GearType, gearIndex);

        /// <summary>
        /// Retrieves a placeholder icon for a specific gear.
        /// </summary>
        /// <param name="type">The type of gear used.</param>
        /// <param name="gearIndex">The index of the gear.</param>
        /// <returns>Path to a placeholder icon texture.</returns>
        public string GetPlaceholderIcon(GearType type, int gearIndex)
        {
            return type switch
            {
                GearType.Bike => ModuloSelectFromArray(_placeholderService.BikeIconPlaceholders, gearIndex),
                GearType.Skate => ModuloSelectFromArray(_placeholderService.SkateIconPlaceholders, gearIndex),
                GearType.Board => ModuloSelectFromArray(_placeholderService.BoardIconPlaceholders, gearIndex),
                _ => ModuloSelectFromArray(_placeholderService.BoardIconPlaceholders, gearIndex)
            };
        }

        internal void UpdateTexturePaths(CustomGearData data)
        {
            data.IconPath = !string.IsNullOrEmpty(data.IconPath) ? data.IconPath : GetPlaceholderIcon(ref data.GearData, data.GearIndex);
            data.NamePath = !string.IsNullOrEmpty(data.NamePath) ? data.NamePath : GetPlaceholderTitle(data.GearIndex);
        }

        private T ModuloSelectFromArray<T>(T[] items, int index) => items[index % items.Length];
    }
}
