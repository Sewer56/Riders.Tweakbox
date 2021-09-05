using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Parser.Layout.Enums;

namespace Riders.Tweakbox.Controllers;

public class RemoveParticlesController : IController
{
    private TweakboxConfig _config;
    private ObjectLayoutController.ObjectLayoutController _layoutController;

    public RemoveParticlesController(TweakboxConfig config)
    {
        _config = config;
        _layoutController = IoC.GetSingleton<ObjectLayoutController.ObjectLayoutController>();
        _layoutController.OnLoadLayout += OnLoadLayout;
    }

    private void OnLoadLayout(ref InMemoryLayoutFile layout)
    {
        if (!_config.Data.NoParticles)
            return;

        for (int x = 0; x < layout.Objects.Count; x++)
        {
            ref var obj = ref layout.Objects[x];
            if (obj.Type >= ObjectId.eParSet00 && obj.Type <= ObjectId.eParSet29)
                obj.Type = ObjectId.oInvalid;
        }
    }
}
