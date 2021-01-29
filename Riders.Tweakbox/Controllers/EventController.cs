using System;
using System.Runtime.InteropServices;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
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
    public unsafe class EventController : TaskEvents, IController
    {
        /// <summary>
        /// When the spawn location for all players is about to be set.
        /// </summary>
        public event SetSpawnLocationsStartOfRaceFn OnSetSpawnLocationsStartOfRace;
        
        /// <summary>
        /// After the spawn location for all players has been set.
        /// </summary>
        public event SetSpawnLocationsStartOfRaceFn AfterSetSpawnLocationsStartOfRace;

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
        /// Handler for the method which sets the task to render an item pickup.
        /// </summary>
        public event SetRenderItemPickupTaskHandlerFn SetItemPickupTaskHandler;

        /// <summary>
        /// Checks if the rendering of the filling up of gauge (when pitted) should be rendered.
        /// </summary>
        public event PlayerAsmFunc CheckIfPitSkipRenderGauge;

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
        /// Checks if a specific player is a human character.
        /// </summary>
        public event PlayerAsmFunc OnCheckIfPlayerIsHumanInput;

        /// <summary>
        /// Checks if a specific player is to be given a human indicator.
        /// </summary>
        public event PlayerAsmFunc OnCheckIfPlayerIsHumanIndicator;

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
        public event SetupRaceFn OnSetupRace;

        /// <summary>
        /// Replaces the game's rand function if set. (Random number generator)
        /// </summary>
        public event RandFn Random;

        /// <summary>
        /// Replaces the game's srand function if set. (Seed random number generator)
        /// </summary>
        public event SRandFn SeedRandom;

        /// <summary>
        /// If true, informs the game the player pressed left in the Quicktime event..
        /// </summary>
        public event AsmFunc OnCheckIfQtePressLeft;

        /// <summary>
        /// If true, informs the game the player pressed left in the Quicktime event..
        /// </summary>
        public event AsmFunc OnCheckIfQtePressRight;

        /// <summary>
        /// Replaces the game's rand function call for determining what item to give on pickup.
        /// </summary>
        public event RandFn ItemPickupRandom;

        /// <summary>
        /// Queries the user whether the character select menu should be left.
        /// </summary>
        public event AsmFunc OnCheckIfGiveAiRandomItems;

        /// <summary>
        /// Sets up the task that displays the new lap and results screen once the player crosses for a new lap.
        /// </summary>
        public event SetGoalRaceFinishTaskHandlerFn SetGoalRaceFinishTask;

        /// <summary>
        /// Updates the player's lap counter.
        /// </summary>
        public event UpdateLapCounterHandlerFn UpdateLapCounter;

        /// <summary>
        /// The task used to render the race finish sequence after the final player crosses the finish line.
        /// </summary>
        public event CdeclReturnByteFnFn GoalRaceFinishTask;

        /// <summary>
        /// Executed when all tasks are about to be removed from the heap.
        /// </summary>
        public event CdeclReturnIntFn RemoveAllTasks;

        private IHook<Functions.SRandFn> _srandHook;
        private IHook<Functions.RandFn>  _randHook;
        private IHook<Functions.StartLineSetSpawnLocationsFn> _setSpawnLocationsStartOfRaceHook;
        private IHook<Functions.StartAttackTaskFn> _startAttackTaskHook;
        private IHook<Functions.SetMovementFlagsBasedOnInputFn> _setMovementFlagsOnInputHook;
        private IHook<Functions.SetNewPlayerStateFn> _setNewPlayerStateHook;
        private IHook<Functions.SetRenderItemPickupTaskFn> _setRenderItemPickupTaskHook;
        private IHook<Functions.SetGoalRaceFinishTaskFn> _setGoalRaceFinishTaskHook;
        private IHook<Functions.UpdateLapCounterFn> _updateLapCounterHook;
        private IHook<Functions.CdeclReturnByteFn> _goalRaceFinishTaskHook;
        private IHook<Functions.CdeclReturnIntFn> _removeAllTasksHook;

        private IAsmHook _onCourseSelectSetStageHook;
        private IAsmHook _onExitCharaSelectHook;
        private IAsmHook _onCheckIfExitCharaSelectHook;
        private IAsmHook _onStartRaceHook;
        private IAsmHook _onCheckIfStartRaceHook;
        private IAsmHook _skipIntroCameraHook;
        private IAsmHook _checkIfSkipIntroCamera;
        private IAsmHook _onSetupRaceSettingsHook;
        private IAsmHook _onCheckIsHumanInputHook;
        private IAsmHook _onCheckIfSkipRenderGaugeFill;
        private IAsmHook _onCheckIfHumanInputIndicatorHook;
        private IAsmHook _onGetRandomDoubleInPlayerFunctionHook;
        private IAsmHook _onCheckIfQtePressLeftHook;
        private IAsmHook _onCheckIfQtePressRightHook;
        private IAsmHook _alwaysSeedRngOnIntroSkipHook;
        private IAsmHook _onCheckIfGiveAiRandomItemsHook;
        private Patch  _randItemPickupPatch;
        private IReverseWrapper<Functions.CdeclReturnIntFn> _randItemPickupWrapper;
        private Random _random = new Random();

        private unsafe Pinnable<BlittablePointer<Player>> _tempPlayerPointer;
        
        public EventController()
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var hooks = SDK.ReloadedHooks;

            // Do not move below onCheckIfSkipIntroAsm because both overwrite same regions of code. You want the other to capture this one. 
            _alwaysSeedRngOnIntroSkipHook = hooks.CreateAsmHook(new[] { $"use32", $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, false)}" }, 0x00415F33, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

            var onCourseSelectSetStageAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCourseSelectSetStageHook, out _)}" };
            _onCourseSelectSetStageHook   = hooks.CreateAsmHook(onCourseSelectSetStageAsm, 0x00464EAA, AsmHookBehaviour.ExecuteAfter).Activate();

            var ifQtePressLeftAsm = new string[] { $"mov eax,[edx+0xB3C]", utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4B3721, Environment.Is64BitProcess) };
            var onCheckIfQtePressLeft = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfQtePressLeftHook, out _, ifQtePressLeftAsm, null, null, "je")}" };
            _onCheckIfQtePressLeftHook = hooks.CreateAsmHook(onCheckIfQtePressLeft, 0x4B3716, AsmHookBehaviour.ExecuteFirst).Activate();

            var ifQtePressRightAsm = new string[] { $"mov ecx,[edx+0xB3C]", utilities.GetAbsoluteJumpMnemonics((IntPtr)0x4B3746, Environment.Is64BitProcess) };
            var onCheckIfQtePressRight = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfQtePressedRightHook, out _, ifQtePressRightAsm, null, null, "je")}" };
            _onCheckIfQtePressRightHook = hooks.CreateAsmHook(onCheckIfQtePressRight, 0x4B373B, AsmHookBehaviour.ExecuteFirst).Activate();

            var onExitCharaSelectAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnExitCharaSelectHook, out _)}" };
            var ifExitCharaSelectAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00463741, Environment.Is64BitProcess) };
            var onCheckIfExitCharaSelectAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfExitCharaSelectHook, out _, ifExitCharaSelectAsm, null, null, "je")}" };
            _onCheckIfExitCharaSelectHook = hooks.CreateAsmHook(onCheckIfExitCharaSelectAsm, 0x00463732, AsmHookBehaviour.ExecuteFirst).Activate();
            _onExitCharaSelectHook = hooks.CreateAsmHook(onExitCharaSelectAsm, 0x00463741, AsmHookBehaviour.ExecuteFirst).Activate();

            var onStartRaceAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnStartRaceHook, out _)}" };
            var ifStartRaceAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x0046364B, Environment.Is64BitProcess) };
            var onCheckIfStartRaceAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfStartRaceHook, out _, ifStartRaceAsm, null, null, "je")}" };
            _onStartRaceHook = hooks.CreateAsmHook(onStartRaceAsm, 0x0046364B, AsmHookBehaviour.ExecuteFirst).Activate();
            _onCheckIfStartRaceHook = hooks.CreateAsmHook(onCheckIfStartRaceAsm, 0x0046352B, AsmHookBehaviour.ExecuteFirst).Activate();

            var onSkipIntroAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnSkipIntroHook, out _)}" };
            var ifSkipIntroAsm = new string[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, Environment.Is64BitProcess)}" };
            var onCheckIfSkipIntroAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfSkipIntroHook, out _, ifSkipIntroAsm, null, null, "je")}" };
            _skipIntroCameraHook = hooks.CreateAsmHook(onSkipIntroAsm, 0x00416001, AsmHookBehaviour.ExecuteFirst).Activate();
            _checkIfSkipIntroCamera = hooks.CreateAsmHook(onCheckIfSkipIntroAsm, 0x415F2F, AsmHookBehaviour.ExecuteFirst).Activate();

            _tempPlayerPointer = new Pinnable<BlittablePointer<Player>>(new BlittablePointer<Player>());
            var ifSkipRenderGauge = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x004A17C0, Environment.Is64BitProcess) };
            var onCheckIfSkipRenderGaugeAsm = new[] { $"use32\nmov [{(int)_tempPlayerPointer.Pointer}], edi\n{utilities.AssembleAbsoluteCall(OnCheckIfSkipRenderGaugeHook, out _, ifSkipRenderGauge, null, null, "je")}" };
            _onCheckIfSkipRenderGaugeFill = hooks.CreateAsmHook(onCheckIfSkipRenderGaugeAsm, 0x004A178C, AsmHookBehaviour.ExecuteFirst).Activate();

            var ifIsHumanInput = new string[] { "mov edx, 0" };
            var ifIsNotHumanInput = new string[] { "mov edx, 1" };
            var onCheckIsHumanInputAsm = new[] { $"use32\nmov [{(int)_tempPlayerPointer.Pointer}], esi\n{utilities.AssembleAbsoluteCall(OnCheckIfIsHumanInputHook, out _, ifIsHumanInput, ifIsNotHumanInput, null, "je")}" };
            _onCheckIsHumanInputHook = hooks.CreateAsmHook(onCheckIsHumanInputAsm, 0x004BD0C4, AsmHookBehaviour.ExecuteFirst).Activate();

            var ifIsHumanIndicator = new string[] { "mov ecx, 0" };
            var ifIsNotHumanIndicator = new string[] { "mov ecx, 1" };
            var onCheckIsHumanIndicatorAsm = new[]
            {
                $"use32",
                $"{utilities.PushXmmRegisters(Constants.XmmRegisters)}",
                $"{utilities.AssembleAbsoluteCall(OnCheckIfIsHumanIndicatorHook, out _, ifIsHumanIndicator, ifIsNotHumanIndicator, null, "je")}",
                $"{utilities.PopXmmRegisters(Constants.XmmRegisters)}",
            };

            var onGetRandomDoubleInPlayerFunctionAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall<GetRandomDouble>(TempNextDouble, out _)}" };
            _onCheckIfHumanInputIndicatorHook = hooks.CreateAsmHook(onCheckIsHumanIndicatorAsm, 0x004270D9, AsmHookBehaviour.ExecuteAfter).Activate();
            _onGetRandomDoubleInPlayerFunctionHook = hooks.CreateAsmHook(onGetRandomDoubleInPlayerFunctionAsm, 0x004E1FA7, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

            _startAttackTaskHook = Functions.StartAttackTask.Hook(OnStartAttackTaskHook).Activate();
            _setMovementFlagsOnInputHook = Functions.SetMovementFlagsOnInput.Hook(OnSetMovementFlagsOnInputHook).Activate();
            _setNewPlayerStateHook = Functions.SetPlayerState.Hook(SetPlayerStateHook).Activate();
            _setRenderItemPickupTaskHook = Functions.SetRenderItemPickupTask.Hook(SetRenderItemPickupHook).Activate();
            _setSpawnLocationsStartOfRaceHook = Functions.SetSpawnLocationsStartOfRace.Hook(SetSpawnLocationsStartOfRaceHook).Activate();
            _srandHook = Functions.SRand.Hook(SRandHandler).Activate();
            _randHook  = Functions.Rand.Hook(RandHandler).Activate();

            _onSetupRaceSettingsHook = hooks.CreateAsmHook(new[]
            {
                $"use32",
                $"{utilities.AssembleAbsoluteCall(() => OnSetupRace?.Invoke((Task<TitleSequence, TitleSequenceTaskState>*) (*State.CurrentTask)), out _)}"
            }, 0x0046C139, AsmHookBehaviour.ExecuteFirst).Activate();

            _randItemPickupWrapper = hooks.CreateReverseWrapper<Functions.CdeclReturnIntFn>(ItemPickupRandImpl);
            _randItemPickupPatch = new Patch((IntPtr)0x004C714C, AsmHelpers.AssembleRelativeCall(0x004C714C, (long)_randItemPickupWrapper.WrapperPointer)).ChangePermission().Enable();

            var ifGiveAiRandomItems = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr) 0x004C721F, false) };
            var onCheckIfAiRandomItemsAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(OnCheckIfGiveAiRandomItemsHook, out _, ifGiveAiRandomItems, null, null, "je")}" };
            _onCheckIfGiveAiRandomItemsHook = hooks.CreateAsmHook(onCheckIfAiRandomItemsAsm, 0x004C71F9, AsmHookBehaviour.ExecuteFirst).Activate();

            _setGoalRaceFinishTaskHook = Functions.SetGoalRaceFinishTask.Hook(SetGoalRaceFinishTaskHook).Activate();
            _updateLapCounterHook = Functions.UpdateLapCounter.Hook(UpdateLapCounterHook).Activate();
            _goalRaceFinishTaskHook = Functions.GoalRaceFinishTask.Hook(GoalRaceFinishTaskHook).Activate();
            _removeAllTasksHook = Functions.RemoveAllTasks.Hook(RemoveAllTasksHook).Activate();
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
            _setRenderItemPickupTaskHook.Disable();
            _onCheckIfSkipRenderGaugeFill.Disable();
            _onCheckIsHumanInputHook.Disable();
            _setSpawnLocationsStartOfRaceHook.Disable();
            _onGetRandomDoubleInPlayerFunctionHook.Disable();
            _onCheckIfQtePressLeftHook.Disable();
            _onCheckIfQtePressRightHook.Disable();
            _srandHook.Disable();
            _randHook.Disable();
            _randItemPickupPatch.Disable();
            _alwaysSeedRngOnIntroSkipHook.Disable();
            _onCheckIfGiveAiRandomItemsHook.Disable();
            _setGoalRaceFinishTaskHook.Disable();
            _updateLapCounterHook.Disable();
            _startAttackTaskHook.Disable();
            _goalRaceFinishTaskHook.Disable();
            _removeAllTasksHook.Disable();
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
            _setRenderItemPickupTaskHook.Enable();
            _onCheckIfSkipRenderGaugeFill.Enable();
            _onCheckIsHumanInputHook.Enable();
            _setSpawnLocationsStartOfRaceHook.Enable();
            _onGetRandomDoubleInPlayerFunctionHook.Enable();
            _onCheckIfQtePressLeftHook.Enable();
            _onCheckIfQtePressRightHook.Enable();
            _randHook.Enable();
            _srandHook.Enable();
            _randItemPickupPatch.Enable();
            _alwaysSeedRngOnIntroSkipHook.Enable();
            _onCheckIfGiveAiRandomItemsHook.Enable();
            _setGoalRaceFinishTaskHook.Enable();
            _updateLapCounterHook.Enable();
            _startAttackTaskHook.Enable();
            _goalRaceFinishTaskHook.Enable();
            _removeAllTasksHook.Enable();
        }

        /// <summary>
        /// Invokes the random number seed generator. (Original Function)
        /// </summary>
        public void InvokeSeedRandom(int seed) => _srandHook.OriginalFunction((uint) seed);

        /// <summary>
        /// Invokes the update lap counter original function.
        /// </summary>
        public void InvokeUpdateLapCounterHook(Player* player, int a2) => _updateLapCounterHook.OriginalFunction(player, a2);

        /// <summary>
        /// Invokes the original function for setting the `GOAL` splash on race finish.
        /// </summary>
        public void InvokeSetGoalRaceFinishTask(Player* player) => _setGoalRaceFinishTaskHook.OriginalFunction(player);

        private double TempNextDouble() => _random.NextDouble() * -600.0;

        private int ItemPickupRandImpl() => ItemPickupRandom?.Invoke(_randHook) ?? RandHandler();

        private int RandHandler() => Random?.Invoke(_randHook) ?? _randHook.OriginalFunction();

        private void SRandHandler(uint seed)
        {
            if (SeedRandom != null)
            {
                SeedRandom.Invoke(seed, _srandHook);
                return;
            }

            _srandHook.OriginalFunction(seed);
        }

        private Enum<AsmFunctionResult> OnCheckIfGiveAiRandomItemsHook() => OnCheckIfGiveAiRandomItems != null && OnCheckIfGiveAiRandomItems.Invoke();

        private Task* SetRenderItemPickupHook(Player* player, byte a2, ushort a3) => SetItemPickupTaskHandler != null ? SetItemPickupTaskHandler(player, a2, a3, _setRenderItemPickupTaskHook) 
                                                                                                                      : _setRenderItemPickupTaskHook.OriginalFunction(player, a2, a3);

        private byte SetPlayerStateHook(Player* player, PlayerState state) => SetNewPlayerStateHandler?.Invoke(player, state, _setNewPlayerStateHook) ?? _setNewPlayerStateHook.OriginalFunction(player, state);

        private Player* OnSetMovementFlagsOnInputHook(Player* player)
        {
            OnSetMovementFlagsOnInput?.Invoke(player);
            var result = _setMovementFlagsOnInputHook.OriginalFunction(player);
            AfterSetMovementFlagsOnInput?.Invoke(player);

            return result;
        }

        private int SetSpawnLocationsStartOfRaceHook(int numberOfPlayers)
        {
            OnSetSpawnLocationsStartOfRace?.Invoke(numberOfPlayers);
            var result = _setSpawnLocationsStartOfRaceHook.OriginalFunction(numberOfPlayers);
            AfterSetSpawnLocationsStartOfRace?.Invoke(numberOfPlayers);
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
        private Enum<AsmFunctionResult> OnCheckIfExitCharaSelectHook() => OnCheckIfExitCharaSelect != null && OnCheckIfExitCharaSelect.Invoke();

        private void OnStartRaceHook() => OnStartRace?.Invoke();
        private Enum<AsmFunctionResult> OnCheckIfStartRaceHook() => OnCheckIfStartRace?.Invoke() ?? AsmFunctionResult.Indeterminate;

        private void OnSkipIntroHook() => OnRaceSkipIntro?.Invoke();
        private Enum<AsmFunctionResult> OnCheckIfSkipIntroHook() => OnCheckIfSkipIntro != null && OnCheckIfSkipIntro.Invoke();
        private Enum<AsmFunctionResult> OnCheckIfSkipRenderGaugeHook() => CheckIfPitSkipRenderGauge?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
        private Enum<AsmFunctionResult> OnCheckIfIsHumanInputHook() => OnCheckIfPlayerIsHumanInput?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
        private Enum<AsmFunctionResult> OnCheckIfIsHumanIndicatorHook() => OnCheckIfPlayerIsHumanIndicator?.Invoke(Sewer56.SonicRiders.API.Player.Players.Pointer) ?? AsmFunctionResult.Indeterminate;

        private Enum<AsmFunctionResult> OnCheckIfQtePressedRightHook() => OnCheckIfQtePressRight != null && OnCheckIfQtePressRight();
        private Enum<AsmFunctionResult> OnCheckIfQtePressLeftHook() => OnCheckIfQtePressLeft != null && OnCheckIfQtePressLeft();

        private int UpdateLapCounterHook(Player* player, int a2) => UpdateLapCounter?.Invoke(_updateLapCounterHook, player, a2) ?? _updateLapCounterHook.OriginalFunction(player, a2);
        private int SetGoalRaceFinishTaskHook(Player* player) => SetGoalRaceFinishTask?.Invoke(_setGoalRaceFinishTaskHook, player) ?? _setGoalRaceFinishTaskHook.OriginalFunction(player);
        private byte GoalRaceFinishTaskHook() => GoalRaceFinishTask?.Invoke(_goalRaceFinishTaskHook) ?? _goalRaceFinishTaskHook.OriginalFunction();
        private int RemoveAllTasksHook() => RemoveAllTasks?.Invoke(_removeAllTasksHook) ?? _removeAllTasksHook.OriginalFunction();

        [Function(CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double GetRandomDouble();
        public delegate void SetSpawnLocationsStartOfRaceFn(int numberOfPlayers);
        public delegate void SetupRaceFn(Task<TitleSequence, TitleSequenceTaskState>* task);
        public unsafe delegate void SRandFn(uint seed, IHook<Functions.SRandFn> hook);
        public unsafe delegate int RandFn(IHook<Functions.RandFn> hook);
        public unsafe delegate byte SetNewPlayerStateHandlerFn(Player* player, PlayerState state, IHook<Functions.SetNewPlayerStateFn> hook);
        public unsafe delegate Task* SetRenderItemPickupTaskHandlerFn(Player* player, byte a2, ushort a3, IHook<Functions.SetRenderItemPickupTaskFn> hook);
        public unsafe delegate int SetGoalRaceFinishTaskHandlerFn(IHook<Functions.SetGoalRaceFinishTaskFn> hook, Player* player);
        public unsafe delegate int UpdateLapCounterHandlerFn(IHook<Functions.UpdateLapCounterFn> hook, Player* player, int a2);
        public delegate byte CdeclReturnByteFnFn(IHook<Functions.CdeclReturnByteFn> hook);
        public delegate int CdeclReturnIntFn(IHook<Functions.CdeclReturnIntFn> hook);

        public delegate Enum<AsmFunctionResult> PlayerAsmFunc(Player* player);
    }
}
