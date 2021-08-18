using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;
using System;
using Reloaded.Memory.Sources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// This controller allows for extra gear slots in the game.
    /// </summary>
    public unsafe class ExtraGearsController : IController
    {
        #region Pointers
        private const int _newGearCount = 255;
        
        // All pointers in game code referring to extreme gear data:
        // i.e. Player.Gears in the library
        private readonly int[] ExtremeGearPtrAddresses = new int[]
        {
            // Offsets from expected address, +4, +5, +5, +5. etc.
            0x408D51, 0x408E30, 0x408F2A, 0x408FC1, 0x414140, 0x4141AF, 0x414679, 0x414856, 0x41489E, 0x44784B,
            0x447AA0, 0x447D49, 0x44C030, 0x450D3A, 0x460A68, 0x460AE7, 0x460BC4, 0x460D79, 0x461139, 0x462862,
            0x462A34, 0x462BD4, 0x462CD9, 0x462DB3, 0x462EF9, 0x462FD3, 0x4630FA, 0x4631B2, 0x463978, 0x465EF9,
            0x466E73, 0x466E95, 0x473683, 0x4743A1, 0x4743C1, 0x4744B2, 0x4744CE, 0x474506, 0x474606, 0x474775,
            0x474795, 0x4B07E0, 0x4B7CEC, 0x4B7D38, 0x4B9444, 0x4BDE7A, 0x4C9B49, 0x4E18E4, 0x4EC80A, 0x4FC251,
            0x4FC2A8, 0x507800, 0x50799D
        };

        // All pointers in game code referring to unlocked gear data.
        private readonly int[] UnlockedGearModelsCodeAddresses = new int[]
        {
            0x460B63, 0x4628B5, 0x462A87, 0x462C27, 0x462E25, 0x463045, 0x463226, 0x4639CA, 0x465C4B, 0x465D66,
            0x465DB4, 0x466D52, 0x46CDBB, 0x46CE57, 0x4742BB, 0x47461F, 0x47462E, 0x50CEDC, 0x50CFE2
        };

        // All pointers in game code referring to gear -> model map
        private readonly int[] GearToModelCodeAddresses = new int[]
        {
            0x460B5D, 0x4628AF, 0x462A81, 0x462C21, 0x00462E19, 0x463039, 0x463220, 0x4639C4
        };

        // Patch total amount of gears available in menu code.
        private readonly int[] MaxGearCountPatch = 
        {
            // > 40
            0x463963, 0x462843, 0x462A13, 0x462BB3, 0x462C97, 0x462EB7, 0x4630B8,

            // < 0
            0x462CAD, 0x462ECD, 0x4630CE
        };
        #endregion

        private ExtremeGear[] _newGears;
        private bool[] _unlockedGearModels;
        private byte[] _gearToModelMap;

        // Also original count for gear->model map.
        private int _originalGearPtr = Sewer56.SonicRiders.API.Player.Gears.Count;
        private int _originalUnlockedModelPtr = Sewer56.SonicRiders.API.State.UnlockedGearModels.Count;
        
        private Logger _log = new Logger(LogCategory.Default);

        public ExtraGearsController()
        {
            // We GC.AllocateArray so stuff gets put on the pinned object heap.
            // Any fixed statement is no-op pinning, and the object will never be moved.
            SetupNewPointers("Gear Data", ref _newGears, ref Sewer56.SonicRiders.API.Player.Gears, ExtremeGearPtrAddresses);
            SetupNewPointers("Unlocked Gear Models", ref _unlockedGearModels, ref Sewer56.SonicRiders.API.State.UnlockedGearModels, UnlockedGearModelsCodeAddresses);
            SetupNewPointers("Gear to Model Map", ref _gearToModelMap, ref Sewer56.SonicRiders.API.State.GearIdToModelMap, GearToModelCodeAddresses);
            //AddGear(_newGears[(int)Sewer56.SonicRiders.Structures.Enums.ExtremeGear.HighBooster]);
        }

        private void AddGear(ExtremeGear gear)
        {
            _log.WriteLine($"Adding Gear in Slot: {_originalGearPtr}");
            var gearType    = gear.GearType;
            _newGears[_originalGearPtr] = gear;
            _gearToModelMap[_originalGearPtr] = (byte)gear.GearModel;

            _originalGearPtr++;
            PatchGearCount(_originalGearPtr);
        }

        private void PatchGearCount(int newCount)
        {
            foreach (var codePointer in MaxGearCountPatch)
            {
                Memory.Instance.SafeWrite((IntPtr)codePointer, (byte)newCount);
            }
        }

        private void SetupNewPointers<T>(string pointerName, ref T[] output, ref RefFixedArrayPtr<T> apiEndpoint, int[] sourceAddresses) where T : unmanaged
        {
            var fixedArrayPtr = new FixedArrayPtr<T>((ulong)apiEndpoint.Pointer, apiEndpoint.Count);
            SetupNewPointers(pointerName, ref output, ref fixedArrayPtr, sourceAddresses);
            apiEndpoint = new RefFixedArrayPtr<T>((ulong)fixedArrayPtr.Pointer, fixedArrayPtr.Count);
        }

        private void SetupNewPointers<T>(string pointerName, ref T[] output, ref FixedArrayPtr<T> apiEndpoint, int[] sourceAddresses) where T : unmanaged
        {
            // Allocate array and copy existing data.
            output = GC.AllocateArray<T>(_newGearCount, true);
            var originalData = apiEndpoint;
            originalData.CopyTo(output, originalData.Count);

            // Set new original gears array
            fixed (T* ptr = output)
            {
                // Fix pointer in library.
                apiEndpoint = new FixedArrayPtr<T>((ulong)ptr, _newGearCount);

                // Patch all pointers in game code.
                var originalAddress = (byte*) originalData.Pointer;
                var newAddress = (byte*) ptr;

                foreach (var codePointer in sourceAddresses)
                {
                    Memory.Instance.SafeRead((IntPtr)codePointer, out int address);
                    var offset = (byte*)(address) - originalAddress;
                    var target = newAddress + offset;
                    Memory.Instance.SafeWrite((IntPtr)codePointer, (int)target);

                    if (offset < 0 || offset > sizeof(T))
                        _log.WriteLine($"[WARNING] Invalid Offset?? Type: {typeof(T).Name}, Offset {offset}, Code Ptr: {codePointer:X}");
                }

                _log.WriteLine($"[{pointerName}] New Pointer: {(long)apiEndpoint.Pointer:X} (from: {(long)originalData.Pointer:X})");
            }
        }
    }
}
