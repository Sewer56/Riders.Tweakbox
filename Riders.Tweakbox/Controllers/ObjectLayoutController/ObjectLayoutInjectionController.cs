using System.IO;
using Reloaded.Universal.Redirector.Interfaces;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.ObjectLayout;

namespace Riders.Tweakbox.Controllers.ObjectLayoutController;

public unsafe class ObjectLayoutInjectionController : IController
{
    private readonly IRedirectorController _redirector;
    private ObjectLayoutService _layoutService;
    private TweakboxConfig _config;
    private ObjectLayoutController _objectLayoutController = IoC.GetSingleton<ObjectLayoutController>();

    public ObjectLayoutInjectionController(ObjectLayoutService layoutService, TweakboxConfig config)
    {
        _layoutService = layoutService;
        _config = config;
        _objectLayoutController.ReplaceStageLayout += ReplaceStageLayout;
    }

    private byte[] ReplaceStageLayout(int stageid)
    {
        var layout = _layoutService.GetRandomLayoutForStage(stageid);
        if (!string.IsNullOrEmpty(layout))
            return File.ReadAllBytes(layout);

        return null;
    }
}
