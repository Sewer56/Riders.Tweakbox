using System;
using System.Numerics;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;
using Void = Reloaded.Hooks.Definitions.Structs.Void;

namespace Riders.Tweakbox.Controllers
{
    public unsafe partial class EventController : TaskEvents, IController
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

        /// <summary>
        /// Executed when the player physics simulation is to be executed.
        /// </summary>
        public event Functions.RunPlayerPhysicsSimulationFn OnRunPlayerPhysicsSimulation;

        /// <summary>
        /// Executed after the player physics simulation has been executed.
        /// </summary>
        public event Functions.RunPlayerPhysicsSimulationFn AfterRunPlayerPhysicsSimulation;

        /// <summary>
        /// Runs the physics simulation for an individual player.
        /// </summary>
        public event RunPlayerPhysicsSimulationFn RunPlayerPhysicsSimulation;

        /// <summary>
        /// Executed before the code to run 1 frame of physics simulation.
        /// </summary>
        public event Functions.CdeclReturnIntFn OnRunPhysicsSimulation;

        /// <summary>
        /// Executed after the code to run 1 frame of physics simulation.
        /// </summary>
        public event Functions.CdeclReturnIntFn AfterRunPhysicsSimulation;

        /// <summary>
        /// Replaces the code to run 1 frame of physics simulation.
        /// </summary>
        public event CdeclReturnIntFn RunPhysicsSimulation;

        private IHook<Functions.StartLineSetSpawnLocationsFn> _setSpawnLocationsStartOfRaceHook;
        private IHook<Functions.StartAttackTaskFn> _startAttackTaskHook;
        private IHook<Functions.SetMovementFlagsBasedOnInputFn> _setMovementFlagsOnInputHook;
        private IHook<Functions.SetNewPlayerStateFn> _setNewPlayerStateHook;
        private IHook<Functions.SetRenderItemPickupTaskFn> _setRenderItemPickupTaskHook;
        private IHook<Functions.SetGoalRaceFinishTaskFn> _setGoalRaceFinishTaskHook;
        private IHook<Functions.UpdateLapCounterFn> _updateLapCounterHook;
        private IHook<Functions.CdeclReturnByteFn> _goalRaceFinishTaskHook;
        private IHook<Functions.CdeclReturnIntFn> _removeAllTasksHook;
        private IHook<Functions.RunPlayerPhysicsSimulationFn> _runPlayerPhysicsSimulationHook;
        private IHook<Functions.CdeclReturnIntFn> _runPhysicsSimulationHook;

        private IAsmHook _onStartRaceHook;
        private IAsmHook _onCheckIfStartRaceHook;
        private IAsmHook _skipIntroCameraHook;
        private IAsmHook _checkIfSkipIntroCamera;
        private IAsmHook _onSetupRaceSettingsHook;
        private IAsmHook _onCheckIsHumanInputHook;
        private IAsmHook _onCheckIfSkipRenderGaugeFill;
        private IAsmHook _onCheckIfHumanInputIndicatorHook;

        private unsafe Pinnable<BlittablePointer<Player>> _tempPlayerPointer = new Pinnable<BlittablePointer<Player>>(new BlittablePointer<Player>());

        public EventController(IReloadedHooks hooks, IReloadedHooksUtilities utilities)
        {
            Constructor_Random(hooks, utilities);
            Constructor_Menu(hooks, utilities);

            var onStartRaceAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnStartRace?.Invoke(), out _)}" };
            var ifStartRaceAsm = new string[] { utilities.GetAbsoluteJumpMnemonics((IntPtr)0x0046364B, Environment.Is64BitProcess) };
            var onCheckIfStartRaceAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnCheckIfStartRace.InvokeIfNotNull(), out _, ifStartRaceAsm, null, null, "je")}" };
            _onStartRaceHook = hooks.CreateAsmHook(onStartRaceAsm, 0x0046364B, AsmHookBehaviour.ExecuteFirst).Activate();
            _onCheckIfStartRaceHook = hooks.CreateAsmHook(onCheckIfStartRaceAsm, 0x0046352B, AsmHookBehaviour.ExecuteFirst).Activate();

            var onSkipIntroAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnRaceSkipIntro?.Invoke(), out _)}" };
            var ifSkipIntroAsm = new string[] { $"{utilities.GetAbsoluteJumpMnemonics((IntPtr)0x00415F8E, Environment.Is64BitProcess)}" };
            var onCheckIfSkipIntroAsm = new[] { $"use32\n{utilities.AssembleAbsoluteCall(() => OnCheckIfSkipIntro.InvokeIfNotNull(), out _, ifSkipIntroAsm, null, null, "je")}" };
            _skipIntroCameraHook = hooks.CreateAsmHook(onSkipIntroAsm, 0x00416001, AsmHookBehaviour.ExecuteFirst).Activate();
            _checkIfSkipIntroCamera = hooks.CreateAsmHook(onCheckIfSkipIntroAsm, 0x415F2F, AsmHookBehaviour.ExecuteFirst).Activate();

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

            _onCheckIfHumanInputIndicatorHook = hooks.CreateAsmHook(onCheckIsHumanIndicatorAsm, 0x004270D9, AsmHookBehaviour.ExecuteAfter).Activate();
            _startAttackTaskHook = Functions.StartAttackTask.Hook(OnStartAttackTaskHook).Activate();
            _setMovementFlagsOnInputHook = Functions.SetMovementFlagsOnInput.Hook(OnSetMovementFlagsOnInputHook).Activate();
            _setNewPlayerStateHook = Functions.SetPlayerState.Hook(SetPlayerStateHook).Activate();
            _setRenderItemPickupTaskHook = Functions.SetRenderItemPickupTask.Hook(SetRenderItemPickupHook).Activate();
            _setSpawnLocationsStartOfRaceHook = Functions.SetSpawnLocationsStartOfRace.Hook(SetSpawnLocationsStartOfRaceHook).Activate();

            _onSetupRaceSettingsHook = hooks.CreateAsmHook(new[]
            {
                $"use32",
                $"{utilities.AssembleAbsoluteCall(() => OnSetupRace?.Invoke((Task<TitleSequence, TitleSequenceTaskState>*) (*State.CurrentTask)), out _)}"
            }, 0x0046C139, AsmHookBehaviour.ExecuteFirst).Activate();

            _setGoalRaceFinishTaskHook = Functions.SetGoalRaceFinishTask.Hook(SetGoalRaceFinishTaskHook).Activate();
            _updateLapCounterHook = Functions.UpdateLapCounter.Hook(UpdateLapCounterHook).Activate();
            _goalRaceFinishTaskHook = Functions.GoalRaceFinishTask.Hook(GoalRaceFinishTaskHook).Activate();
            _removeAllTasksHook = Functions.RemoveAllTasks.Hook(RemoveAllTasksHook).Activate();
            _runPlayerPhysicsSimulationHook = Functions.RunPlayerPhysicsSimulation.Hook(RunPlayerPhysicsSimulationHook).Activate();
            _runPhysicsSimulationHook = Functions.RunPhysicsSimulation.Hook(RunPhysicsSimulationHook).Activate();
        }

        /// <summary>
        /// Invokes the update lap counter original function.
        /// </summary>
        public void InvokeUpdateLapCounter(Player* player, int a2) => _updateLapCounterHook.OriginalFunction(player, a2);

        /// <summary>
        /// Invokes the original function for setting the `GOAL` splash on race finish.
        /// </summary>
        public void InvokeSetGoalRaceFinishTask(Player* player) => _setGoalRaceFinishTaskHook.OriginalFunction(player);

        /// <summary>
        /// Invokes the function that runs the player physics simulation.
        /// </summary>
        public void InvokeRunPlayerPhysicsSimulation(void* somephysicsobjectptr, Vector4* vector, int* playerindex) => _runPlayerPhysicsSimulationHook.OriginalFunction((Void*)somephysicsobjectptr, vector, playerindex);

        

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

        private int RunPlayerPhysicsSimulationHook(void* somephysicsobjectptr, Vector4* vector, int* playerindex)
        {
            OnRunPlayerPhysicsSimulation?.Invoke(somephysicsobjectptr, vector, playerindex);
            var result = RunPlayerPhysicsSimulation?.Invoke(_runPlayerPhysicsSimulationHook, (Void*)somephysicsobjectptr, vector, playerindex) ?? _runPlayerPhysicsSimulationHook.OriginalFunction((Void*)somephysicsobjectptr, vector, playerindex);
            AfterRunPlayerPhysicsSimulation?.Invoke(somephysicsobjectptr, vector, playerindex);
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

        private int RunPhysicsSimulationHook()
        {
            OnRunPhysicsSimulation?.Invoke();
            var result = RunPhysicsSimulation?.Invoke(_runPhysicsSimulationHook) ?? _runPhysicsSimulationHook.OriginalFunction(); 
            AfterRunPhysicsSimulation?.Invoke();
            return result;
        }

        private Enum<AsmFunctionResult> OnCheckIfSkipRenderGaugeHook() => CheckIfPitSkipRenderGauge?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
        private Enum<AsmFunctionResult> OnCheckIfIsHumanInputHook() => OnCheckIfPlayerIsHumanInput?.Invoke(_tempPlayerPointer.Value.Pointer) ?? AsmFunctionResult.Indeterminate;
        private Enum<AsmFunctionResult> OnCheckIfIsHumanIndicatorHook() => OnCheckIfPlayerIsHumanIndicator?.Invoke(Sewer56.SonicRiders.API.Player.Players.Pointer) ?? AsmFunctionResult.Indeterminate;

        private int UpdateLapCounterHook(Player* player, int a2) => UpdateLapCounter?.Invoke(_updateLapCounterHook, player, a2) ?? _updateLapCounterHook.OriginalFunction(player, a2);
        private int SetGoalRaceFinishTaskHook(Player* player) => SetGoalRaceFinishTask?.Invoke(_setGoalRaceFinishTaskHook, player) ?? _setGoalRaceFinishTaskHook.OriginalFunction(player);
        private byte GoalRaceFinishTaskHook() => GoalRaceFinishTask?.Invoke(_goalRaceFinishTaskHook) ?? _goalRaceFinishTaskHook.OriginalFunction();
        private int RemoveAllTasksHook() => RemoveAllTasks?.Invoke(_removeAllTasksHook) ?? _removeAllTasksHook.OriginalFunction();

        public delegate void SetSpawnLocationsStartOfRaceFn(int numberOfPlayers);
        public delegate void SetupRaceFn(Task<TitleSequence, TitleSequenceTaskState>* task);
        public unsafe delegate byte SetNewPlayerStateHandlerFn(Player* player, PlayerState state, IHook<Functions.SetNewPlayerStateFn> hook);
        public unsafe delegate Task* SetRenderItemPickupTaskHandlerFn(Player* player, byte a2, ushort a3, IHook<Functions.SetRenderItemPickupTaskFn> hook);
        public unsafe delegate int SetGoalRaceFinishTaskHandlerFn(IHook<Functions.SetGoalRaceFinishTaskFn> hook, Player* player);
        public unsafe delegate int UpdateLapCounterHandlerFn(IHook<Functions.UpdateLapCounterFn> hook, Player* player, int a2);
        public delegate byte CdeclReturnByteFnFn(IHook<Functions.CdeclReturnByteFn> hook);
        public delegate int CdeclReturnIntFn(IHook<Functions.CdeclReturnIntFn> hook);
        public delegate int RunPlayerPhysicsSimulationFn(IHook<Functions.RunPlayerPhysicsSimulationFn> hook, void* somePhysicsObjectPtr, Vector4* vector, int* playerIndex);

        public delegate Enum<AsmFunctionResult> PlayerAsmFunc(Player* player);
    }
}
