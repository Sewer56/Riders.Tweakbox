using Riders.Tweakbox.CharacterPack.DX.Chars.Common;
using Riders.Tweakbox.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.CharacterPack.DX;

public class CustomCharacterPack
{
    /// <summary>
    /// Contains a list of all loaded gears with custom code.
    /// </summary>
    public List<CustomCharacterBase> Characters = new List<CustomCharacterBase>();

    public CustomCharacterPack(string modFolder, ITweakboxApiImpl api)
    {
        var characterApi = api.GetCustomCharacterApi();

        // Get all implemented gears via reflection
        var types = Assembly.GetExecutingAssembly().GetTypes();

        // Initialize all configs.
        var configTypes = types.Where(x => typeof(CustomCharacterBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        foreach (var type in configTypes)
        {
            var character = (CustomCharacterBase)Activator.CreateInstance(type);
            character.Initialize(characterApi);
            Characters.Add(character);
        }
    }

    public void SetCharacterStats()
    {
        var stats = Player.TypeStats;
        stats[0].LevelTwo.AdditiveSpeed = 0.791666687f;
        stats[1].LevelTwo.AdditiveSpeed = 0.777777791f;
        stats[2].LevelTwo.AdditiveSpeed = 0.791666687f;
    }
}
