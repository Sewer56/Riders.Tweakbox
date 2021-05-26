using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class SinglePlayerStageController : IController
    {
        // Settings
        private TweaksConfig _config = IoC.Get<TweaksConfig>();

        private IHook<Functions.CdeclReturnIntFn> _loadWorldAssetsHook;

        // Hooks Persistent Data

        public SinglePlayerStageController()
        {
            // Now for our hooks.
            _loadWorldAssetsHook = Functions.LoadWorldAssets.Hook(LoadWorldAssetsHook).Activate();
        }

        private int LoadWorldAssetsHook()
        {
            var forceSinglePlayer = _config.Data.SinglePlayerStageData;
            int originalNumCameras = *State.NumberOfCameras;

            if (forceSinglePlayer)
                *State.NumberOfCameras = 1;

            var result = _loadWorldAssetsHook.OriginalFunction();

            *State.NumberOfCameras = originalNumCameras;
            return result;
        }
    }
}
