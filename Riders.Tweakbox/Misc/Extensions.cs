namespace Riders.Tweakbox.Misc
{
    public static class Extensions
    {
        public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*) 0x0;
    }
}
