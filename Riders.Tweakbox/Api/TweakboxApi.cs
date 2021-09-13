using Riders.Tweakbox.Api.Children;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Api;

public class TweakboxApi : ITweakboxApi
{
    private ICustomGearApi _customGearApi;
    private IPhysicsApi _physicsApi;

    private ApiGearImplementation _apiGearImplementation;

    public TweakboxApi()
    {
        _physicsApi = new PhysicsApi();
        _customGearApi = IoC.Get<CustomGearApi>();
        _apiGearImplementation = IoC.Get<ApiGearImplementation>();
    }

    /// <inheritdoc />
    public ICustomGearApi GetCustomGearApi() => _customGearApi;

    /// <inheritdoc />
    public IPhysicsApi GetPhysicsApi() => _physicsApi;
}
