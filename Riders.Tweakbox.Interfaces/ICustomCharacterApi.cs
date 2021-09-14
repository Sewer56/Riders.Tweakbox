using Riders.Tweakbox.Interfaces.Structs.Characters;
using System.Collections.Generic;

namespace Riders.Tweakbox.Interfaces;

/// <summary>
/// Allows you to modify existing in-game characters.
/// </summary>
public interface ICustomCharacterApi
{
    /// <summary>
    /// Adds custom behaviour to a character in the game.
    /// </summary>
    /// <param name="request">The character data.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    ModifyCharacterRequest AddCharacterBehaviour(ModifyCharacterRequest request);

    /// <summary>
    /// Removes a custom character with a specific name.
    /// </summary>
    /// <param name="name">Name of the character used in <see cref="ModifyCharacterRequest.AddCharacterBehaviour"/> when character gear was added.</param>
    /// <returns>True on success, else false.</returns>
    bool RemoveCharacterBehaviour(string name);

    /// <summary>
    /// Retrieves all the custom behaviours attached to a character.
    /// </summary>
    /// <param name="index">The index of the character.</param>
    /// <param name="requests">The custom character behaviours.</param>
    /// <returns>True if the custom behaviour data was found, else false.</returns>
    bool TryGetAllCharacterBehaviours(int index, out List<ModifyCharacterRequest> requests);

    /// <summary>
    /// Retrieves only the character behaviour data that should be applied to the character.
    /// </summary>
    /// <param name="index">The index of the character.</param>
    /// <param name="requests">The custom character behaviours.</param>
    /// <returns>True if the custom behaviour data was found, else false.</returns>
    bool TryGetCharacterBehaviours(int index, out List<ModifyCharacterRequest> requests);
}
