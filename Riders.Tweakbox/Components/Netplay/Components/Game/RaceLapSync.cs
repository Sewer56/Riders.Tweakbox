using System;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using static Sewer56.SonicRiders.API.Player;
using static Sewer56.SonicRiders.API.State;
using Constants = Riders.Netplay.Messages.Misc.Constants;
using Extensions = Riders.Tweakbox.Components.Netplay.Helpers.Extensions;
namespace Riders.Tweakbox.Components.Netplay.Components.Game;

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
            - Ignore lap increment for non-local player.
            - Apply on Receive and call lap increment ourselves.
    */

    /// <inheritdoc />
    public Socket Socket { get; set; }
    public EventController Event { get; set; }
    public CommonState State { get; set; }

    /// <summary>
    /// Called when the lap counter of a local player is updated.
    /// </summary>
    public event OnUpdateLocalLapCounter OnUpdateLocalLapCounter;

    /// <summary>
    /// Set to true if currently applying an updated lap counter.
    /// Prevents increment lap function from running outside our control.
    /// </summary>
    private bool _isSyncingLapCounter = false;

    /// <summary>
    /// [Host] Contains lap data for all the players.
    /// </summary>
    private LapCounter[] _lapSync = new LapCounter[Constants.MaxNumberOfPlayers];
    private DeliveryMethod _lapDeliveryMethod = DeliveryMethod.ReliableOrdered;

    private void* _goalRaceFinishTaskPtr;
    private bool _isRaceFinishTaskEnabled;
    private Logger _log = new Logger(LogCategory.LapSync);

    public RaceLapSync(Socket socket, EventController @event)
    {
        Socket = socket;
        Event = @event;
        State = socket.State;

        Event.SetGoalRaceFinishTask += SetGoalRaceFinishTask;
        Event.UpdateLapCounter += UpdateLapCounterTask;
        Event.GoalRaceFinishTask += GoalRaceFinishTask;

        Sewer56.SonicRiders.API.Event.OnKillTask += OnKillTask;
        Sewer56.SonicRiders.API.Event.OnKillAllTasks += RemoveAllTasks;
        Event.AfterRace += CheckIfAllFinishedRace;

        IoC.Get<MiscPatchController>().DisableRacePositionOverwrite.Enable();
        IoC.Get<EnableTimerPostRaceController>().Hook.Enable();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Event.SetGoalRaceFinishTask -= SetGoalRaceFinishTask;
        Event.UpdateLapCounter -= UpdateLapCounterTask;
        Event.GoalRaceFinishTask -= GoalRaceFinishTask;

        Sewer56.SonicRiders.API.Event.OnKillTask -= OnKillTask;
        Sewer56.SonicRiders.API.Event.OnKillAllTasks -= RemoveAllTasks;
        Event.AfterRace -= CheckIfAllFinishedRace;

        IoC.Get<MiscPatchController>().DisableRacePositionOverwrite.Disable();
        IoC.Get<EnableTimerPostRaceController>().Hook.Disable();
    }

    public void Reset()
    {
        Array.Fill(_lapSync, new Timestamped<LapCounter>());
    }

    /* Implementation */
    private unsafe int SetGoalRaceFinishTask(IHook<Functions.SetGoalRaceFinishTaskFn> hook, Player* player)
    {
        // Suppress task creation; we will decide ourselves when it's time.
        _log.WriteLine($"[{nameof(RaceLapSync)}] OnSetGoalRaceFinishTask P{GetPlayerIndex(player)}");
        return 0;
    }

    private int UpdateLapCounterTask(IHook<Functions.UpdateLapCounterFn> hook, Player* player, int a2)
    {
        // Cancel update counter if we're not the one instigating it.
        if (_isSyncingLapCounter)
            return 0;

        // Discard non-local players.
        // We will invoke underlying function manually when needed.
        var playerIndex = GetPlayerIndex(player);
        if (!State.IsLocal(playerIndex))
            return 0;

        var result = hook.OriginalFunction(player, a2);

        // Update lap counters for local clients.
        var counter = new LapCounter(player);
        _lapSync[playerIndex] = counter;
        OnUpdateLocalLapCounter?.Invoke(playerIndex, counter);
        _log.WriteLine($"[{nameof(RaceLapSync)}] Set: {playerIndex} | Lap {_lapSync[playerIndex].Counter} Timer {_lapSync[playerIndex].Timer}");

        switch (Socket.GetSocketType())
        {
            case SocketType.Host:
                HostSendLapCounters(null);
                break;

            case SocketType.Client when State.NumLocalPlayers > 0:
            {
                // Send counters of local players.
                using var counters = new LapCountersPacked();
                counters.ToPooled(State.NumLocalPlayers);
                counters.Set(_lapSync, State.NumLocalPlayers);

                Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(counters), _lapDeliveryMethod);
                break;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        // Check message type.
        if (packet.MessageType != MessageType.LapCounters)
            return;

        // Get message.
        var lapCounters = packet.GetMessage<LapCountersPacked>();

        // Index of first player to fill.
        int playerIndex = Socket.GetSocketType() switch
        {
            SocketType.Host => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
            SocketType.Client => State.NumLocalPlayers,
            _ => throw new ArgumentOutOfRangeException()
        };

        var counters   = lapCounters.Elements;
        var numCounters = lapCounters.NumElements;

        for (int x = 0; x < numCounters; x++)
            _lapSync[playerIndex + x] = counters[x];

        if (Socket.GetSocketType() == SocketType.Host)
            HostSendLapCounters(source);

        _log.WriteLine($"[{nameof(RaceLapSync)}] Applying Sync on Receive");
        ApplyLapSync();
    }

    private void HostSendLapCounters(NetPeer excludePeer)
    {
        // Upload new lap counters to everyone.
        Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers]; // Player indices to exclude.

        for (var x = 0; x < Socket.Manager.ConnectedPeerList.Count; x++)
        {
            var peer = Socket.Manager.ConnectedPeerList[x];
            if (peer == excludePeer)
                continue;

            if (!((HostState)State).ClientMap.Contains(peer))
                continue;

            var excludeIndices = Extensions.GetExcludeIndices((HostState)State, peer, excludeIndexBuffer);
            using var rental = Extensions.GetItemsWithoutIndices(_lapSync.AsSpan(0, State.GetPlayerCount()), excludeIndices);

            if (rental.Length <= 0)
                continue;

            // Transmit Packet Information
            using var counters = new LapCountersPacked();
            counters.Set(rental.Segment.Array, rental.Length);
            Socket.Send(peer, ReliablePacket.Create(counters), _lapDeliveryMethod);
        }

        Socket.Update();
    }

    private void ApplyLapSync()
    {
        _isSyncingLapCounter = true;

        // Apply sync for non-local players.
        for (int x = State.NumLocalPlayers; x < Constants.MaxRidersNumberOfPlayers; x++)
        {
            ref var lap = ref _lapSync[x];
            var player = &Players.Pointer[x];

            // Call the update lap function to increment the value, bypassing our hook code.
            var laps = lap.Counter - player->LapCounter;
            for (int y = 0; y < laps; y++)
            {
                _log.WriteLine($"[{nameof(RaceLapSync)}] Sync: Set Lap for {x} | Lap {lap.Counter} | Timer {lap.Timer}");

                // Copy stage timer (lap increment will use this value for lap time math!)
                var stageTimerBackup = *StageTimer;

                *StageTimer = lap.Timer;
                Event.InvokeUpdateLapCounter(player, *(int*)0x017E3E2C);
                *StageTimer = stageTimerBackup;

                // Restore old timer.
                player->FinishTime = lap.Timer;
                if (player->LapCounter > CurrentRaceSettings->Laps)
                {
                    // Just in case so end of race placements don't screw up.
                    // This is the same way in which the game handles it.
                    player->CheckpointProgression += 10000 >> player->RacePosition;
                }
            }

            player->LapCounter = lap.Counter;
        }

        _isSyncingLapCounter = false;
    }

    #region Results Screen Activation Handling
    private byte GoalRaceFinishTask(IHook<Functions.CdeclReturnByteFn> hook)
    {
        _isRaceFinishTaskEnabled = true;
        _goalRaceFinishTaskPtr = *CurrentTask;

        return hook.OriginalFunction();
    }

    private void OnKillTask()
    {
        if (*CurrentTask != _goalRaceFinishTaskPtr)
            return;

        _log.WriteLine($"[{nameof(RaceLapSync)}] Kill Results Task");
        _isRaceFinishTaskEnabled = false;
        _goalRaceFinishTaskPtr = (void*)-1;
        Reset();
    }

    private void RemoveAllTasks()
    {
        _log.WriteLine($"[{nameof(RaceLapSync)}] Kill All Tasks");
        _isRaceFinishTaskEnabled = false;
        _goalRaceFinishTaskPtr = (void*)-1;
        Reset();
    }

    private void CheckIfAllFinishedRace(Task<byte, RaceTaskState>* task)
    {
        if (_isRaceFinishTaskEnabled)
            return;

        // Update Lap Sync
        ApplyLapSync();

        // Set goal race finish task if all players finished racing.
        bool allFinished = true;
        var numPlayer = State.GetPlayerCount();
        for (int x = 0; x < numPlayer; x++)
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
            _log.WriteLine($"[{nameof(RaceLapSync)}] Trigger GoalRaceFinishTask");
            Reset();
            Event.InvokeSetGoalRaceFinishTask(Players.Pointer);
        }
    }
    #endregion

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}

public delegate void OnUpdateLocalLapCounter(int playerIndex, in LapCounter counter);