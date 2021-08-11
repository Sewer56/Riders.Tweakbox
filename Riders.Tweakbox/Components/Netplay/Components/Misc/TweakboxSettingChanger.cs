using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    public class TweakboxSettingChanger : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }

        private bool _oldReturnToStageSelFromRace;
        private bool _oldReturnToStageSelFromTag;
        private bool _oldReturnToStageSelFromSurvival;
        private bool _miscPatchEnableBackwards;
        private bool _hasFpsCap;

        private readonly StageSelectFromRaceController _stageSelRace;
        private readonly StageSelectFromSurvivalController _stageSelSurv;
        private readonly StageSelectFromTagController _stageSelTag;
        private readonly MiscPatchController _miscPatch;
        private readonly TweakboxConfig _tweakboxConfig;

        public TweakboxSettingChanger(StageSelectFromRaceController stageSelRace, StageSelectFromSurvivalController stageSelSurv, StageSelectFromTagController stageSelTag, MiscPatchController miscPatch, TweakboxConfig tweakboxConfig)
        {
            _stageSelRace = stageSelRace;
            _stageSelSurv = stageSelSurv;
            _stageSelTag = stageSelTag;
            _miscPatch = miscPatch;
            _tweakboxConfig = tweakboxConfig;

            _oldReturnToStageSelFromRace     = _stageSelRace.Hook.IsEnabled;
            _oldReturnToStageSelFromSurvival = _stageSelSurv.Hook.IsEnabled;
            _oldReturnToStageSelFromTag      = _stageSelTag.Hook.IsEnabled;
            _miscPatchEnableBackwards        = _miscPatch.EnableGoingBackwards.IsEnabled;
            _hasFpsCap = _tweakboxConfig.Data.RemoveFpsCap;

            _stageSelRace.Hook.Enable();
            _stageSelSurv.Hook.Enable();
            _stageSelTag.Hook.Enable();
            _miscPatch.EnableGoingBackwards.Enable();
            _tweakboxConfig.Data.RemoveFpsCap = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stageSelRace.Hook.Toggle(_oldReturnToStageSelFromRace);
            _stageSelSurv.Hook.Toggle(_oldReturnToStageSelFromSurvival);
            _stageSelTag.Hook.Toggle(_oldReturnToStageSelFromTag);
            _miscPatch.EnableGoingBackwards.Set(_miscPatchEnableBackwards);
            _tweakboxConfig.Data.RemoveFpsCap = _hasFpsCap;
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
