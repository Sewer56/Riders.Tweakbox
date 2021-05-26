namespace Riders.Tweakbox.Misc.Extensions
{
    public static class Pointers
    {
        public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*) 0x0;
    }
}
