using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class RaceIntroSync : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }

        /// <summary>
        /// True if a skip has been requested by host.
        /// </summary>
        private bool _skipRequested = false;

        /// <summary>
        /// [Client] Gets the go command from the host for syncing start time.
        /// </summary>
        private Volatile<SyncStartGo> _startSyncGo = new Volatile<SyncStartGo>();

        public RaceIntroSync(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;

            Event.OnCheckIfSkipIntro += OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro += OnRaceSkipIntro;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.OnCheckIfSkipIntro -= OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro -= OnRaceSkipIntro;
        }

        private Enum<AsmFunctionResult> OnCheckIfRaceSkipIntro() => _skipRequested;
        private void OnRaceSkipIntro()
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (!HostTrySyncRaceSkip()) 
                    return;
            }
            else
            {
                if (!ClientTrySyncRaceSkip()) 
                    return;
            }

            OnIntroCutsceneEnd();
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> pkt)
        {
            if (pkt.GetPacketKind() != PacketKind.Reliable)
                return;

            var packet = pkt.As<ReliablePacket>();
            if (packet.HasSyncStartSkip)
            {
                _skipRequested = true;
                if (Socket.GetSocketType() == SocketType.Host)
                    Socket.SendToAllExcept(pkt.Source, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Received Skip from Client, Rebroadcasting.");
            }

            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (packet.HasSyncStartReady)
                {
                    var state = (HostState)Socket.State;
                    Trace.WriteLine($"[{nameof(RaceIntroSync)} / Host] Received {nameof(packet.HasSyncStartReady)} from Client.");
                    var playerCustomData = state.ClientMap.GetCustomData(pkt.Source);
                    playerCustomData.ReadyToStartRace = true;
                }
            }
            else
            {
                if (packet.SyncStartGo.HasValue)
                {
                    Trace.WriteLine($"[{nameof(RaceIntroSync)} / Client] Set {nameof(_startSyncGo)}");
                    _startSyncGo = packet.SyncStartGo.Value;
                }
            }
        }

        private bool ClientTrySyncRaceSkip()
        {
            SyncStartGo goMessage = default;

            bool IsGoSignal()
            {
                goMessage = _startSyncGo.Get();
                return !goMessage.IsDefault();
            }

            // Send skip message to host.
            if (!_skipRequested)
                Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Skipped intro ourselves, sending skip notification to host.");

            _skipRequested = false;
            Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { HasSyncStartReady = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Sending {nameof(ReliablePacket.HasSyncStartReady)}.");

            if (!Socket.PollUntil(IsGoSignal, Socket.State.HandshakeTimeout))
            {
                Trace.WriteLine($"[{nameof(RaceIntroSync)} / Client] No Go Signal Received, Bailing Out!.");
                Dispose();
                return false;
            }

            Socket.WaitWithSpin(goMessage.StartTime);
            Trace.WriteLine($"[{nameof(RaceIntroSync)} / Client] Race Started.");
            return true;
        }

        private bool HostTrySyncRaceSkip()
        {
            var state = (HostState)Socket.State;
            bool TestAllReady() => state.ClientMap.GetCustomData().All(x => x.ReadyToStartRace);

            // Send skip signal to clients if we are initializing the skip.
            if (!_skipRequested)
                Socket.SendToAllAndFlush(new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Broadcasting Skip Signal.");

            _skipRequested = false;
            Trace.WriteLine($"[{nameof(RaceIntroSync)} / Host] Waiting for ready messages.");

            // Note: Don't use wait for all clients, because the messages may have already been sent by the clients.
            if (!Socket.PollUntil(TestAllReady, state.HandshakeTimeout))
            {
                Trace.WriteLine($"[{nameof(RaceIntroSync)} / Host] It's no use, let's get outta here!.");
                Dispose();
                return false;
            }

            var startTime = new SyncStartGo(state.MaxLatency);
            Socket.SendToAllAndFlush(new ReliablePacket() { SyncStartGo = startTime }, DeliveryMethod.ReliableOrdered, "[Host] Sending Race Start Signal.");

            // Disable skip flags for everyone.
            var data = state.ClientMap.GetCustomData();
            foreach (var dt in data)
                dt.ReadyToStartRace = false;

            Socket.WaitWithSpin(startTime.StartTime, $"[{nameof(RaceIntroSync)} / Host] Race Started.");
            return true;
        }

        /// <summary>
        /// Sets all players to CPU/Human post race start trigger.
        /// </summary>
        private unsafe void OnIntroCutsceneEnd()
        {
            for (int x = 1; x < Player.MaxNumberOfPlayers; x++)
            {
                Player.Players[x].IsAiLogic = PlayerType.CPU;
                Player.Players[x].IsAiVisual = PlayerType.CPU;
            }
        }
    }
}
