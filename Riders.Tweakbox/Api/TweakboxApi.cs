using System.Collections.Generic;
using Riders.Tweakbox.Api.Children;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Api;

public class TweakboxApi : ITweakboxApiImpl, ITweakboxApi
{
    public List<string> LoadedMods { get; private set; } = new List<string>();

    private ICustomGearApi _customGearApi;
    private ICustomCharacterApi _customCharacterApi;
    private IPhysicsApi _physicsApi;

    private ApiBehaviourImplementation _apiBehaviourImplementation;

    public TweakboxApi()
    {
        _physicsApi = new PhysicsApi();
        _customGearApi = IoC.Get<CustomGearApi>();
        _customCharacterApi = IoC.GetSingleton<CustomCharacterApi>();
        _apiBehaviourImplementation = IoC.Get<ApiBehaviourImplementation>();
    }

    /// <inheritdoc />
    public ICustomGearApi GetCustomGearApi() => _customGearApi;

    /// <inheritdoc />
    public IPhysicsApi GetPhysicsApi() => _physicsApi;

    /// <inheritdoc />
    public ICustomCharacterApi GetCustomCharacterApi() => _customCharacterApi;

    // API Register/Unregister.
    public ITweakboxApiImpl Register(string modName)
    {
        LoadedMods.Add(modName);
        return this;
    }

    public void Unregister(string modName) => LoadedMods.Remove(modName);

}
