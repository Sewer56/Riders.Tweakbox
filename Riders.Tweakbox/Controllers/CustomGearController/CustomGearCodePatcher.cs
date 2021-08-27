using Reloaded.Memory.Pointers;
using Sewer56.SonicRiders.Structures.Gameplay;
using System;
using Reloaded.Memory.Sources;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// Patches game code and static data to allow for the possibility of custom gears.
/// </summary>
internal unsafe class CustomGearCodePatcher
{
    // Image Ptr 4616c3
    // Also original count for gear->model map.
    public int OriginalGearCount { get; private set; } = Player.OriginalNumberOfGears;
    public int GearCount { get; private set; } = Player.Gears.Count;
    public int AvailableSlots => byte.MaxValue - GearCount;

    // New pointer targets.
    private ExtremeGear* _newGearsPtr;
    private ExtremeGear[] _newGears;
    private bool[] _unlockedGearModels;
    private byte[] _gearToModelMap;

    // Private members
    private Logger _log = new Logger(LogCategory.CustomGear);

    internal CustomGearCodePatcher()
    {
        // We GC.AllocateArray so stuff gets put on the pinned object heap.
        // Any fixed statement is no-op pinning, and the object will never be moved.
        SetupNewPointers("Gear Data", ref _newGears, ref Player.Gears, ExtremeGearPtrAddresses);
        SetupNewPointers("Unlocked Gear Models", ref _unlockedGearModels, ref Sewer56.SonicRiders.API.State.UnlockedGearModels, UnlockedGearModelsCodeAddresses);
        SetupNewPointers("Gear to Model Map", ref _gearToModelMap, ref Sewer56.SonicRiders.API.State.GearIdToModelMap, GearToModelCodeAddresses);

        // Set other useful pointers.
        fixed (ExtremeGear* ptr = &_newGears[0])
            _newGearsPtr = ptr;
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear information.</param>
    /// <returns>A pointer to your gear data.</returns>
    internal void AddGear(CustomGearData data, AddGearResult result)
    {
        _log.WriteLine($"[{nameof(CustomGearCodePatcher)}] Adding Gear in Slot: {GearCount}");
        ref var gear = ref data.GearData;
        var gearType = gear.GearType;
        _newGears[GearCount] = gear;
        _gearToModelMap[GearCount] = (byte)gear.GearModel;

        result.GearIndex = GearCount;
        UpdateGearCount(GearCount + 1);
        data.SetGearIndex(result.GearIndex);
        PatchMaxGearId(result.GearIndex);
    }

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    internal void Reset()
    {
        _log.WriteLine($"[{nameof(CustomGearCodePatcher)}] Resetting Gears");
        SetNewGearCount(OriginalGearCount);
    }

    /// <summary>
    /// Sets the new amount of custom gears to use.
    /// This function is supposed to be used to remove gears, not add them.
    /// </summary>
    /// <param name="newCount">The number of gears to have in the game.</param>
    internal void SetNewGearCount(int newCount)
    {
        _log.WriteLine($"[{nameof(CustomGearCodePatcher)}] Setting Gear Count of {newCount}");
        PatchMaxGearId(newCount - 1);
        Cleanup(newCount);
        UpdateGearCount(newCount);
    }

    private void Cleanup(int newCount)
    {
        for (int x = newCount - 1; x < GearCount; x++)
        {
            _newGears[x] = default;
            _gearToModelMap[x] = 0;
        }
    }

    private void PatchMaxGearId(int maxId)
    {
        foreach (var codePointer in MaxGearCountPatch)
            Memory.Instance.SafeWrite((IntPtr)codePointer, (byte)maxId);
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
            var originalAddress = (byte*)originalData.Pointer;
            var newAddress = (byte*)ptr;

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

    private void UpdateGearCount(int newCount)
    {
        GearCount = newCount;
        Player.NumberOfGears = newCount;
        Player.Gears.Count = newCount;
    }

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
}
