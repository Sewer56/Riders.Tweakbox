﻿using Riders.Tweakbox.Interfaces.Structs.Characters;
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
    /// <param name="clearCharacter">True to fully remove the character, else false to only unload.</param>
    /// <returns>True on success, else false.</returns>
    bool RemoveCharacterBehaviour(string name, bool clearCharacter = true);

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

    /// <summary>
    /// Resets all custom character data.
    /// </summary>
    /// <param name="clearCharacters">Removes all known characters if set to true.</param>
    public void Reset(bool clearCharacters = true);

    /// <summary>
    /// Clears the list of loaded character modifiers and loads from a new list in order.
    /// </summary>
    /// <param name="names">List of character names to apply.</param>
    public void Reload(IEnumerable<string> names);

    /// <summary>
    /// Reloads all available character data.
    /// </summary>
    public void ReloadAll();

    /// <summary>
    /// Unloads a custom character behaviour with a specific name.
    /// </summary>
    /// <param name="name">Name of the character used in <see cref="ModifyCharacterRequest.AddCharacterBehaviour"/> when character gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool UnloadCharacter(string name);
}
