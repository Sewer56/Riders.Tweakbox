using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;

namespace Riders.Tweakbox.Controllers
{
    public class AllowBackwardsDrivingController : IController
    {
        private TweakboxConfig _config;
        private MiscPatchController _miscPatchController;

        public AllowBackwardsDrivingController(TweakboxConfig config, MiscPatchController miscPatchController)
        {
            _config = config;
            _miscPatchController = miscPatchController;
            _config.Data.AddPropertyUpdatedHandler(OnPropertyUpdated);
        }

        private void OnPropertyUpdated(string propertyname)
        {
            if (propertyname == nameof(_config.Data.AllowBackwardsDriving))
                _miscPatchController.EnableGoingBackwards.Set(_config.Data.AllowBackwardsDriving);
        }
    }
}
