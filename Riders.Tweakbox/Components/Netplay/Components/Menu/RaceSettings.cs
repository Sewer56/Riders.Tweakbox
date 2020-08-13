using System;
using System.Collections.Concurrent;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Components.Menu
{
    public unsafe class RaceSettings : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public MenuChangedManualEvents Delta { get; set; } = new MenuChangedManualEvents();

        /// <summary> Sync data for course select. </summary>
        private Volatile<Timestamped<RuleSettingsSync>> _sync = new Volatile<Timestamped<RuleSettingsSync>>(new Timestamped<RuleSettingsSync>());

        /// <summary> [Host Only] Changes in rule settings since the last time they were sent to the clients. </summary>
        private ConcurrentQueue<Timestamped<RuleSettingsLoop>> _loop => _queue.Get<Timestamped<RuleSettingsLoop>>();
        private MessageQueue _queue = new MessageQueue();

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
                _sync = new Volatile<Timestamped<RuleSettingsSync>>(sync);
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
                Socket.SendToAllAndFlush(new ReliablePacket(RuleSettingsSync.FromGame(task)), DeliveryMethod.ReliableOrdered);
            else
            {
                loop.Undo(task);
                if (Socket.GetSocketType() == SocketType.Client) // Spectator gets no input.
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket(loop), DeliveryMethod.ReliableOrdered);
            }
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (!packet.TryGetPacket<ReliablePacket>(Socket.State.MaxLatency, out var reliable))
                return;

            var command = reliable.MenuSynchronizationCommand;
            if (!command.HasValue)
                return;

            switch (command.Value.Command)
            {
                case RuleSettingsLoop ruleSettingsLoop:
                    _loop.Enqueue(ruleSettingsLoop);
                    break;
                case RuleSettingsSync ruleSettingsSync:
                    _sync = new Volatile<Timestamped<RuleSettingsSync>>(ruleSettingsSync);
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
            if (!_sync.HasValue)
                return;

            var sync = _sync.Get();
            if (!sync.IsDiscard(Socket.State.MaxLatency))
                sync.Value.ToGame(task);
        }
    }
}
