using System;
using System.Runtime.InteropServices;

namespace Riders.Tweakbox.Misc.Pointers;

public unsafe struct NativeAllocation<T> : IDisposable where T : unmanaged
{
    /// <summary>
    /// Pointer to the data.
    /// </summary>
    public T* Data { get; private set; }

    /// <summary>
    /// Number of items in the array.
    /// </summary>
    public int Count;

    /// <summary>
    /// Creates an allocation from an existing allocation.
    /// </summary>
    /// <param name="count">Number of items.</param>
    /// <param name="data">Pointer to data.</param>
    public NativeAllocation(IntPtr data, int count)
    {
        Count = count;
        Data = (T*)data;
    }

    public void Dispose()
    {
        if (Data != (T*)0)
            Marshal.FreeHGlobal((IntPtr)Data);

        Data  = default;
        Count = 0;
    }

    public void Free() => Dispose();

    /// <summary>
    /// Allocates an array of specified type and count.
    /// </summary>
    public static NativeAllocation<T> Create(int count) => new NativeAllocation<T>(Marshal.AllocHGlobal(sizeof(T) * count), count);

    /// <summary>
    /// Copies the elements of the current allocation to another allocation.
    /// </summary>
    /// <param name="other">The other element to copy to.</param>
    public void CopyTo(NativeAllocation<T> other) => this.AsSpan().CopyTo(other.AsSpan());

    /// <summary>
    /// Converts the current allocation into a native span.
    /// </summary>
    public Span<T> AsSpan() => new Span<T>((void*)Data, Count);
}
