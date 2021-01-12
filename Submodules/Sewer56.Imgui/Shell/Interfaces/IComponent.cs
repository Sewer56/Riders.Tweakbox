namespace Sewer56.Imgui.Shell.Interfaces
{
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// True if the component is enabled (is selected), else false.
        /// </summary>
        ref bool IsEnabled();

        /// <summary>
        /// True if the component is available (can be selected), else false.
        /// </summary>
        bool IsAvailable() { return true; }

        /// <summary>
        /// Disables the component and restores its original state.
        /// </summary>
        void Disable() { }

        /// <summary>
        /// Re-enables the component and sets the modified state.
        /// </summary>
        void Enable() { }

        /// <summary>
        /// Renders the component to the screen using Dear Imgui.
        /// </summary>
        void Render();
    }
}