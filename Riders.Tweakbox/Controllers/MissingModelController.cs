using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Functions;
using System;
using System.IO;
using Reloaded.Hooks.Definitions.Enums;
using static Sewer56.SonicRiders.Functions.Functions;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.X86;
using System.Diagnostics;
using Sewer56.SonicRiders.Structures.Enums;
using System.Text;
using Reloaded.Memory.Sources;
using Sewer56.SonicRiders.API;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// A controller which loads in alternatives for missing model files.
    /// </summary>
    public unsafe class MissingModelController : IController
    {
        private UtilDvdMallocReadFn _originalFunction;
        private string _dataFolderPath;
        private IAsmHook _loadPlayerModelRaceHook;
        private IAsmHook _loadPlayerModelMenuHook;
        private IAsmHook _replaceCharacterWithSuperHook;

        private Logger _generalLogger = new Logger(LogCategory.Default);
        private Logger _raceLogger = new Logger(LogCategory.Race);
        private Logger _menuLogger = new Logger(LogCategory.Menu);

        public MissingModelController(IReloadedHooks hooks)
        {
            // TODO: Improve implementation to take into account actual animation
            // type used for the model.
            var utilities = hooks.Utilities;
            var exeDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            _originalFunction = Functions.ArchiveOpenAndReadFile.GetWrapper();
            _dataFolderPath   = Path.GetFullPath(Path.Combine(exeDirectory, "Data"));

            string[] raceHookAsm = new []
            {
                "use32",
                // Replace original jmp with our function.
                $"{AsmHelpers.AssembleAbsoluteCall<UtilDvdMallocReadFn>(utilities, ArchiveSetIngameLoadFileImpl, out _, false)}",
            
                // Original Code
                "push ebx",
                "push esi"
            };

            _loadPlayerModelRaceHook = hooks.CreateAsmHook(raceHookAsm, 0x00408E9C, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

            string[] menuHookAsm = new[]
            {
                "use32",
                // Push pointer to the string.
                "push esi",
                $"{AsmHelpers.AssembleAbsoluteCall<OverrideMenuModelNameFn>(utilities, OverrideMenuModelName, out _, false)}",
            };

            _loadPlayerModelMenuHook = hooks.CreateAsmHook(menuHookAsm, 0x00460C26, AsmHookBehaviour.ExecuteFirst).Activate();

            // Compare Super Sonic Gear Model instead of Gear Id
            Memory.Instance.WriteRaw((IntPtr)0x460BCB, new byte[] { 0x83, 0xFB, 0x12, 0x90, 0x90, 0x90, 0x90 }); // cmp ebx, 0x12

            string[] changeCharacterToSuperAsm = new[]
            {
                "use32",
                // Backup Registers
                $"{utilities.PushCdeclCallerSavedRegisters()}",

                // Offset player pointer and push
                "mov eax, edi",
                "sub eax, 0xBA",
                "push eax",

                $"{AsmHelpers.AssembleAbsoluteCall<OverrideCharacterFn>(utilities, OverrideCharacterImpl, out _, false)}",
                
                // Save Registers
                $"{utilities.PopCdeclCallerSavedRegisters()}",
            };

            _replaceCharacterWithSuperHook = hooks.CreateAsmHook(changeCharacterToSuperAsm, 0x4638C0, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        private void OverrideCharacterImpl(Player* playerptr)
        { 
            var gear = (ExtremeGear*) Sewer56.SonicRiders.API.Player.Gears.GetPointerToElement((int)playerptr->ExtremeGear);
            if (gear->GearModel == ExtremeGearModel.ChaosEmerald)
                playerptr->Character = Characters.SuperSonic;
        }

        private void OverrideMenuModelName(byte* fileNamePtr)
        {
            var fileName = Marshal.PtrToStringAnsi((IntPtr)fileNamePtr);
            if (HandleMissingModel(fileName, out fileName))
            {
                var fileNameBytes = Encoding.ASCII.GetBytes(fileName);
                for (int x = 0; x < fileNameBytes.Length; x++)
                    fileNamePtr[x] = fileNameBytes[x];
            }
        }

        private void* ArchiveSetIngameLoadFileImpl(string fileName, void* maybeDataAddress)
        {
            var filePath = Path.Combine(_dataFolderPath, fileName);
            HandleMissingModel(fileName, out fileName);
            return _originalFunction(fileName, maybeDataAddress);
        }

        private bool HandleMissingModel(string fileName, out string newFileName)
        {
            // NOTE: DO NOT MAKE FILE NAMES LONGER THAN ORIGINAL
            var filePath = Path.Combine(_dataFolderPath, fileName);
            if (!File.Exists(filePath))
            {
                var gearModel = int.Parse(fileName.Substring(2, 2));
                var replacementModel = 0;

                // Bike Check
                if (gearModel > 40 && gearModel < 70)
                    replacementModel = 40;

                // Skate Check
                if (gearModel > 70)
                    replacementModel = 70;

                // Set new fileName
                var builder = new StringBuilder();
                builder.Append(fileName.AsSpan(0, 2));
                builder.Append(replacementModel.ToString("00"));

                // Any suffix (if available)
                if (fileName.Length > builder.Length)
                    builder.Append(fileName.AsSpan(builder.Length));

                newFileName = builder.ToString();
                _generalLogger.WriteLine($"Replacing Missing Model: {fileName} with {newFileName}");
                return true;
            }

            newFileName = fileName;
            return false;
        }

        [Function(CallingConventions.Stdcall)]
        private delegate void OverrideMenuModelNameFn(byte* fileNamePtr);

        [Function(CallingConventions.Stdcall)]
        private delegate void OverrideCharacterFn(Player* playerPtr);
    }
}
