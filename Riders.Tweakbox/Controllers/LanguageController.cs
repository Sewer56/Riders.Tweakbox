using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Controllers
{
    public class LanguageController : IController
    {
        private IHook<Functions.CdeclReturnIntFn> _readConfigHook;
        private readonly TweakboxConfig _config;

        public LanguageController(TweakboxConfig config)
        {
            _readConfigHook = Functions.ReadConfigFile.Hook(ReadConfigFile).Activate();
            _config = config;
            _config.Data.AddPropertyUpdatedHandler(OnPropertyUpdated);
        }

        private int ReadConfigFile()
        {
            var originalResult = _readConfigHook.OriginalFunction();
            SetMessageLanguage(_config.Data);
            SetVoiceLanguage(_config.Data);
            return originalResult;
        }

        private unsafe void OnPropertyUpdated(string propertyname)
        {
            var data = _config.Data;
            switch (propertyname)
            {
                case nameof(data.Language):
                    SetMessageLanguage(data);
                    break;

                case nameof(data.VoiceLanguage):
                    SetVoiceLanguage(data);
                    LoadAfsFile();
                    break;
            }
        }

        private unsafe void SetVoiceLanguage(TweakboxConfig.Internal data)
        {
            *Sewer56.SonicRiders.API.Misc.ConfigVoiceLanguage = data.VoiceLanguage;
            *Sewer56.SonicRiders.API.Misc.VoiceLanguage = data.VoiceLanguage;
        }

        private unsafe void LoadAfsFile()
        {
            if (*Sewer56.SonicRiders.API.Misc.IsCriInitialized <= 0)
            {
                Log.WriteLine($"[{nameof(LanguageController)}] CRI Is Not Yet Initialised");
                return;
            }

            var afsFilePath = GetVoiceFileForLanguage(_config.Data.VoiceLanguage);
            Log.WriteLine($"Loading AFS File: {afsFilePath}");

            var loadAfs  = Functions.LoadAFSNoWait.GetWrapper();
            var getStat  = Functions.AfsGetStatus.GetWrapper();
            var execMain = Functions.ADXM_ExecMain.GetWrapper();

            const int partitionId = 0;
            AdxfStat stat;
            loadAfs(partitionId, afsFilePath, (void*)0, (void*)0x00743AD0);

            while ((stat = getStat(partitionId)) != AdxfStat.ADXF_STAT_READEND)
            {
                execMain();
            }
        }

        private unsafe void SetMessageLanguage(TweakboxConfig.Internal data)
        {
            *Sewer56.SonicRiders.API.Misc.ConfigMessageLanguage = data.Language;
            *Sewer56.SonicRiders.API.Misc.MessageLanguage = data.Language;
        }

        private string GetVoiceFileForLanguage(VoiceLanguage lang)
        {
            return lang switch
            {
                VoiceLanguage.Japanese => @"Voice\VOICE.AFS",
                VoiceLanguage.English => @"Voice\VOICE_E.AFS",
                _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null)
            };
        }
    }
}
