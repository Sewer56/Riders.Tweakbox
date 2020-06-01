namespace Riders.Tweakbox.Definitions.Interfaces
{
    public interface IConfiguration : ISerializable
    {
        /// <summary>
        /// Apply the current configuration to the game.
        /// </summary>
        void Apply();
    }
}