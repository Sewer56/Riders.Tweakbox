using System;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Controller that contains minor patches to game code.
    /// </summary>
    public class PatchController : IController
    {
        /// <summary>
        /// If enabled, adds code that keeps the lap timer running even after the race finishes.
        /// </summary>
        public IAsmHook InjectRunTimerPostRace;

        /// <summary>
        /// Disables overwriting of race position after the race has completed.
        /// </summary>
        public Patch DisableRacePositionOverwrite = new Patch((IntPtr)0x4B40E6, new byte[] { 0xEB, 0x44 });

        /// <summary>
        /// Allows the player to always start a race in character select.
        /// </summary>
        public Patch AlwaysCanStartRaceInCharacterSelect = new Patch((IntPtr) 0x004634B8, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, });

        /// <summary>
        /// Loads single player character models regardless of whether the character is an AI or split screen is used.
        /// </summary>
        public Patch AlwaysLoadSinglePlayerCharacterModels = new Patch((IntPtr)0x00408E87, new byte[] { 0xEB, 0x0B });

        // Settings
        private TweaksEditorConfig _config = IoC.Get<TweaksEditorConfig>();

        public PatchController()
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var runTimerPostRace = new string[]
            {
                "use32",
                "lea eax, dword [ebp+8]",
                "push 0x00692AE0",
                "push eax",
                $"{utilities.GetAbsoluteCallMnemonics((IntPtr) 0x00414F00, false)}",
                "add esp, 8"
            };

            InjectRunTimerPostRace = SDK.ReloadedHooks.CreateAsmHook(runTimerPostRace, 0x004166EB).Activate();
            _config.ConfigUpdated += OnConfigUpdated;
        }

        /// <inheritdoc />
        public void Disable()
        {
            AlwaysLoadSinglePlayerCharacterModels.Disable();
            InjectRunTimerPostRace.Disable();
            DisableRacePositionOverwrite.Disable();
            AlwaysCanStartRaceInCharacterSelect.Disable();
        }

        /// <inheritdoc />
        public void Enable()
        {
            AlwaysLoadSinglePlayerCharacterModels.Enable();
            InjectRunTimerPostRace.Enable();
            DisableRacePositionOverwrite.Enable();
            AlwaysCanStartRaceInCharacterSelect.Enable();
        }

        private void OnConfigUpdated()
        {
            AlwaysLoadSinglePlayerCharacterModels.Set(_config.Data.SinglePlayerStageData);
        }
    }
}
