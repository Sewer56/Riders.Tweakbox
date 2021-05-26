using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Controllers
{
    public unsafe partial class EventController
    {
        /// <summary>
        /// Sets the stage when the player leaves the course select stage in battle mode picker.
        /// </summary>
        public event AsmAction OnBattleCourseSelectSetStage;

        /// <summary>
        /// Called right before entering character select from the (battle) course select menu.
        /// </summary>
        public event AsmAction OnBattleEnterCharacterSelect;

        /// <summary>
        /// Called as the game is about to enter character select menu.
        /// </summary>
        public event AsmAction OnEnterCharacterSelect;

        /// <summary>
        /// Called as the game is about to leave the course select menu.
        /// </summary>
        public event AsmAction OnExitCourseSelect;

        /// <summary>
        /// Executed when the user exits the character select menu.
        /// </summary>
        public event AsmAction OnExitCharacterSelect;

        /// <summary>
        /// Queries the user whether the character select menu should be left.
        /// </summary>
        public event AsmFunc OnCheckIfExitCharaSelect;
 
        private IAsmHook _onEnterCharacterSelectHook;
        private IAsmHook _onBattleCourseSelectSetStageHook;
        private IAsmHook _onExitCharaSelectHook;
        private IAsmHook _onCheckIfExitCharaSelectHook;

        private void Constructor_Menu(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
        {
            var onBattleCourseSelectSetStageAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnBattleCourseSelectSetStage?.Invoke(), out _)}" };
            _onBattleCourseSelectSetStageHook   = hooks.CreateAsmHook(onBattleCourseSelectSetStageAsm, 0x00464EAA, AsmHookBehaviour.ExecuteAfter).Activate();

            var onExitCharaSelectAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnExitCharacterSelect?.Invoke(), out _)}" };
            var ifExitCharaSelectAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00463741, Environment.Is64BitProcess) };
            var onCheckIfExitCharaSelectAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnCheckIfExitCharaSelect.InvokeIfNotNull(), out _, ifExitCharaSelectAsm, null, null, "je")}" };
            _onCheckIfExitCharaSelectHook = hooks.CreateAsmHook(onCheckIfExitCharaSelectAsm, 0x00463732, AsmHookBehaviour.ExecuteFirst).Activate();
            _onExitCharaSelectHook = hooks.CreateAsmHook(onExitCharaSelectAsm, 0x00463741, AsmHookBehaviour.ExecuteFirst).Activate();

            var onEnterCharacterSelectAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnBattleEnterCharacterSelectHook, out _)}" };
            _onEnterCharacterSelectHook   = hooks.CreateAsmHook(onEnterCharacterSelectAsm, 0x00464EAA, AsmHookBehaviour.ExecuteFirst).Activate();

            OnCourseSelect    += OnBeforeCourseSelect;
            AfterCourseSelect += OnAfterCourseSelect;
        }

        private CourseSelectTaskState _lastStatus = CourseSelectTaskState.Exit;
        private void OnBeforeCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task) => _lastStatus = task->TaskStatus;

        private void OnAfterCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (_lastStatus != CourseSelectTaskState.Exit && task->TaskStatus == CourseSelectTaskState.Exit)
            {
                if (task->TaskData->NextMenu == 1)
                    OnEnterCharacterSelect?.Invoke();
                else if (task->TaskData->NextMenu == 0)
                    OnExitCourseSelect?.Invoke();
            }
        }

        private void OnBattleEnterCharacterSelectHook() => OnBattleEnterCharacterSelect?.Invoke();
    }
}
