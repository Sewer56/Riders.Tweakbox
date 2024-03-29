﻿using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Services.Texture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.X86;
using System.Numerics;
using System.Runtime.CompilerServices;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Services.TextureGen;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Parser.Menu.Metadata;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs;
using Sewer56.SonicRiders.Parser.TextureArchive;
using Sewer56.SonicRiders.Parser.TextureArchive.Structs;
using Sewer56.SonicRiders.Structures.Functions;
using StructLinq;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute.Register;
using Functions = Sewer56.SonicRiders.Functions.Functions;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Services;
using Riders.Tweakbox.Services.TextureGen.Structs;
using Sewer56.SonicRiders.API;
using CustomGearDataInternal = Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal.CustomGearDataInternal;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

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
    private CustomGearService _customGearService;
    private Logger _log = new Logger(LogCategory.CustomGear);
    private ManualTextureDictionary _redirectDictionary;
    private HeapController _heapController;

    // File Generation
    private InMemoryMenuMetadata _originalMetadata = new InMemoryMenuMetadata();
    private InMemoryMenuMetadata _newMetadata = new InMemoryMenuMetadata();

    // Hooks
    private Functions.Spani_SABInitFn _initFn = Functions.Initialize2dMetadataFile.GetWrapper();
    private Functions.GetSet_TexFn _loadXvrsFromArchiveFn = Functions.LoadXvrsFromArchive.GetWrapper();
    private IHook<Functions.Spani_SABInitWrapperFn> _initWrapperFn;

    private IAsmHook _setGearTextureIndexHook;
    private IAsmHook _setEntrySelectionTextureIndexHook;
    private IAsmHook _createMetadataHook;
    private IAsmHook _createXvrsHook;

    internal CustomGearUiController(CustomGearCodePatcher codePatcher)
    {
        _codePatcher = codePatcher;
        _customGearService  = IoC.GetSingleton<CustomGearService>();
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

            $"{utilities.PushCdeclCallerSavedRegisters()}", // Save registers

            // Get player ptr
            "lea eax, [ecx+ecx*8]",
            "shl eax, 9",
            $"add eax, 0x{(long)Sewer56.SonicRiders.API.Player.Players.Pointer:X}",

            "push esi", // 2d object
            "push eax", // player ptr
            $"{utilities.AssembleAbsoluteCall<Tm2dPlayerFunction>(SetCustomTextureIndexInGearselHook, false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}", // Restore registers
        };

        _setGearTextureIndexHook = hooks.CreateAsmHook(setTextureIdHook, 0x461756).Activate();

        string[] setTextureIdOnceSelectedHook = new[]
        {
            "use32",

            $"{utilities.PushCdeclCallerSavedRegisters()}", // Save registers

            // Get player ptr
            "lea eax, [edx+edx*8]",
            "shl eax, 9",
            $"add eax, 0x{(long)Sewer56.SonicRiders.API.Player.Players.Pointer:X}",

            "push esi", // 2d object
            "push eax", // player ptr
            $"{utilities.AssembleAbsoluteCall<Tm2dPlayerFunction>(SetCustomTextureIndexInCharSelectedHook, false)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}", // Restore registers
        };

        _setEntrySelectionTextureIndexHook = hooks.CreateAsmHook(setTextureIdOnceSelectedHook, 0x00461B81).Activate();

        // Create Metadata Hook
        _initWrapperFn = Functions.Initialize2dMetadataFileWrapper.Hook(SabInitWrapperHook).Activate();

        // Create Metadata Hook
        string[] loadXvrsHook = new[]
        {
            "use32",

            // Call our code
            $"{utilities.AssembleAbsoluteCall<Functions.GetSet_TexFn>(LoadXvrsHook, false)}",
            
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
    private int SabInitWrapperHook(int* pFileOffset, void* pRidersArchiveData, void** ppMetadata)
    {
        // Ignore other menus.
        if (ppMetadata != Menu.CharacterSelect.LayoutPointer)
            return _initWrapperFn.OriginalFunction(pFileOffset, pRidersArchiveData, ppMetadata);

        // Reuse our new metadata.
        int newFileOffset = 0;
        if (_newMetadataPointer != (void*)0)
        {
            pFileOffset = &newFileOffset;
            pRidersArchiveData = _newMetadataPointer;
            return _initWrapperFn.OriginalFunction(pFileOffset, pRidersArchiveData, ppMetadata);
        }

        // Parse original file
        _originalMetadataPointer = ((byte*)pRidersArchiveData + *pFileOffset);
        _originalMetadata.Initialize((MetadataHeader*)_originalMetadataPointer, false);

        // Calculate sizes and indexes.
        var fileSize = (int)_originalMetadata.FileSize;
        var additionalTextures = _allAllocations.ToStructEnumerable().Sum(x => x.Count, x => x);
        var additionalEntriesSize = sizeof(TextureEntry) * (additionalTextures);
        var nextXvrsTextureId = GetHighestTextureId(_originalMetadata.TextureIdEntries, _originalMetadata.TextureIdHeader->NumTextures) + 1;

        // Allocate & Copy Data
        _newMetadataPointer = (void*)Marshal.AllocHGlobal(fileSize + additionalEntriesSize);
        Unsafe.CopyBlockUnaligned(_newMetadataPointer, _originalMetadataPointer, (uint)fileSize);

        // Add entries
        _newMetadata.Initialize((MetadataHeader*)_newMetadataPointer, false);
        var textureHeader = &_newMetadata.TextureIdEntries[_newMetadata.TextureIdHeader->NumTextures];

        for (int x = 0; x < additionalTextures; x++)
        {
            textureHeader[x] = new TextureEntry()
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
        pFileOffset = &newFileOffset;
        pRidersArchiveData = _newMetadataPointer;
        return _initWrapperFn.OriginalFunction(pFileOffset, pRidersArchiveData, ppMetadata);
    }

    private int GetHighestTextureId(TextureEntry* firstEntry, int entryCount)
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

    private void SetCustomTextureIndexInGearselHook(Player* player, GearSelSys2DObject* object2d)
    {
        var gear = (int) player->ExtremeGear;
        if (gear >= _codePatcher.OriginalGearCount)
        {
            var gearOffset = (int) (gear - _codePatcher.OriginalGearCount);
            object2d->GearTexNo = (ushort)(_iconAllocations.TextureIndex + gearOffset);
            object2d->GearNameTexNo = (ushort)(_nameAllocations.TextureIndex + gearOffset);
        }
    }

    private void SetCustomTextureIndexInCharSelectedHook(Player* player, GearSelSys2DObject* object2d)
    {
        var gear = (int) player->ExtremeGear;
        if (gear >= _codePatcher.OriginalGearCount)
        {
            var gearOffset = (int)(gear - _codePatcher.OriginalGearCount);
            object2d->PlayerIndexTexNo = (ushort)(_nameAllocations.TextureIndex + gearOffset);
            // Note: The above assignment assigns the gear name; I'm just lazy to make a new struct.
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

    internal void AddGear(CustomGearDataInternal data)
    {
        // Get icon paths.
        _customGearService.UpdateTexturePaths(data, ref data.GearData);
        
        // Set overrides.
        var indexOffset = data.GearIndex - _codePatcher.OriginalGearCount;

        AddTextureOrAnimatedTexture(data.IconPath, data.AnimatedIconFolder, _iconAllocations.Hashes[indexOffset]);
        AddTextureOrAnimatedTexture(data.NamePath, data.AnimatedNameFolder, _nameAllocations.Hashes[indexOffset]);
    }

    private void AddTextureOrAnimatedTexture(string texturePath, string animatedFolderPath, string hash)
    {
        if (!string.IsNullOrEmpty(animatedFolderPath) && Directory.Exists(animatedFolderPath) && !Native.PathIsDirectoryEmptyW(animatedFolderPath))
            _redirectDictionary.TryAddAnimatedTextureFromFolder(animatedFolderPath, hash);
        else
            _redirectDictionary.TryAddTextureFromFilePath(texturePath, hash);

        _textureService.GenerateMipmaps(hash);
        _textureService.TryReloadTexture(hash);
    }

    internal void Reset()
    {
        foreach (var allocation in _allAllocations)
        {
            foreach (var hash in allocation.Hashes)
            {
                _redirectDictionary.TryRemoveTexture(hash);
                _redirectDictionary.TryRemoveAnimatedTexture(hash);
                _textureService.DontGenerateMipmaps(hash);
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

    #region Definitions

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