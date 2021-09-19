using System.Collections.Generic;
using Riders.Tweakbox.Api.Children;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Api;

public class TweakboxApi : ITweakboxApiImpl, ITweakboxApi
{
    public List<string> LoadedMods { get; private set; } = new List<string>();

    private ICustomGearApi _customGearApi;
    private ICustomCharacterApi _customCharacterApi;
    private IPhysicsApi _physicsApi;
    private ApiPointers _pointers;

    private ApiBehaviourImplementation _apiBehaviourImplementation;

    public TweakboxApi(CustomGearController customGearController)
    {
        _physicsApi = new PhysicsApi();
        _customGearApi = IoC.Get<CustomGearApi>();
        _customCharacterApi = IoC.GetSingleton<CustomCharacterApi>();
        _apiBehaviourImplementation = IoC.Get<ApiBehaviourImplementation>();
        _pointers = new ApiPointers();
        customGearController.AfterGearCountChanged += UpdateApiPointers;
        UpdateApiPointers();
    }

    /// <inheritdoc />
    public ICustomGearApi GetCustomGearApi() => _customGearApi;

    /// <inheritdoc />
    public IPhysicsApi GetPhysicsApi() => _physicsApi;

    public ApiPointers GetPointers() => _pointers;

    /// <inheritdoc />
    public ICustomCharacterApi GetCustomCharacterApi() => _customCharacterApi;

    // API Register/Unregister.
    public ITweakboxApiImpl Register(string modName)
    {
        LoadedMods.Add(modName);
        return this;
    }

    public void Unregister(string modName) => LoadedMods.Remove(modName);

    private unsafe void UpdateApiPointers()
    {
        _pointers.Gears = new ApiPointer((System.IntPtr)Player.Gears.Pointer, Player.Gears.Count);
    }
}
