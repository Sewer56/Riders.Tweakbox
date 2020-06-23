using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class EventController : TaskEvents
    {
        /// <summary>
        /// Executed when an attack is executed by the game.
        /// </summary>
        public event Functions.StartAttackTaskFn OnStartAttackTask;

        /// <summary>
        /// Executed when an attack is executed by the game before <see cref="OnStartAttackTask"/>.
        /// If this returns 1, execution of original code will be omitted.
        /// </summary>
        public event Functions.StartAttackTaskFn OnShouldRejectAttackTask;

        /// <summary>
        /// Executed after an attack is executed by the game.
        /// </summary>
        public event Functions.StartAttackTaskFn AfterStartAttackTask;

        /// <summary>
        /// Executed before the game gets the input flags based on player movements.
        /// </summary>
        public event Functions.SetMovementFlagsBasedOnInputFn OnSetMovementFlagsOnInput;

        /// <summary>
        /// Executed after the game gets the input flags based on player movements.
        /// </summary>
        public event Functions.SetMovementFlagsBasedOnInputFn AfterSetMovementFlagsOnInput;

        /// <summary>
        /// Handles the player state event.
        /// The handler assigned to this event is responsible for calling the original function.
        /// </summary>
        public event SetNewPlayerStateHandlerFn SetNewPlayerStateHandler;

        /// <summary>
        /// Sets the stage when the player leaves the course select stage in battle mode picker.
        /// </summary>
        public event AsmAction OnCourseSelectSetStage;

        /// <summary>
        /// Executed when the user exits the character select menu.
        /// </summary>
        public event AsmAction OnExitCharaSelect;

        /// <summary>
        /// Queries the user whether the character select menu should be left.
        /// </summary>
        public event AsmFunc OnCheckIfExitCharaSelect;

        /// <summary>
        /// Executed when the Enter key is pressed to start a race in character select.
        /// </summary>
        public event AsmAction OnStartRace;

        /// <summary>
        /// Queries the user whether the race should be started.
        /// </summary>
        public event AsmFunc OnCheckIfStartRace;

        /// <summary>
        /// Executed when the stage intro is skipped.
        /// </summary>
        public event AsmAction OnRaceSkipIntro;

        /// <summary>
        /// Queries the user whether the intro should be skipped.
        /// </summary>
        public event AsmFunc OnCheckIfSkipIntro;

        /// <summary>
        /// Provides a "last-chance" event to modify stage load properties, such as the number of players
        /// or cameras to be displayed after stage load. Consider some fields in the <see cref="State"/> class.
        /// </summary>
        public event SetupRace OnSetupRace;

        private RuleSettingsLoop _rule = new RuleSettingsLoop();
        private CourseSelectLoop _course = new CourseSelectLoop();

        private IHook<Functions.StartAttackTaskFn> _startAttackTaskHook;
        private IHook<Functions.SetMovementFlagsBasedOnInputFn> _setMovementFlagsOnInputHook;
        private IHook<Functions.SetNewPlayerStateFn> _setNewPlayerStateHook;
        private IAsmHook _onCourseSelectSetStageHook;
        private IAsmHook _onExitCharaSelectHook;
        private IAsmHook _onCheckIfExitCharaSelectHook;
        private IAsmHook _onStartRaceHook;
        private IAsmHook _onCheckIfStartRaceHook;
        private IAsmHook _skipIntroCameraHook;
        private IAsmHook _checkIfSkipIntroCamera;
        private IAsmHook _onSetupRaceSettingsHook;

        public EventController()
        {
            var utilities = SDK.ReloadedHooks.Utilities;

            var onCourseSelectSetStageAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCourseSelectSetStageHook, out _)}" };

            var onExitCharaSelectAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnExitCharaSelectHook, out _)}" };
            var ifExitCharaSelectAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00463741, Environment.Is64BitProcess) };
            var onCheckIfExitCharaSelectAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCheckIfExitCharaSelectHook, out _, ifExitCharaSelectAsm, null, null, "je")}" };

            var onStartRaceAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnStartRaceHook, out _)}" };
            var ifStartRaceAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x0046364B, Environment.Is64BitProcess) };
            var onCheckIfStartRaceAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCheckIfStartRaceHook, out _, ifStartRaceAsm, null, null, "je")}" };

            var onSkipIntroAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnSkipIntroHook, out _)}" };
            var ifSkipIntroAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, Environment.Is64BitProcess) };
            var onCheckIfSkipIntroAsm = new[] { $"use32\n{AsmHelpers.AssembleAbsoluteCall(OnCheckIfSkipIntroHook, out _, ifSkipIntroAsm, null, null, "je")}" };

            var hooks = SDK.ReloadedHooks;
            _onCourseSelectSetStageHook = hooks.CreateAsmHook(onCourseSelectSetStageAsm, 0x00464EAA, AsmHookBehaviour.ExecuteAfter).Activate();
            _onExitCharaSelectHook = hooks.CreateAsmHook(onExitCharaSelectAsm, 0x00463741, AsmHookBehaviour.ExecuteFirst).Activate();
            _onCheckIfExitCharaSelectHook = hooks.CreateAsmHook(onCheckIfExitCharaSelectAsm, 0x00463732, AsmHookBehaviour.ExecuteFirst).Activate();
            _skipIntroCameraHook = hooks.CreateAsmHook(onSkipIntroAsm, 0x00416001, AsmHookBehaviour.ExecuteFirst).Activate();
            _checkIfSkipIntroCamera = hooks.CreateAsmHook(onCheckIfSkipIntroAsm, 0x415F2F, AsmHookBehaviour.ExecuteFirst).Activate();
            _onStartRaceHook = hooks.CreateAsmHook(onStartRaceAsm, 0x0046364B, AsmHookBehaviour.ExecuteFirst).Activate();
            _onCheckIfStartRaceHook = hooks.CreateAsmHook(onCheckIfStartRaceAsm, 0x0046352B, AsmHookBehaviour.ExecuteFirst).Activate();
            _startAttackTaskHook = Functions.StartAttackTask.Hook(OnStartAttackTaskHook).Activate();
            _setMovementFlagsOnInputHook = Functions.SetMovementFlagsOnInput.Hook(OnSetMovementFlagsOnInputHook).Activate();
            _setNewPlayerStateHook = Functions.SetPlayerState.Hook(SetPlayerStateHook).Activate();

            _onSetupRaceSettingsHook = hooks.CreateAsmHook(new[]
            {
                $"use32",
                $"{AsmHelpers.AssembleAbsoluteCall(() => OnSetupRace?.Invoke((Task<TitleSequence, TitleSequenceTaskState>*) (*State.CurrentTask)), out _)}"
            }, 0x0046C139, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        /// <summary>
        /// Disables the event tracker.
        /// </summary>
        public new void Disable()
        {
            base.Disable();
            _setMovementFlagsOnInputHook.Disable();
            _onCourseSelectSetStageHook.Disable();
            _onExitCharaSelectHook.Disable();
            _onCheckIfExitCharaSelectHook.Disable();
            _onStartRaceHook.Disable();
            _onCheckIfStartRaceHook.Disable();
            _skipIntroCameraHook.Disable();
            _checkIfSkipIntroCamera.Disable();
            _onSetupRaceSettingsHook.Disable();
            _setNewPlayerStateHook.Disable();
        }

        /// <summary>
        /// Re-enables the event tracker.
        /// </summary>
        public new void Enable()
        {
            base.Enable();
            _setMovementFlagsOnInputHook.Enable();
            _onCourseSelectSetStageHook.Enable();
            _onExitCharaSelectHook.Enable();
            _onCheckIfExitCharaSelectHook.Enable();
            _onStartRaceHook.Enable();
            _onCheckIfStartRaceHook.Enable();
            _skipIntroCameraHook.Enable();
            _checkIfSkipIntroCamera.Enable();
            _onSetupRaceSettingsHook.Enable();
            _setNewPlayerStateHook.Enable();
        }

        private byte SetPlayerStateHook(Player* player, PlayerState state)
        {
            if (SetNewPlayerStateHandler != null)
                return SetNewPlayerStateHandler(player, state, _setNewPlayerStateHook);

            return _setNewPlayerStateHook.OriginalFunction(player, state);
        }

        private Player* OnSetMovementFlagsOnInputHook(Player* player)
        {
            OnSetMovementFlagsOnInput?.Invoke(player);
            var result = _setMovementFlagsOnInputHook.OriginalFunction(player);
            AfterSetMovementFlagsOnInput?.Invoke(player);

            return result;
        }

        private int OnStartAttackTaskHook(Player* playerOne, Player* playerTwo, int a3)
        {
            var reject = OnShouldRejectAttackTask?.Invoke(playerOne, playerTwo, a3);
            if (reject.HasValue && reject.Value == 1)
                return 0;

            OnStartAttackTask?.Invoke(playerOne, playerTwo, a3);
            var result = _startAttackTaskHook.OriginalFunction(playerOne, playerTwo, a3);
            AfterStartAttackTask?.Invoke(playerOne, playerTwo, a3);
            return result;
        }

        private void OnCourseSelectSetStageHook() => OnCourseSelectSetStage?.Invoke();

        private void OnExitCharaSelectHook() => OnExitCharaSelect?.Invoke();
        private bool OnCheckIfExitCharaSelectHook() => OnCheckIfExitCharaSelect != null && OnCheckIfExitCharaSelect.Invoke();

        private void OnStartRaceHook() => OnStartRace?.Invoke();
        private bool OnCheckIfStartRaceHook() => OnCheckIfStartRace != null && OnCheckIfStartRace.Invoke();

        private void OnSkipIntroHook() => OnRaceSkipIntro?.Invoke();
        private bool OnCheckIfSkipIntroHook() => OnCheckIfSkipIntro != null && OnCheckIfSkipIntro.Invoke();

        public delegate void SetupRace(Task<TitleSequence, TitleSequenceTaskState>* task);
        public unsafe delegate byte SetNewPlayerStateHandlerFn(Player* player, PlayerState state, IHook<Functions.SetNewPlayerStateFn> hook);
    }
}
