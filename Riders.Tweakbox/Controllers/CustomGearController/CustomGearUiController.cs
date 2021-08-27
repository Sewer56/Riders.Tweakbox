using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Services.Placeholder;
using Riders.Tweakbox.Services.Texture;
using Sewer56.SonicRiders.Structures.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Sewer56.SonicRiders.Structures.Gameplay;
using Reloaded.Hooks.Definitions.X86;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Pointers;
using Sewer56.SonicRiders;
using Riders.Tweakbox.Services.TextureGen;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Parser.Menu.Metadata;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs;
using Sewer56.SonicRiders.Parser.TextureArchive;
using Sewer56.SonicRiders.Parser.TextureArchive.Structs;
using Sewer56.SonicRiders.Structures.Functions;
using StructLinq;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using ExtremeGear = Sewer56.SonicRiders.Structures.Enums.ExtremeGear;
using Functions = Sewer56.SonicRiders.Functions.Functions;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Services.TextureGen.Structs;

namespace Riders.Tweakbox.Controllers.CustomGearController;

internal unsafe class CustomGearUiController
{
    // Placeholder Textures
    private List<TextureInjectionAllocationEx> _allAllocations = new List<TextureInjectionAllocationEx>();
    private TextureInjectionAllocationEx _iconAllocations => _allAllocations[0];
    private TextureInjectionAllocationEx _nameAllocations => _allAllocations[1];

    private void* _originalXvrsPointer;
    private void* _originalMetadataPointer;
    private void* _newXvrsPointer;
    private void* _newMetadataPointer;

    // Allocator
    private PvrtTextureInjectionAllocatorService _allocatorService;

    // Private 
    private CustomGearCodePatcher _codePatcher;
    private TextureService _textureService;
    private PlaceholderTextureService _placeholderService;
    private Logger _log = new Logger(LogCategory.CustomGear);
    private ManualTextureDictionary _redirectDictionary;
    private HeapController _heapController;

    // File Generation
    private InMemoryMenuMetadata _originalMetadata = new InMemoryMenuMetadata();
    private InMemoryMenuMetadata _newMetadata = new InMemoryMenuMetadata();

    // Hooks
    private Functions.Spani_SABInitFn _initFn = Functions.Initialize2dMetadataFile.GetWrapper();
    private Functions.GetSet_TexFn _loadXvrsFromArchiveFn = Functions.LoadXvrsFromArchive.GetWrapper();

    private IAsmHook _setTextureIndexHook;
    private IAsmHook _createMetadataHook;
    private IAsmHook _createXvrsHook;

    internal CustomGearUiController(CustomGearCodePatcher codePatcher)
    {
        _codePatcher = codePatcher;
        _placeholderService = IoC.GetSingleton<PlaceholderTextureService>();
        _allocatorService   = IoC.GetSingleton<PvrtTextureInjectionAllocatorService>();
        _textureService     = IoC.GetSingleton<TextureService>();
        
        // Custom file cleanup support
        _heapController     = IoC.GetSingleton<HeapController>();
        _heapController.OnFreeFrame += FreeCustomFiles;

        // Add texture replacement support.
        _redirectDictionary = new ManualTextureDictionary();
        _textureService.AddDictionary(_redirectDictionary, false);

        // Allocate Dummy Textures. DO NOT REORDER!!
        var requiredExtraGears = codePatcher.AvailableSlots;
        MakeAllocation(requiredExtraGears, new PvrtGeneratorSettings() { Width = 128, Height = 128 }); // Icons
        MakeAllocation(requiredExtraGears, new PvrtGeneratorSettings() { Width = 128, Height = 16 });  // Titles

        // Texture Index Override
        var hooks = IoC.GetSingleton<IReloadedHooks>();
        var utilities = IoC.GetSingleton<IReloadedHooksUtilities>();

        string[] setTextureIdHook = new[]
        {
            "use32",

            // Get player ptr
            "lea eax, [ecx+ecx*8]",
            "shl eax, 9",
            "add eax, 0x6A4B80",

            $"{utilities.PushCdeclCallerSavedRegisters()}", // Save registers
            "push esi", // 2d object
            "push eax", // player ptr
            $"{utilities.AssembleAbsoluteCall<Tm2dPlayerFunction>(SetCustomTextureIndexHook, out _, false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}", // Restore registers
        };

        _setTextureIndexHook = hooks.CreateAsmHook(setTextureIdHook, 0x461756).Activate();

        // Create Metadata Hook
        string[] createMetadataHook = new[]
        {
            "use32",

            // Save Registers
            "push ecx", 
            "push edx",

            "push 0x17DD640", // Re-push parameter.
            $"{utilities.AssembleAbsoluteCall<Spani_SABInitWrapper>(SabInitHook, out _, false)}",
            
            // Restore registers
            "pop edx",
            "pop ecx",
            // Note: No pop after original call; so no add esp 4 here.

            // Original Code
            "mov ecx, [esp + 78h]"
        };

        _createMetadataHook = hooks.CreateAsmHook(createMetadataHook, 0x0040656A, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

        // Create Metadata Hook
        string[] loadXvrsHook = new[]
        {
            "use32",

            // Call our code
            $"{utilities.AssembleAbsoluteCall<Functions.GetSet_TexFn>(LoadXvrsHook, out _, false)}",
            
            // Original Code
            "mov eax, [esp+88h]",

            // Note: No pop after original call; so no add esp 4 here.
        };

        _createXvrsHook = hooks.CreateAsmHook(loadXvrsHook, 0x0040658E, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
    }

    private void LoadXvrsHook(void* pridersarchivedata, int* pfileoffset, void** ppPvrtTextureHeaders, MemoryHeapHeaderHigh* pHeapHighHeader)
    {
        // Just in case!
        int offset = 0;
        if (_newXvrsPointer != (void*) 0)
        {
            _loadXvrsFromArchiveFn(_newXvrsPointer, &offset, ppPvrtTextureHeaders, pHeapHighHeader);
            return;
        }

        var startOffset = (byte*)pridersarchivedata + *pfileoffset;
        var archiveSize = (*(pfileoffset + 1)) - (*pfileoffset);
        
        // Read original file.
        var unmanagedMemoryStream = new UnmanagedMemoryStream(startOffset, archiveSize);
        var reader = new TextureArchiveReader(unmanagedMemoryStream, archiveSize, false);

        // Write new file
        var writer = new TextureArchiveWriter();

        // Write new file: Add original Files
        foreach (var file in reader.Files)
        {
            var data = reader.GetFile(file);
            var packFile = new PackTextureFile()
            {
                Data = data,
                Name = file.Name
            };

            writer.Files.Add(packFile);
        }

        // Write new file: Add new files.
        int dummyCount = 0;
        foreach (var allocation in _allAllocations)
        {
            for (int x = 0; x < allocation.Count; x++)
            {
                writer.Files.Add(new PackTextureFile()
                {
                    Name = $"DUM{dummyCount}",
                    Data = allocation.GeneratePvrt((uint)x)
                });

                dummyCount++;
            }
        }
        
        // Copy file to unmanaged memory in an aligned address.
        var fileSize = writer.EstimateFileSize(TextureArchiveWriterSettings.PC);
        _originalXvrsPointer = startOffset;
        _newXvrsPointer = (void*) Marshal.AllocHGlobal(fileSize);
        writer.Write(new UnmanagedMemoryStream((byte*)_newXvrsPointer, fileSize, fileSize, FileAccess.ReadWrite), TextureArchiveWriterSettings.PC);
        
        // Load the file.
        _loadXvrsFromArchiveFn(_newXvrsPointer, &offset, ppPvrtTextureHeaders, pHeapHighHeader);
    }

    // Modify the Menu Metadata file as its loaded to define new textures.
    private int SabInitHook(int* pFileoffset, byte* pRidersarchivedata, void** ppmetadata)
    {
        // Just in case!
        if (_newMetadataPointer != (void*)0)
        {
            *ppmetadata = _newMetadataPointer;
            _initFn(_newMetadataPointer);
            return 1;
        }

        // Parse original file
        _originalMetadataPointer = (pRidersarchivedata + *pFileoffset);
        _originalMetadata.Initialize((MetadataHeader*)_originalMetadataPointer, false);

        // Calculate sizes and indexes.
        var fileSize = (int)_originalMetadata.FileSize;
        var additionalTextures = _allAllocations.ToStructEnumerable().Sum(x => x.Count, x => x);
        var additionalEntriesSize = sizeof(TextureIdEntry) * (additionalTextures);
        var nextXvrsTextureId = GetHighestTextureId(_originalMetadata.TextureIdEntries, _originalMetadata.TextureIdHeader->NumTextures) + 1;
        
        // Allocate & Copy Data
        _newMetadataPointer = (void*) Marshal.AllocHGlobal(fileSize + additionalEntriesSize);
        Unsafe.CopyBlockUnaligned(_newMetadataPointer, _originalMetadataPointer, (uint)fileSize);

        // Add entries
        _newMetadata.Initialize((MetadataHeader*)_newMetadataPointer, false);
        var textureHeader = &_newMetadata.TextureIdEntries[_newMetadata.TextureIdHeader->NumTextures];
        
        for (int x = 0; x < additionalTextures; x++)
        {
            textureHeader[x] = new TextureIdEntry()
            {
                NormalizedHeight = 1,
                NormalizedPosX = 0,
                NormalizedPosY = 0,
                NormalizedWidth = 1,
                Unknown = 0,
                XvrsTextureId = (short)nextXvrsTextureId++
            };
        }

        var metadataTextureId = _newMetadata.TextureIdHeader->NumTextures;
        foreach (var allocation in _allAllocations)
        {
            allocation.TextureIndex = metadataTextureId;
            metadataTextureId += allocation.Count;
        }

        _newMetadata.TextureIdHeader->NumTextures += additionalTextures;

        // Initialize with new pointer.
        *ppmetadata = _newMetadataPointer;
        _initFn(_newMetadataPointer);
        return 1;
    }

    private int GetHighestTextureId(TextureIdEntry* firstEntry, int entryCount)
    {
        int max = firstEntry->XvrsTextureId;
        for (int x = 1; x < entryCount; x++)
        {
            var id = firstEntry[x].XvrsTextureId;
            if (id > max)
                max = id;
        }

        return max;
    }

    private void SetCustomTextureIndexHook(Player* player, GearSelSys2DObject* object2d)
    {
        var gear = player->ExtremeGear;
        if (gear > ExtremeGear.Cannonball)
        {
            var gearOffset = (int) (gear - ExtremeGear.Cannonball) - 1;
            object2d->GearTexNo = (ushort)(_iconAllocations.TextureIndex + gearOffset);
            object2d->GearNameTexNo = (ushort)(_nameAllocations.TextureIndex + gearOffset);
        }
    }

    private void FreeCustomFiles(IntPtr pointer)
    {
        // Assume object is allocated on front heap.
        if ((void*)pointer < _originalXvrsPointer)
        {
            _log.WriteLine($"[{nameof(CustomGearUiController)}] Freeing Xvrs");
            Marshal.FreeHGlobal((IntPtr) _newXvrsPointer);
            _originalXvrsPointer = (void*) 0;
            _newXvrsPointer = (void*)0;
        }

        if ((void*)pointer < _originalMetadataPointer)
        {
            _log.WriteLine($"[{nameof(CustomGearUiController)}] Freeing Metadata");
            Marshal.FreeHGlobal((IntPtr) _newMetadataPointer);
            _originalMetadataPointer = (void*) 0;
            _newMetadataPointer = (void*)0;
        }
    }

    internal void AddGear(CustomGearData data, AddGearResult result)
    {
        // Get icon paths.
        data.IconPath = !string.IsNullOrEmpty(data.IconPath) ? data.IconPath : GetPlaceholderIcon(ref data.GearData, data.GearIndex);
        data.NamePath = !string.IsNullOrEmpty(data.NamePath) ? data.NamePath : _placeholderService.TitleIconPlaceholders[data.GearIndex % _placeholderService.TitleIconPlaceholders.Length];

        // Set overrides.
        var indexOffset = data.GearIndex - _codePatcher.OriginalGearCount;
        _redirectDictionary.TryAddTextureFromFilePath(data.IconPath, _iconAllocations.Hashes[indexOffset]);
        _redirectDictionary.TryAddTextureFromFilePath(data.NamePath, _nameAllocations.Hashes[indexOffset]);
    }

    internal void Reset()
    {
        foreach (var allocation in _allAllocations)
        {
            foreach (var hash in allocation.Hashes)
            {
                _redirectDictionary.TryRemoveTexture(hash);
            }
        }
    }

    private void MakeAllocation(int count, PvrtGeneratorSettings options)
    {
        var result = _allocatorService.Allocate(count, options);
        _allAllocations.Add(new TextureInjectionAllocationEx(result));

        foreach (var hash in result.Hashes)
            _textureService.DontGenerateMipmaps(hash);
    }

    private string GetPlaceholderIcon(ref Sewer56.SonicRiders.Structures.Gameplay.ExtremeGear gear, int gearIndex)
    {
        return gear.GearType switch
        {
            GearType.Bike => ModuloSelectFromArray(_placeholderService.BikeIconPlaceholders, gearIndex),
            GearType.Skate => ModuloSelectFromArray(_placeholderService.SkateIconPlaceholders, gearIndex),
            GearType.Board => ModuloSelectFromArray(_placeholderService.BoardIconPlaceholders, gearIndex),
            _ => ModuloSelectFromArray(_placeholderService.BoardIconPlaceholders, gearIndex)
        };
    }
    
    private T ModuloSelectFromArray<T>(T[] items, int index) => items[index % items.Length];

    #region Definitions
    [Function(new Register[] { eax, edx }, eax, StackCleanup.Callee)]
    internal delegate int Spani_SABInitWrapper(int* pFileOffset, byte* pRidersArchiveData, void** ppMetadata);

    [Function(CallingConventions.Stdcall)]
    internal delegate void Tm2dPlayerFunction(Player* player, GearSelSys2DObject* object2d);

    /// <summary>
    /// Note: This will be merged to Sewer56.SonicRiders once the struct is standardised after additional reverse engineering.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xB0)]
    internal struct GearSelSys2DObject
    {
        [FieldOffset(0)]
        public Matrix4x4 Matrix;

        [FieldOffset(0x40)]
        public void* pUnknown;

        [FieldOffset(0x44)]
        public short SomeObjectIndex;

        [FieldOffset(0x47)]
        public byte PlayerNo;

        [FieldOffset(0x50)]
        public int PvrtMetadataPtr;

        [FieldOffset(0x54)]
        public int PvrtTexturePtr;

        [FieldOffset(0x58)]
        public int FrameCounter;

        [FieldOffset(0x70)]
        public byte WeirdAffectsPositioning;

        [FieldOffset(0x71)]
        public byte SetToPlayerNo;

        [FieldOffset(0x98)]
        public ushort GearTexNo;

        [FieldOffset(0x9A)]
        public ushort CharaNameTexNo;

        [FieldOffset(0x9C)]
        public ushort CharacterTypeTexNo;

        [FieldOffset(0x9E)]
        public ushort PlayerIndexTexNo;

        [FieldOffset(0xA0)]
        public ushort GearNameTexNo;
    };
    #endregion
}