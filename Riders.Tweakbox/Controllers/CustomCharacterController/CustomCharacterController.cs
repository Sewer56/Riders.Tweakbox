using System;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using System.Collections.Generic;
using Sewer56.SonicRiders.API;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Log;
using System.Linq;
using OneOf.Types;

namespace Riders.Tweakbox.Controllers.CustomCharacterController;

public class CustomCharacterController : IController
{
    public readonly int MaxCharacters;

    private List<ModifyCharacterRequest>[] _characterOverrides;
    private Dictionary<string, ModifyCharacterRequest> _allOverrides;

    private Logger _log = new Logger(LogCategory.CustomGear);

    public CustomCharacterController()
    {
        MaxCharacters = (int)(Characters.E10000R + 1);

        // Init Overrides.
        _characterOverrides = new List<ModifyCharacterRequest>[MaxCharacters];
        for (int x = 0; x < _characterOverrides.Length; x++)
            _characterOverrides[x] = new List<ModifyCharacterRequest>();

        _allOverrides = new Dictionary<string, ModifyCharacterRequest>();
    }

    /// <summary>
    /// Adds custom behaviour to a character in the game.
    /// </summary>
    /// <param name="request">The character data.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    public ModifyCharacterRequest AddCharacterBehaviour(ModifyCharacterRequest request)
    {
        if (_allOverrides.ContainsKey(request.CharacterName))
            return null;

        _allOverrides[request.CharacterName] = request;
        _characterOverrides[request.CharacterId].Add(request);
        return request;
    }

    /// <summary>
    /// Checks if the user has all character overrides from a given list of overrides.
    /// </summary>
    /// <param name="charNames">List of names of each character.</param>
    public bool HasAllCharacters(IEnumerable<string> charNames, out List<string> missingChars)
    {
        // If there's no chars, we have them all!
        if (charNames == null)
        {
            missingChars = default;
            return true;
        }

        // Else actually do the check.
        missingChars = new List<string>();
        foreach (var charName in charNames)
        {
            if (!_allOverrides.ContainsKey(charName))
                missingChars.Add(charName);
        }

        return missingChars.Count <= 0;
    }

    /// <summary>
    /// Resets all custom character data.
    /// </summary>
    /// <param name="clearCharacters">Removes all known characters if set to true.</param>
    public void Reset(bool clearCharacters = true)
    {
        foreach (var ovr in _characterOverrides)
            ovr.Clear();

        if (clearCharacters)
            _allOverrides.Clear();
    }

    /// <summary>
    /// Clears the list of loaded character modifiers and loads from a new list in order.
    /// </summary>
    /// <param name="names">List of character names to apply.</param>
    public void Reload(IEnumerable<string> names)
    {
        _log.WriteLine($"[{nameof(CustomCharacterController)}] Reloading Characters");
        Reset(false);

        foreach (var name in names)
        {
            if (_allOverrides.TryGetValue(name, out var value))
                AddCharacterBehaviour(value);
        }
    }

    /// <summary>
    /// Reloads all available character data.
    /// </summary>
    public void ReloadAll() => Reload(_allOverrides.Select(x => x.Value.CharacterName));

    /// <summary>
    /// Unloads a custom character behaviour with a specific name.
    /// </summary>
    /// <param name="name">Name of the character used in <see cref="ModifyCharacterRequest.AddCharacterBehaviour"/> when character gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool UnloadCharacter(string name) => RemoveCharacterBehaviour(name, false);

    /// <summary>
    /// Removes a custom character with a specific name.
    /// </summary>
    /// <param name="name">Name of the character used in <see cref="ModifyCharacterRequest.AddCharacterBehaviour"/> when character gear was added.</param>
    /// <param name="clearCharacter">True to fully remove the character, else false to only unload.</param>
    /// <returns>True on success, else false.</returns>
    public bool RemoveCharacterBehaviour(string name, bool clearCharacter = true)
    {
        bool result = clearCharacter ? _allOverrides.Remove(name, out var request) : _allOverrides.TryGetValue(name, out request);

        if (!result)
            return false;

        _characterOverrides[request.CharacterId].Remove(request);
        return true;
    }

    /// <summary>
    /// Returns all character behaviours
    /// </summary>
    /// <param name="index">The index of the character.</param>
    /// <param name="behaviours">The list of character behaviours.</param>
    public bool TryGetAllCharacterBehaviours(int index, out List<ModifyCharacterRequest> behaviours)
    {
        if (index < 0 || index >= _characterOverrides.Length)
        {
            behaviours = default;
            return false;
        }

        var results = _characterOverrides[index];
        behaviours = new List<ModifyCharacterRequest>(results.Count);
        foreach (var request in results)
            behaviours.Add(Mapping.Mapper.Map<ModifyCharacterRequest>(request));

        return behaviours.Count > 0;
    }


    /// <summary>
    /// Retrieves only the character behaviour data that should be applied to the character.
    /// Data is retrieved in the order it should be applied.
    /// </summary>
    /// <param name="index">The index of the character.</param>
    /// <param name="behaviours">The custom character behaviours.</param>
    public bool TryGetCharacterBehaviours(int index, out List<ModifyCharacterRequest> behaviours)
    {
        bool result = TryGetCharacterBehaviours_Internal(index, out var originalBehaviours);
        if (result)
            behaviours = Mapping.Mapper.Map<List<ModifyCharacterRequest>>(originalBehaviours);
        else
            behaviours = null;

        return behaviours != null && behaviours.Count > 0;
    }

    /// <summary>
    /// Returns all loaded character behaviours.
    /// </summary>
    internal List<ModifyCharacterRequest>[] GetAllCharacterBehaviours_Internal() => _characterOverrides;

    internal bool TryGetCharacterBehaviours_Internal(int index, out List<ModifyCharacterRequest> behaviours)
    {
        if (index < 0 || index >= _characterOverrides.Length)
        {
            behaviours = default;
            return false;
        }

        var results = _characterOverrides[index];
        behaviours = new List<ModifyCharacterRequest>(results.Count);

        // Get last mandatory behaviour.
        ModifyCharacterRequest lastMandatory = default;
        foreach (var result in results)
        {
            if (!result.Stack)
                lastMandatory = result;
        }

        if (lastMandatory != null)
            behaviours.Add(lastMandatory);

        // Get all optional behaviours.
        for (var x = results.Count - 1; x >= 0; x--)
        {
            var result = results[x];
            if (result.Stack)
                behaviours.Add(result);
        }

        return behaviours.Count > 0;
    }
}
