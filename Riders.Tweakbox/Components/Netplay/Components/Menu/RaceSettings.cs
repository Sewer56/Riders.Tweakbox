using System.Collections.Generic;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
namespace Riders.Tweakbox.Components.Netplay.Components.Menu;

public unsafe class RaceSettings : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public EventController Event { get; set; }
    public MenuChangedManualEvents Delta { get; set; } = new MenuChangedManualEvents();

    /// <summary> Sync data for course select. </summary>
    private Timestamped<RuleSettingsSync> _sync;

    /// <summary> [Host Only] Changes in rule settings since the last time they were sent to the clients. </summary>
    private Queue<Timestamped<RuleSettingsLoop>> _loop = new Queue<Timestamped<RuleSettingsLoop>>(Constants.MaxNumberOfPlayers * 4);

    public RaceSettings(Socket socket, EventController @event)
    {
        Socket = socket;
        Event = @event;
        Event.OnRaceSettings += OnRaceSettings;
        Event.AfterRaceSettings += Delta.Update;
        Delta.OnRuleSettingsUpdated += OnRuleSettingsUpdated;
    }

    public void Dispose()
    {
        Event.OnRaceSettings -= OnRaceSettings;
        Event.AfterRaceSettings -= Delta.Update;
        Delta.OnRuleSettingsUpdated -= OnRuleSettingsUpdated;
    }

    private unsafe void OnRaceSettings(Task<RaceRules, RaceRulesTaskState>* task)
    {
        // For host, first set delta and then get changes from other clients, such that if net change is 0, no packet is sent.
        // For others, sync first and then set delta, so only user changes are picked up.
        if (Socket.GetSocketType() == SocketType.Host)
        {
            Delta.Set(task);
            var sync = RuleSettingsSync.FromGame(task).Merge(GetLoop());
            _sync = new Timestamped<RuleSettingsSync>(sync);
            SyncRuleSettings(task);
        }
        else
        {
            SyncRuleSettings(task);
            Delta.Set(task);
        }
    }

    private unsafe void OnRuleSettingsUpdated(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task)
    {
        if (Socket.GetSocketType() == SocketType.Host)
        {
            using var message = RuleSettingsSync.FromGame(task);
            Socket.SendToAllAndFlush(ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered);
        }
        else
        {
            loop.Undo(task);
            if (Socket.State.NumLocalPlayers > 0)
                Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(loop), DeliveryMethod.ReliableOrdered);
        }
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        switch (packet.MessageType)
        {
            case MessageType.RuleSettingsLoop:
                _loop.Enqueue(packet.GetMessage<RuleSettingsLoop>());
                break;

            case MessageType.RuleSettingsSync:
                _sync = packet.GetMessage<RuleSettingsSync>();
                break;
        }
    }

    /// <summary>
    /// Empties the rule settings queue and merges all loops into a single loop.
    /// </summary>
    private RuleSettingsLoop GetLoop()
    {
        var loop = new RuleSettingsLoop();
        while (_loop.TryDequeue(out var result))
        {
            if (result.IsDiscard(Socket.State.MaxLatency))
                continue;

            loop = loop.Add(result.Value);
        }

        return loop;
    }

    /// <summary>
    /// Synchronizes rule setting state with the currently reported state.
    /// </summary>
    private unsafe void SyncRuleSettings(Task<RaceRules, RaceRulesTaskState>* task)
    {
        if (!_sync.IsDiscard(Socket.State.MaxLatency))
            _sync.Value.ToGame(task);
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
