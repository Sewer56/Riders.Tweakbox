using System;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using System.Collections.Generic;
using Sewer56.SonicRiders.API;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Controllers.CustomCharacterController;

public class CustomCharacterController
{
    public List<ModifyCharacterRequest>[] CharacterOverrides;
    public Dictionary<string, ModifyCharacterRequest> AllOverrides;

    public CustomCharacterController()
    {
        // Init Overrides.
        CharacterOverrides = new List<ModifyCharacterRequest>[(int)(Characters.E10000R + 1)];
        for (int x = 0; x < CharacterOverrides.Length; x++)
            CharacterOverrides[x] = new List<ModifyCharacterRequest>();

        AllOverrides = new Dictionary<string, ModifyCharacterRequest>();
    }

    /// <summary>
    /// Adds custom behaviour to a character in the game.
    /// </summary>
    /// <param name="request">The character data.</param>
    /// <returns>Null if the operation did not suceed, else valid result.</returns>
    public ModifyCharacterRequest AddCharacterBehaviour(ModifyCharacterRequest request)
    {
        if (AllOverrides.ContainsKey(request.CharacterName))
            return null;

        AllOverrides[request.CharacterName] = request;
        CharacterOverrides[request.CharacterId].Add(request);
        return request;
    }

    /// <summary>
    /// Removes a custom character with a specific name.
    /// </summary>
    /// <param name="name">Name of the character used in <see cref="ModifyCharacterRequest.AddCharacterBehaviour"/> when character gear was added.</param>
    /// <returns>True on success, else false.</returns>
    public bool RemoveCharacterBehaviour(string name)
    {
        if (!AllOverrides.Remove(name, out var request))
            return false;

        CharacterOverrides[request.CharacterId].Remove(request);
        return true;
    }

    /// <summary>
    /// Returns all character behaviours
    /// </summary>
    /// <param name="index">Index of the character behaviours.</param>
    /// <param name="behaviours">The list of character behaviours.</param>
    public bool TryGetAllCharacterBehaviours(int index, out List<ModifyCharacterRequest> behaviours)
    {
        if (index < 0 || index >= CharacterOverrides.Length)
        {
            behaviours = default;
            return false;
        }

        var results = CharacterOverrides[index];
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
    
    internal bool TryGetCharacterBehaviours_Internal(int index, out List<ModifyCharacterRequest> behaviours)
    {
        if (index < 0 || index >= CharacterOverrides.Length)
        {
            behaviours = default;
            return false;
        }

        var results = CharacterOverrides[index];
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
