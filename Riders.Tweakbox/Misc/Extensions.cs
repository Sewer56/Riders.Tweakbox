using Reloaded.Hooks.Definitions;

namespace Riders.Tweakbox.Misc
{
    public static class Extensions
    {
        public static string PushCdeclCallerSavedRegisters(this IReloadedHooksUtilities utils) => "push eax\npush ecx\npush edx";
        public static string PopCdeclCallerSavedRegisters(this IReloadedHooksUtilities utils) => "pop edx\npop ecx\npop eax";

        public static unsafe bool IsNotNull<T>(T* ptr) where T : unmanaged => ptr != (void*) 0x0;
    }
}
