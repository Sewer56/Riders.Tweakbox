using System;
using System.Diagnostics;
using EnumsNET;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Contains various hooks of events throughout the game.
    /// </summary>
    public unsafe class EventHook
    {
        public event CharacterSelectFn OnCharaSelect;
        public event CharacterSelectFn AfterCharaSelect;

        public event AsmAction OnStartRace;
        public event AsmFunc OnCheckIfStartRace;

        public event RaceFn OnRaceTask;
        public event RaceFn AfterRaceTask;

        public event AsmAction OnSkipIntro;
        public event AsmFunc OnCheckIfSkipIntro;

        /// <summary>
        /// Provides a "last-chance" event to modify stage load properties, such as the number of players
        /// or cameras to be displayed after stage load. Consider some fields in the <see cref="State"/> class.
        /// </summary>
        public event SetupRace OnSetupRace;

        private IAsmHook _onStartRaceHook;
        private IAsmHook _onCheckIfStartRaceHook;
        private IAsmHook _skipIntroCameraHook;
        private IAsmHook _checkIfSkipIntroCamera;
        private IAsmHook _onSetupRaceSettingsHook;
        
        private IHook<Functions.DefaultTaskFnWithReturn> _charaSelectHook;
        private IHook<Functions.DefaultTaskFnWithReturn> _raceHook;

        public EventHook()
        {
            var utilities = SDK.ReloadedHooks.Utilities;

            var onStartRaceAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnStartRaceHook, out _)}" };
            var ifStartRaceAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x0046364B, Environment.Is64BitProcess) };
            var onCheckIfStartRaceAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCheckIfStartRaceHook, out _, ifStartRaceAsm, null, null, "je")}" };

            var onSkipIntroAsm = new [] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnSkipIntroHook, out _)}" };
            var ifSkipIntroAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x00415F8E, Environment.Is64BitProcess) };
            var onCheckIfSkipIntroAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCheckIfSkipIntroHook, out _, ifSkipIntroAsm, null, null, "je")}" };

            var hooks = SDK.ReloadedHooks;
            _skipIntroCameraHook = hooks.CreateAsmHook(onSkipIntroAsm, 0x00416001, AsmHookBehaviour.ExecuteFirst).Activate();
            _checkIfSkipIntroCamera = hooks.CreateAsmHook(onCheckIfSkipIntroAsm, 0x415F2F, AsmHookBehaviour.ExecuteFirst).Activate();
            _onStartRaceHook = hooks.CreateAsmHook(onStartRaceAsm, 0x0046364B, AsmHookBehaviour.ExecuteFirst).Activate();
            _onCheckIfStartRaceHook = hooks.CreateAsmHook(onCheckIfStartRaceAsm, 0x0046352B, AsmHookBehaviour.ExecuteFirst).Activate();
            _charaSelectHook = Functions.CharaSelectTask.Hook(CharaSelectHook).Activate();
            _raceHook = Functions.RaceSettingTask.Hook(RaceHook).Activate();

            _onSetupRaceSettingsHook = hooks.CreateAsmHook(new[]
            {
                $"use32",
                $"{AsmHelpers.AssembleAbsoluteCall(() => OnSetupRace?.Invoke((Task<TitleSequence, TitleSequenceTaskState>*) (*State.CurrentTask)), out _)}"
            }, 0x0046C139, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        private byte RaceHook()
        {
            OnRaceTask?.Invoke((Task<byte, RaceTaskState>*) (*State.CurrentTask));
            var result = _raceHook.OriginalFunction();
            AfterRaceTask?.Invoke((Task<byte, RaceTaskState>*) (*State.CurrentTask));
            return result;
        }

        private byte CharaSelectHook()
        {
            OnCharaSelect?.Invoke((Task<CharacterSelect, CharacterSelectTaskState>*) (*State.CurrentTask));
            var value = _charaSelectHook.OriginalFunction();
            AfterCharaSelect?.Invoke((Task<CharacterSelect, CharacterSelectTaskState>*) (*State.CurrentTask));
            return value;
        }

        private void OnStartRaceHook() => OnStartRace?.Invoke();
        private bool OnCheckIfStartRaceHook() => OnCheckIfStartRace != null && OnCheckIfStartRace.Invoke();

        private void OnSkipIntroHook() => OnSkipIntro?.Invoke();
        private bool OnCheckIfSkipIntroHook() => OnCheckIfSkipIntro != null && OnCheckIfSkipIntro.Invoke();

        public delegate void RaceFn(Task<byte, RaceTaskState>* task);
        public delegate void CharacterSelectFn(Task<CharacterSelect, CharacterSelectTaskState>* task);
        public delegate void SetupRace(Task<TitleSequence, TitleSequenceTaskState>* task);
    }
}
