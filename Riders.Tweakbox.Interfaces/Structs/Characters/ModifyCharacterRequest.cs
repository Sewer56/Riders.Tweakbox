using Riders.Tweakbox.Interfaces.Interfaces;

namespace Riders.Tweakbox.Interfaces.Structs.Characters;

public class ModifyCharacterRequest
{
    /// <summary>
    /// User friendly name for this character replacement.
    /// </summary>
    public string CharacterName;

    /// <summary>
    /// Index of the character.
    /// See: `Sewer56.SonicRiders.Structures.Enums.Characters` for reference.
    /// </summary>
    public int CharacterId;

    /// <summary>
    /// Represents custom character behaviour.
    /// </summary>
    public ICustomCharacter Behaviour;

    /// <summary>
    /// This custom character behaviour stacks and can be applied on top of
    /// other behaviours.
    /// </summary>
    public bool Stack;
}
