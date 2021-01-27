using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using static Sewer56.SonicRiders.API.Player;
using static Sewer56.SonicRiders.API.State;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    /// <summary>
    /// Attacks, box pickups, etc.
    /// </summary>
    public unsafe class RaceLapSync : INetplayComponent
    {
        /*
            NOTE TO PROGRAMMERS: 

            There's a few problems with synchronizing the lap counter at the time of writing:
                
            - Players with unstable connections can jitter a lot, which can cause them to teleport past the start line and not count a lap.
                - Therefore, we have no guarantee that OnUpdateLapCounterTask will be executed for laggy players in the first place.

            - Lap counter update can arrive either before or after OnUpdateLapCounterTask due to packet loss.
            - Lap updates cannot be immediately processed because that could cause laps to be counted twice. 
              (Increment when receive packet and increment again on function execute)
            - OnUpdateLapCounterTask controls things such as end of race time; so cannot be disabled.
              Neither can its number addition behaviour.

            Current Solution:
                - Apply on Receive
                - If Lap Counter function is executed for non-P1 within a short time period after, subtract the counter so the function can then re-add.
        */

        public const int StopwatchLapDiscardPeriod = 100;
        private static Patch _disableRacePositionOverwrite = new Patch((IntPtr) 0x4B40E6, new byte[] { 0xEB, 0x44 });
        private static IAsmHook _injectRunTimerPostRace;

        /// <inheritdoc />
        public Socket Socket            { get; set; }
        public EventController Event    { get; set; }
        public CommonState State        { get; set; }

        /// <summary>
        /// [Host] Contains lap data for all the players.
        /// </summary>
        private Timestamped<LapCounter>[] _lapSync = new Timestamped<LapCounter>[Constants.MaxNumberOfPlayers];
        private DeliveryMethod _lapDeliveryMethod = DeliveryMethod.ReliableOrdered;

        private void* _goalRaceFinishTaskPtr;
        private bool  _isTaskEnabled;
        private Stopwatch _applyTimeWatch;

        public RaceLapSync(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            State  = socket.State;

            Event.SetGoalRaceFinishTask += OnSetGoalRaceFinishTask;
            Event.UpdateLapCounter      += OnUpdateLapCounterTask;
            Event.GoalRaceFinishTask    += OnGoalRaceFinishTask;

            Sewer56.SonicRiders.API.Event.OnKillTask += OnKillTask;
            Event.AfterRace      += CheckIfAllFinishedRace;
            Event.RemoveAllTasks += RemoveAllTasks;
            _applyTimeWatch = Stopwatch.StartNew();
            _disableRacePositionOverwrite.Enable();

            if (_injectRunTimerPostRace == null)
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

                _injectRunTimerPostRace = SDK.ReloadedHooks.CreateAsmHook(runTimerPostRace, 0x004166EB).Activate();
            }

            _injectRunTimerPostRace.Enable();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.SetGoalRaceFinishTask -= OnSetGoalRaceFinishTask;
            Event.UpdateLapCounter      -= OnUpdateLapCounterTask;
            Event.GoalRaceFinishTask    -= OnGoalRaceFinishTask;

            Sewer56.SonicRiders.API.Event.OnKillTask -= OnKillTask;
            Event.AfterRace -= CheckIfAllFinishedRace;
            Event.RemoveAllTasks -= RemoveAllTasks;
            _disableRacePositionOverwrite.Disable();
            _injectRunTimerPostRace.Disable();
        }

        public void Reset()
        {
            Array.Fill(_lapSync, new Timestamped<LapCounter>());
        }

        /* Implementation */
        private unsafe int OnSetGoalRaceFinishTask(IHook<Functions.SetGoalRaceFinishTaskFn> hook, Player* player)
        {
            // Suppress task creation; we will decide ourselves when it's time.
            var playerIndex = GetPlayerIndex(player);

            Log.WriteLine($"[{nameof(RaceLapSync)}] OnSetGoalRaceFinishTask P{playerIndex}", LogCategory.LapSync);
            if (player->LapCounter > CurrentRaceSettings->Laps)
                return 0;

            return hook.OriginalFunction(player);
        }

        private int OnUpdateLapCounterTask(IHook<Functions.UpdateLapCounterFn> hook, Player* player, int a2)
        {
            var playerIndex = GetPlayerIndex(player);
            Log.WriteLine($"[{nameof(RaceLapSync)}] OnUpdateLapCounterTask P{playerIndex}", LogCategory.LapSync);
            if (playerIndex != 0 && _applyTimeWatch.Elapsed.TotalMilliseconds < StopwatchLapDiscardPeriod)
            {
                Log.WriteLine($"[{nameof(RaceLapSync)}] Decrement P{playerIndex}", LogCategory.LapSync);
                player->LapCounter -= 1;
            }

            var result = hook.OriginalFunction(player, a2);
            if (playerIndex != 0) 
                return result;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                // Update own lap counter.
                _lapSync[0] = new Timestamped<LapCounter>(new LapCounter(player->LapCounter));
                HostSendLapCounters();
            }
            else
            {
                // Send updated lap counter to host.
                Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { SetLapCounter = new LapCounter(player->LapCounter) }, _lapDeliveryMethod);
            }

            return result;
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.GetPacketKind() == PacketKind.Reliable)
                HandleReliablePacket(packet.Source, packet.As<ReliablePacket>());
        }

        private void HandleReliablePacket(NetPeer packetSource, ReliablePacket packet)
        {
            switch (Socket.GetSocketType())
            {
                case SocketType.Host:

                    if (!packet.SetLapCounter.HasValue)
                        return;

                    var state = (HostState)State;
                    var lapCounter = packet.SetLapCounter.Value;
                    var playerIndex = state.ClientMap.GetPlayerData(packetSource).PlayerIndex;
                    _lapSync[playerIndex] = new Timestamped<LapCounter>(lapCounter);

                    HostSendLapCounters();
                    ApplyLapSync();
                    break;
                case SocketType.Client:

                    if (!packet.LapCounters.HasValue)
                        return;

                    var counters = packet.LapCounters.Value.AsInterface();
                    for (int x = 1; x < _lapSync.Length; x++)
                        _lapSync[x] = new Timestamped<LapCounter>(counters.Elements[x - 1]);

                    ApplyLapSync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HostSendLapCounters()
        {
            // Upload new lap counters to everyone.
            var hostState = (HostState)State;
            for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
            {
                var peer            = Socket.Manager.ConnectedPeerList[x];
                var excludeIndex    = hostState.ClientMap.GetPlayerData(peer).PlayerIndex;
                var laps            = _lapSync.Where((loop, y) => y != excludeIndex).Select(y => y.Value).ToArray();
                Socket.Send(peer, new ReliablePacket() { LapCounters = new LapCounters().AsInterface().Create(laps) }, _lapDeliveryMethod);
            }

            Socket.Update();
        }

        private void ApplyLapSync()
        {
            for (int x = 1; x < _lapSync.Length; x++)
            {
                var timestampedSync = _lapSync[x];
                if (timestampedSync.IsDiscard(State.MaxLatency))
                    continue;

                var player = &Players.Pointer[x];
                var counter = timestampedSync.Value.Counter;

                if (counter <= player->LapCounter) 
                    continue;

                // Call the update lap function to increment the value, bypassing our hook code.
                var laps = counter - player->LapCounter;
                for (int y = 0; y < laps; y++)
                {
                    Log.WriteLine($"[{nameof(RaceLapSync)}] Sync: Increment Lap for {x}", LogCategory.LapSync);
                    Event.InvokeUpdateLapCounterHook(player, *(int*)0x017E3E2C);
                    
                    // The code to set the finish time is omitted if this packet is processed before the player touches the line.
                    // In which case you get an autogenerated time in the results screen.
                    // As such, just in case, we set the time ourselves.
                    if (player->LapCounter > CurrentRaceSettings->Laps)
                    {
                        player->FinishTime = *StageTimer;

                        // Just in case so end of race placements don't screw up.
                        // This is the same way in which the game handles it.
                        player->CheckpointProgression += 10000 >> player->RacePosition;
                    }
                }
            }

            Log.WriteLine($"[{nameof(RaceLapSync)}] Applied Sync", LogCategory.LapSync);
            _applyTimeWatch.Restart();
        }

        #region Results Screen Activation Handling
        private byte OnGoalRaceFinishTask(IHook<Functions.DefaultTaskFnWithReturn> hook)
        {
            _isTaskEnabled = true;
            _goalRaceFinishTaskPtr = *CurrentTask;

            return hook.OriginalFunction();
        }

        private void OnKillTask()
        {
            if (*CurrentTask == _goalRaceFinishTaskPtr)
            {
                Log.WriteLine($"[{nameof(RaceLapSync)}] Kill Results Task", LogCategory.LapSync);
                _isTaskEnabled = false;
                _goalRaceFinishTaskPtr = (void*)-1;
                Reset();
            }
        }

        private int RemoveAllTasks(IHook<Functions.DefaultReturnFn> hook)
        {
            Log.WriteLine($"[{nameof(RaceLapSync)}] Kill All Tasks", LogCategory.LapSync);
            _isTaskEnabled = false;
            _goalRaceFinishTaskPtr = (void*)-1;
            Reset();

            return hook.OriginalFunction();
        }

        private void CheckIfAllFinishedRace(Task<byte, RaceTaskState>* task)
        {
            if (!_isTaskEnabled)
            {
                // Set goal race finish task if all players finished racing.
                bool allFinished = true;
                for (int x = 0; x < Constants.MaxNumberOfPlayers; x++)
                {
                    if (!State.IsHuman(x))
                        continue;

                    ref var player = ref Players[x];
                    if (player.LapCounter > CurrentRaceSettings->Laps)
                        continue;

                    allFinished = false;
                    break;
                }

                // TODO: Local Multiplayer support.
                if (allFinished)
                {
                    Log.WriteLine($"[{nameof(RaceLapSync)}] Trigger GoalRaceFinishTask", LogCategory.LapSync);
                    Event.InvokeSetGoalRaceFinishTask(Players.Pointer);
                }
            }
        }
        #endregion
    }
}
