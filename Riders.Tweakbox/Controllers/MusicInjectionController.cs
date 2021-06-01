using System;
using System.Diagnostics;
using System.IO;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Universal.Redirector.Interfaces;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services.Music;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class MusicInjectionController : IController
    {
        private IHook<Functions.PlayMusicFn> _playMusicHook;
        private readonly IRedirectorController _redirector;
        private MusicService _musicService;
        private TweaksConfig _tweaksConfig;
        
        public MusicInjectionController(IReloadedHooks hooks, IRedirectorController redirector, MusicService musicService, TweaksConfig tweaksConfig)
        {
            _redirector = redirector;
            _musicService = musicService;
            _tweaksConfig = tweaksConfig;
            _playMusicHook = Functions.PlayMusic.Hook(PlayMusicImpl).Activate();
        }

        private int PlayMusicImpl(void* maybeBuffer, string song)
        {
            var fileName   = Path.GetFileName(song);
            var sourceFile = Path.Combine(IO.DataFolderLocation, song);
            var target     = _musicService.GetRandomTrack(fileName, _tweaksConfig.Data.IncludeVanillaMusic, true);
            _redirector.AddRedirect(sourceFile, target);
            return _playMusicHook.OriginalFunction(maybeBuffer, song);
        }
    }
}
