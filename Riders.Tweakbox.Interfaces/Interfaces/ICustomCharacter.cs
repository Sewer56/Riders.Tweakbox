using Riders.Tweakbox.Interfaces.Structs.Characters;

namespace Riders.Tweakbox.Interfaces.Interfaces;

public interface ICustomCharacter : ICustomStats
{
    /// <summary>
    /// Modifies the character parameters applied to the character.
    /// </summary>
    public ApiCharacterParameters GetCharacterParameters() => default;
}
