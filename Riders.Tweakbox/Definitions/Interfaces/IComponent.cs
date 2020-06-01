namespace Riders.Tweakbox.Definitions.Interfaces
{
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Disables the component and restores its original state.
        /// </summary>
        void Disable();

        /// <summary>
        /// Re-enables the component and sets the modified state.
        /// </summary>
        void Enable();

        /// <summary>
        /// Renders the component to the screen using Dear Imgui.
        /// </summary>
        /// <param name="compEnabled"></param>
        void Render(ref bool compEnabled);
    }
}