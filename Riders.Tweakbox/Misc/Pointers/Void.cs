namespace Riders.Tweakbox.Misc.Pointers;

public unsafe struct VoidPtr
{
    public void* Ptr;

    public VoidPtr(void* ptr)
    {
        Ptr = ptr;
    }

    public static implicit operator void*(VoidPtr x) => x.Ptr;
    public static explicit operator VoidPtr(void* x) => new VoidPtr(x);
}

public unsafe struct VoidPtrPtr
{
    public void** Ptr;

    public VoidPtrPtr(void** ptr)
    {
        Ptr = ptr;
    }

    public static implicit operator void**(VoidPtrPtr x) => x.Ptr;
    public static explicit operator VoidPtrPtr(void** x) => new VoidPtrPtr(x);
}
