using System.Runtime.CompilerServices;
using Reloaded.Memory.Pointers;
namespace Riders.Tweakbox.Misc.Extensions;

public static class PointerExtensions
{
    public static unsafe T* AsPointer<T>(this ref T value) where T : unmanaged => (T*)Unsafe.AsPointer(ref value);

    public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*)0x0;

    public static unsafe int Size<T>(this RefFixedArrayPtr<T> ptr) where T : unmanaged => (ptr.Count * sizeof(T));
    public static unsafe BlittablePointer<BlittablePointer<T>> ToBlittable<T>(T** item) where T : unmanaged
    {
        return new BlittablePointer<BlittablePointer<T>>((BlittablePointer<T>*)item);
    }
}
