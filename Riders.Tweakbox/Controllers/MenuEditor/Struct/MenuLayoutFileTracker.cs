using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook.Misc;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.API;
using StructLinq;
using Functions = Sewer56.SonicRiders.Functions.Functions;

namespace Riders.Tweakbox.Controllers.MenuEditor.Struct;

/// <summary>
/// Tracks where all loaded menu metadata files lie.
/// </summary>
public unsafe class MenuLayoutFileTracker
{
    /// <summary>
    /// Executes when a new menu item is added.
    /// </summary>
    public event Action<Tm2dMetadataFileItem> OnAddItem;

    /// <summary>
    /// Executes when a menu item is removed.
    /// </summary>
    public event Action<Tm2dMetadataFileItem> OnRemoveItem;

    private HashSet<Tm2dMetadataFileItem> _pointers = new HashSet<Tm2dMetadataFileItem>();

    private readonly IHook<Functions.Spani_SABInitWrapperFnPtr> _hook;
    private Logger _logger = new Logger(LogCategory.MenuEditor);
    private static MenuLayoutFileTracker _instance;

    private HeapController _heapController = IoC.GetSingleton<HeapController>();

    public MenuLayoutFileTracker()
    {
        _instance = this;
        _hook = Functions.Initialize2dMetadataFileWrapper.HookAs<Functions.Spani_SABInitWrapperFnPtr>(typeof(MenuLayoutFileTracker), nameof(Initialize2dMetadataFileWrapperImplStatic)).Activate();
        _heapController.OnFreeFrame += RemoveInvalidPointersOnFreeFrame;
    }

    private unsafe int Initialize2dMetadataFileWrapperImpl(int* pFileOffset, byte* pRidersArchiveData, byte** ppMetadata)
    {
        var a = 5;
        var result = _hook.OriginalFunction.Value.Invoke(pFileOffset, pRidersArchiveData, new BlittablePointer<BlittablePointer<byte>>((BlittablePointer<byte>*)ppMetadata));

        var item = new Tm2dMetadataFileItem() { MetadataFilePtrPtr = ppMetadata };
        _pointers.Add(item);
        OnAddItem?.Invoke(item);

        _logger.WriteLine($"[{nameof(MenuLayoutFileTracker)}] New Ptr: {((nint)ppMetadata):X}, ({((nint)(*ppMetadata)):X})");
        return result;
    }

    private void RemoveInvalidPointersOnFreeFrame(IntPtr newAddress)
    {
        Span<Tm2dMetadataFileItem> items = stackalloc Tm2dMetadataFileItem[_pointers.Count];
        int itemIndex = 0;
        foreach (var item in _pointers)
            items[itemIndex++] = item;

        items = items.Slice(0, itemIndex);
        for (int x = 0; x < items.Length; x++)
        {
            var item = items[x];
            if ((*item.MetadataFilePtrPtr) <= (byte*)newAddress && (*item.MetadataFilePtrPtr) >= Heap.StartPtr) 
                continue;

            _pointers.Remove(item);
            OnRemoveItem?.Invoke(item);
            _logger.WriteLine($"[{nameof(MenuLayoutFileTracker)}] Removed Invalid Menu Item: {((nint)(*item.MetadataFilePtrPtr)):X}");
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe int Initialize2dMetadataFileWrapperImplStatic(int* pFileOffset, byte* pRidersArchiveData, byte** ppMetadata) => _instance.Initialize2dMetadataFileWrapperImpl(pFileOffset, pRidersArchiveData, ppMetadata);
}

/// <summary>
/// Encapsulates access to a 2D metadata item.
/// </summary>
public unsafe struct Tm2dMetadataFileItem : IEquatable<Tm2dMetadataFileItem>
{
    /// <summary>
    /// Pointer to where the game memory stores the metadata file.
    /// </summary>
    public byte** MetadataFilePtrPtr;

    public bool Equals(Tm2dMetadataFileItem other) => MetadataFilePtrPtr == other.MetadataFilePtrPtr;

    public override bool Equals(object obj) => obj is Tm2dMetadataFileItem other && Equals(other);

    public override int GetHashCode()
    {
        return unchecked((int)(long)MetadataFilePtrPtr);
    }
}