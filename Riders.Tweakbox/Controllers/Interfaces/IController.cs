namespace Riders.Tweakbox.Controllers.Interfaces
{
    public interface IController
    {
        /// <summary>
        /// Disables the current controller from operating.
        /// (e.g. Unhooks functions)
        /// </summary>
        void Disable();

        /// <summary>
        /// Enables the current controller to operate.
        /// (e.g. Re-enables function hooks)
        /// </summary>
        void Enable();
    }
}