using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.CharacterPack.DX.Chars.Common;

public abstract class CustomCharacterBase : ICustomCharacter
{
    public ModifyCharacterRequest Request { get; private set; }
    public abstract string Name { get; }
    public abstract Characters Character { get; }

    /// <summary>
    /// Initializes this custom character.
    /// </summary>
    public void Initialize(Interfaces.ICustomCharacterApi characterApi)
    {
        Request = new ModifyCharacterRequest()
        {
            Behaviour = this,
            CharacterId = (int) Character,
            CharacterName = Name,
            Stack = false // Does not combine with other mods.
        };

        characterApi.AddCharacterBehaviour(Request);
    }
}
