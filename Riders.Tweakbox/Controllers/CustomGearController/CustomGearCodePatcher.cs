using Reloaded.Memory.Pointers;
using ExtremeGear = Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Sources;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Player = Sewer56.SonicRiders.API.Player;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// Patches game code and static data to allow for the possibility of custom gears.
/// </summary>
internal unsafe class CustomGearCodePatcher
{
    private static IReloadedHooks Hooks => SDK.ReloadedHooks;
    private static IReloadedHooksUtilities Utilities => SDK.ReloadedHooks.Utilities;

    // Image Ptr 4616c3
    // Also original count for gear->model map.
    public int OriginalGearCount { get; private set; } = Player.OriginalNumberOfGears;
    public int GearCount { get; private set; } = Player.Gears.Count;
    public int AvailableSlots => MaxGearCount - GearCount;
    public bool HasAvailableSlots => AvailableSlots > 0;
    public const int MaxGearCount = 254; // 256th is reserved for "unjoined"

    // New pointer targets.
    private ExtremeGear* _newGearsPtr;
    private ExtremeGear[] _newGears;
    private bool[] _unlockedGearModels;
    private byte[] _gearToModelMap;

    // Code patching shenanigans.
    private int[] _gearCountMinusOneArr;
    private int* _gearCountMinusOne; // Accessed in comparisons from asm code.
    private List<IAsmHook> _boundsCheckPatches = new List<IAsmHook>();

    // Private members
    private Logger _log = new Logger(LogCategory.CustomGear);

    internal CustomGearCodePatcher()
    {
        MakeMaxGearAsmPatches();

        // We GC.AllocateArray so stuff gets put on the pinned object heap.
        // Any fixed statement is no-op pinning, and the object will never be moved.
        SetupNewPointers("Gear Data", ref _newGears, ref Player.Gears, ExtremeGearPtrAddresses);
        SetupNewPointers("Unlocked Gear Models", ref _unlockedGearModels, ref Sewer56.SonicRiders.API.State.UnlockedGearModels, UnlockedGearModelsCodeAddresses);
        SetupNewPointers("Gear to Model Map", ref _gearToModelMap, ref Sewer56.SonicRiders.API.State.GearIdToModelMap, GearToModelCodeAddresses);

        // Set other useful pointers.
        _gearCountMinusOneArr = GC.AllocateUninitializedArray<int>(1, true);
        fixed (int* ptr = &_gearCountMinusOneArr[0])
            _gearCountMinusOne = ptr;

        fixed (ExtremeGear* ptr = &_newGears[0])
            _newGearsPtr = ptr;

        // Patch Branches
        UpdateGearCount(OriginalGearCount);
        PatchOpcodes();
        PatchBoundsChecks();

        // Respect save data
        EventController.OnEnterCharacterSelect += LoadUnlockedGearsFromSave;
    }

    /// <summary>
    /// Adds a new extreme gear to the game.
    /// </summary>
    /// <param name="data">The gear information.</param>
    /// <returns>A pointer to your gear data.</returns>
    internal void AddGear(CustomGearDataInternal data)
    {
        _log.WriteLine($"[{nameof(CustomGearCodePatcher)}] Adding Gear in Slot: {GearCount}");
        ref var gear = ref data.GearData;
        var gearType = gear.GearType;
        _newGears[GearCount] = gear;
        _gearToModelMap[GearCount] = (byte)GetFreeGearModelIndex();

        var gearIndex = GearCount;
        UpdateGearCount(GearCount + 1);
        data.SetGearIndex(gearIndex);
        PatchMaxGearId(gearIndex);
    }

    /// <summary>
    /// Gets the next unused index for an <see cref="ExtremeGearModel"/>.<br></br>
    /// Intended for assigning arbitrary indices to Custom Extreme Gear for the <see cref="_gearToModelMap"/>, such that they can be unlocked independently of the Extreme Gear used for the physical model.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal int GetFreeGearModelIndex()
    {
        bool[] reservedGearModelIndices = GC.AllocateArray<bool>(MaxGearCount);
        for (int x = 0; x <= GearCount; x++)
        {
            int modelIndex = _gearToModelMap[x];
            reservedGearModelIndices[modelIndex] = true;
        }

        for (int x = 0; x < reservedGearModelIndices.Length; x++)
        {
            if (!reservedGearModelIndices[x])
                return x;
        }

        throw new InvalidOperationException("Failed to identify a free gear model index. All possible indices are reserved.");
    }

    /// <summary>
    /// Resets all custom gear data.
    /// </summary>
    internal void Reset(bool removeVanillaGears = false)
    {
        _log.WriteLine($"[{nameof(CustomGearCodePatcher)}] Resetting Gears");
        if (removeVanillaGears)
            OriginalGearCount = 0;

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
        PatchMaxGearId(Math.Max(0, newCount - 1));
        Cleanup(newCount);
        UpdateGearCount(newCount);
    }

    private void Cleanup(int newCount)
    {
        for (int x = Math.Max(0, newCount - 1); x < GearCount; x++)
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
        output = GC.AllocateArray<T>(MaxGearCount, true);
        var originalData = apiEndpoint;
        originalData.CopyTo(output, originalData.Count);

        // Set new original gears array
        fixed (T* ptr = output)
        {
            // Fix pointer in library.
            apiEndpoint = new FixedArrayPtr<T>((ulong)ptr, MaxGearCount);

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

    /// <summary>
    /// Sets the "Unlocked" state of original game's gears to the currently loaded save file.
    /// Any custom gears will be automatically unlocked.
    /// </summary>
    private void LoadUnlockedGearsFromSave()
    {
        // If "Boot to Menu" is enabled, then everything is unlocked anyway, so we don't need to check for saved data.
        var tweakboxConf = IoC.GetSingleton<TweakboxConfig>();
        if (tweakboxConf.Data.BootToMenu)
            return;

        // Reference to the vanilla location for storing gear unlock state
        RefFixedArrayPtr<bool> vanillaUnlockedGearModels = new RefFixedArrayPtr<bool>((ulong)0x017BE4E8, (int)ExtremeGearModel.Cannonball + 1);
        for (var x = 0; x < vanillaUnlockedGearModels.Count; x++)
        {
            bool isUnlocked = vanillaUnlockedGearModels[x];
            State.UnlockedGearModels[x] = isUnlocked;
        }

        // Unlock any remaining custom gears
        int firstFreeGearModelSlot = (int)ExtremeGearModel.Berserker + 1;
        for (var x = firstFreeGearModelSlot; x < State.UnlockedGearModels.Count; x++)
        {
            // If it's defined in the Enum, it's a Vanilla gear, thus managed by the save.
            if (Enum.IsDefined(typeof(ExtremeGearModel), (byte) x))
                continue;

            State.UnlockedGearModels[x] = true;
        }
    }

    private void UpdateGearCount(int newCount)
    {
        GearCount = newCount;
        *_gearCountMinusOne = Math.Max(0, GearCount - 1);
        Player.NumberOfGears = newCount;
        Player.Gears.Count = newCount;
    }

    private void PatchOpcodes()
    {
        foreach (var branch in PatchOpcodeAddresses)
        {
            if (OpcodeShortMap.TryGetValue(*(ushort*)branch, out var replacementShort))
            {
                *(ushort*)branch = replacementShort;
                continue;
            }

            if (OpcodeByteMap.TryGetValue(*(byte*)branch, out var replacementByte))
            {
                *(byte*)branch = replacementByte;
                continue;
            }
        }
    }

    private void PatchBoundsChecks()
    {
        foreach (var patch in MaxGearAsmPatches)
        {
            var asm = GetAsmCompareToMaxGearCount(patch.Register, String.Join(Environment.NewLine, patch.ExtraCode));
            _boundsCheckPatches.Add(Hooks.CreateAsmHook(asm, patch.Address, patch.Behaviour).Activate());
        }
    }

    private string[] GetAsmCompareToMaxGearCount(string registerToCompare, string extraCode)
    {
        return new string[]
        {
            "use32",
            $"cmp {registerToCompare}, [{(long)_gearCountMinusOne}]",
            $"{extraCode}"
        };
    }

    #region Pointers
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
        0x4FC2A8, 0x507800, 0x50799D, 0x41350C, 0x413514, 0x413F3C, 0x413E9F, 0x461EB7, 0x4B4F2F, 0x4F4253,
        0x50782B, 0x507831, 0x507837, 0x50783F, 0x50784E, 0x507856

        // 0x4B9444: Problems with Tag Mode
    };

    // Patch total amount of gears available in menu code.
    private readonly int[] MaxGearCountPatch =
    {
        // > 40
        // Note: Obsoleted for 255 gears by ASM Patches below.
        //0x463963, 0x462843, 0x462A13, 0x462BB3, 0x462C97, 0x462EB7, 0x4630B8,

        // < 0
        0x462CAD, 0x462ECD, 0x4630CE
    };


    private void MakeMaxGearAsmPatches()
    {
        MaxGearAsmPatches = new []
        {
            // Experimental
            new MaxGearCheckAsmPatch(
                0x463961, "edx",
                new string[] { Utilities.AssembleTrueFalseComplete(Utilities.GetAbsoluteJumpMnemonics((IntPtr)0x463968, false), "xor edx, edx", "", "jbe") },
                AsmHookBehaviour.DoNotExecuteOriginal), // TEST

            new MaxGearCheckAsmPatch(0x462841, "eax"), // OK
            new MaxGearCheckAsmPatch(0x462A11, "eax"), // OK
            new MaxGearCheckAsmPatch(0x462BB1, "eax"), // OK
        
            // Experimental
            new MaxGearCheckAsmPatch(0x462C95, "edx",
                new string[] { Utilities.AssembleTrueFalseComplete(Utilities.GetAbsoluteJumpMnemonics((IntPtr)0x462D98, false), "", "", "jbe") },
                AsmHookBehaviour.ExecuteFirst), // TEST

            // Experimental
            new MaxGearCheckAsmPatch(0x462EB5, "edx",
                new string[] { Utilities.AssembleTrueFalseComplete(Utilities.GetAbsoluteJumpMnemonics((IntPtr)0x462FB8, false), "", "", "jbe") },
                AsmHookBehaviour.ExecuteFirst), // TEST

            // Experimental
            new MaxGearCheckAsmPatch(0x4630B6, "edx",
                new string[] { Utilities.AssembleTrueFalseComplete(Utilities.GetAbsoluteJumpMnemonics((IntPtr)0x463199, false), "", "", "jbe") },
                AsmHookBehaviour.ExecuteFirst), // TEST
        };
    }

    private MaxGearCheckAsmPatch[] MaxGearAsmPatches { get; set; }
    private readonly int[] PatchOpcodeAddresses =
    {
        // Patch Signed to Unsigned for 255 gears instead of 127
        // Obsoleted by MaxGearAsmPatches
        // jle -> jbe
        //0x463964, 0x462848, 0x462A18, 0x462BB8, 0x462C98, 0x462EB8, 0x4630B9,

        // movsx (0xBE0F) -> movzx (0xB60F)
        // Corresponds to ExtremeGearPtrAddresses: Duplicates are here just for clarity.
        0x408D40, 0x408E20, 0x408F1D, 0x408FB5, 0x414128, 0x41417E, 0x414656, 0x414874, /*    ,*/ 0x447839,
        0x447A91, 0x447D3A, 0x44C020, 0x450D2A, 0x4609FF, 0x4609FF, 0x460BB4, 0x460D49, 0x461129, /*    */
        /*     */ 0x462B90, 0x462C60, 0x462C60, 0x462E84, 0x462E84, 0x46307F, 0x46307F, /*     */ /*    */
        /*     */ /*     */ /*     */ 0x474368, 0x47436C, 0x474479, 0x473660, 0x473664, 0x473668, /*    */
        /*     */ 0x4B07D0, /*     */ 0x4B7D44, 0x4B942C, 0x4BDE6B, 0x4C9B33, 0x4E18CF, 0x4EC7FB, /*    */
        /*     */ 0x507820, /*     */ 0x4134F5, 0x4134F5, 0x413F29, 0x413E85, 0x461E55, 0x4B4F0A, 0x4F4230,
        0x507820, 0x507820, 0x507820, 0x507820, 0x507820, 0x507820,

        /* Extra ones found via Cheat Engine/Debugging */
        0x4616BC, 0x461B0E, 0x4B9151, 0x50CC60, 0x50CCEF, 0x50CD6E, 0x50C825, 0x50C8B0, 0x461980
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

    // Map to convert signed to unsigned branches
    private Dictionary<byte, byte> OpcodeByteMap = new Dictionary<byte, byte>()
    {
        // jle 0x7E -> jbe 0x76
        { 0x7E, 0x76 },
    };

    // Map to convert signed to unsigned branches
    private Dictionary<ushort, ushort> OpcodeShortMap = new Dictionary<ushort, ushort>()
    {
        // movsx -> movzx
        { 0xBE0F, 0xB60F },
    };

    struct MaxGearCheckAsmPatch
    {
        public uint Address;
        public string Register;
        public string[] ExtraCode;
        public AsmHookBehaviour Behaviour;

        public MaxGearCheckAsmPatch(uint address, string register, string[] extraCode, AsmHookBehaviour behaviour = AsmHookBehaviour.ExecuteAfter)
        {
            Address = address;
            Register = register;
            ExtraCode = extraCode;
            Behaviour = behaviour;
        }

        public MaxGearCheckAsmPatch(uint address, string register, AsmHookBehaviour behaviour = AsmHookBehaviour.ExecuteAfter) : this()
        {
            Address = address;
            Register = register;
            ExtraCode = new string[0];
            Behaviour = behaviour;
        }
    }
    #endregion

}
