using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
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

        // Settings
        private TweaksEditorConfig _config = IoC.Get<TweaksEditorConfig>();
        private IAsmHook _alwaysLoadSinglePlayerCharacterModels;

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

            var loadSinglePlayerCharModel = new string[]
            {
                "use32",

                // Check if story mode.
                "push eax",
                "mov eax, [0x00692B88]",
                "cmp eax, 100",
                "pop eax",
                $"je story",
                // Not story mode, we can load SP models. Insert null terminator.
                $"mov [esp+0x29], bl",
                $"jmp complete",
                $"story:",
                // Original code for story mode.
                $"mov [esp+0x28], byte 0x4D",
                $"mov [esp+0x29], bl",
                $"complete:"
            };
            _alwaysLoadSinglePlayerCharacterModels = SDK.ReloadedHooks.CreateAsmHook(loadSinglePlayerCharModel, 0x00408E87, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
        }

        /// <inheritdoc />
        public void Disable()
        {
            InjectRunTimerPostRace.Disable();
            DisableRacePositionOverwrite.Disable();
            AlwaysCanStartRaceInCharacterSelect.Disable();
            _alwaysLoadSinglePlayerCharacterModels.Disable();
        }

        /// <inheritdoc />
        public void Enable()
        {
            _alwaysLoadSinglePlayerCharacterModels.Enable();
            InjectRunTimerPostRace.Enable();
            DisableRacePositionOverwrite.Enable();
            AlwaysCanStartRaceInCharacterSelect.Enable();
        }

        public void SetAlwaysLoadSinglePlayerModels(bool isEnabled)
        {
            if (isEnabled)
                _alwaysLoadSinglePlayerCharacterModels.Enable();
            else
                _alwaysLoadSinglePlayerCharacterModels.Disable();
        }

        private void OnConfigUpdated() => SetAlwaysLoadSinglePlayerModels(_config.Data.SinglePlayerStageData);
    }
}
