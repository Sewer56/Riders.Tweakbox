// ReSharper disable once CheckNamespace
namespace Reloaded.Hooks.Definitions
{
    public static class HooksExtensions
    {
        /// <summary>
        /// Disables or enables the hook based on the value of toggle.
        /// </summary>
        /// <param name="hook">The hook.</param>
        /// <param name="toggle">Whether to enable or disable.</param>
        public static void Toggle(this IAsmHook hook, bool toggle)
        {
            if (toggle)
                hook.Enable();
            else
                hook.Disable();
        }
    }
}
