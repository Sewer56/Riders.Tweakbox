using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Reloaded.Memory;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Enums;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using Riders.Tweakbox.Services.Placeholder;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;
using Riders.Tweakbox.Misc.Data;

namespace Riders.Tweakbox.Services
{
    /// <summary>
    /// Tools for operating with custom gears.
    /// </summary>
    public unsafe class CustomGearService : ISingletonService
    {
        private const string _iconFileName  = "icon";
        private const string _titleFileName = "title";
        private const string _dataFileName = "data.bin";
        private const string _instructionsFileName = "instructions.txt";
        private const string _instructions = "Create a new Reloaded mod and create the folders `Tweakbox` and inside it `Gears`. Copy the folder containing this file to that folder.\n\nPlease refer to the Tweakbox wiki for more guidance.";

        private IO _io = IoC.GetSingleton<IO>();
        private Logger _log = new Logger(LogCategory.Default);
        private PlaceholderTextureService _placeholderService = IoC.GetSingleton<PlaceholderTextureService>();

        /// <summary>
        /// Imports a custom gear from a given folder.
        /// </summary>
        /// <param name="folder">Full path to the folder containing the gear.</param>
        public AddGearRequest ImportFromFolder(string folder)
        {
            var result = new CustomGearDataInternal();
            
            result.GearName = Path.GetFileName(folder);
            result.GearDataLocation = Path.Combine(folder, _dataFileName);

            var gearData = File.ReadAllBytes(result.GearDataLocation).AsSpan();
            Struct.FromArray(gearData, out result.GearData);

            SetAnimatedTexturePath(ref result.AnimatedIconFolder, folder, _iconFileName);
            SetAnimatedTexturePath(ref result.AnimatedNameFolder, folder, _titleFileName);
            DirectorySearcher.TryGetDirectoryContents(folder, out var files, out var directories);
            result.IconPath = DirectoryExtensions.GetFileWithName(files, _iconFileName);
            result.NamePath = DirectoryExtensions.GetFileWithName(files, _titleFileName);
            UpdateTexturePaths(result, ref result.GearData);

            return Mapping.Mapper.Map<AddGearRequest>(result);
        }

        /// <summary>
        /// Exports a full gear (incl. Icons) to a new folder.
        /// </summary>
        /// <param name="gear">The gear data to write to the folder.</param>
        /// <param name="gearName">The name of the gear.</param>
        /// <returns>The output folder.</returns>
        public void ExportToFolder(ExtremeGear* gear, string gearName)
        {
            ExportToFolder(gear, new CustomGearDataInternal()
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
        public void ExportToFolder(ExtremeGear* gear, CustomGearDataInternal data)
        {
            var exportFolder = Path.Combine(_io.ExportFolder, data.GearName);
            UpdateTexturePaths(data, ref Unsafe.AsRef<ExtremeGear>(gear));
            Directory.CreateDirectory(exportFolder);
            CopyTexFileOrAnimatedDirectory(data.AnimatedIconFolder, data.IconPath, _iconFileName, exportFolder);
            CopyTexFileOrAnimatedDirectory(data.AnimatedNameFolder, data.NamePath, _titleFileName, exportFolder);
            UpdateRawGearData(gear, Path.Combine(exportFolder, _dataFileName));
            File.WriteAllText(Path.Combine(exportFolder, _instructionsFileName), _instructions);
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{exportFolder}\"") { CreateNoWindow = true });
            _log.WriteLine($"Created Custom Gear in {exportFolder}");
        }

        /// <summary>
        /// Exports raw gear data to a specified path.
        /// </summary>
        /// <param name="gear">The raw data to save.</param>
        /// <param name="data">The custom gear data.</param>
        public void UpdateRawGearData(ExtremeGear* gear, CustomGearDataInternal data)
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

        internal void UpdateTexturePaths(CustomGearDataInternal data, ref ExtremeGear gear)
        {
            data.IconPath = !string.IsNullOrEmpty(data.IconPath) && File.Exists(data.IconPath) ? data.IconPath : GetPlaceholderIcon(ref gear, data.GearIndex);
            data.NamePath = !string.IsNullOrEmpty(data.NamePath) && File.Exists(data.NamePath) ? data.NamePath : GetPlaceholderTitle(data.GearIndex);
        }

        private T ModuloSelectFromArray<T>(T[] items, int index) => items[index % items.Length];

        private static void CopyTexFileOrAnimatedDirectory(string animatedTexFolder, string texPath, string texFileName, string exportFolder)
        {
            if (Directory.Exists(animatedTexFolder) && !Native.PathIsDirectoryEmptyW(animatedTexFolder))
                DirectoryExtensions.DirectoryCopy(animatedTexFolder, GetAnimatedTexturePath(exportFolder, texFileName), true);
            else
                File.Copy(texPath, Path.Combine(exportFolder, texFileName), true);
        }

        private static string GetAnimatedTexturePath(string exportFolder, string texFileName) => Path.Combine(exportFolder, Path.GetFileNameWithoutExtension(texFileName));

        private void SetAnimatedTexturePath(ref string target, string gearFolder, string texFileName)
        {
            var fullPath = GetAnimatedTexturePath(gearFolder, texFileName);
            target = Directory.Exists(fullPath) && !Native.PathIsDirectoryEmptyW(fullPath) ? fullPath : null;
        }
    }
}
