using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Components.Misc;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
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

        private FramePacingController _framePacingController;
        private Dictionary<int, bool> _hostReadyToStartRaceMap;
        private bool _clientReadyToStartRace;

        public RaceIntroSync(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            _framePacingController = IoC.Get<FramePacingController>();

            Event.OnCheckIfSkipIntro += OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro += OnRaceSkipIntro;

            if (Socket.GetSocketType() == SocketType.Host)
                _hostReadyToStartRaceMap = new Dictionary<int, bool>(8);
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
            _framePacingController.ResetSpeedup();
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
                    Socket.SendToAllExceptAndFlush(pkt.Source, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Received Skip from Client, Rebroadcasting.");
            }

            if (packet.HasSyncStartReady)
            {
                if (Socket.GetSocketType() == SocketType.Host)
                {
                    Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Received {nameof(packet.HasSyncStartReady)} from Client.", LogCategory.Race);
                    _hostReadyToStartRaceMap[pkt.Source.Id] = true;
                }
                else
                {
                    _clientReadyToStartRace = true;
                }
            }
        }

        private bool ClientTrySyncRaceSkip()
        {
            bool IsGoSignal() => _clientReadyToStartRace;

            // Send skip message to host.
            if (!_skipRequested)
                Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Skipped intro ourselves, sending skip notification to host.", LogCategory.Race);

            _skipRequested = false;
            Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { HasSyncStartReady = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Sending {nameof(ReliablePacket.HasSyncStartReady)}.", LogCategory.Race);

            if (!Socket.PollUntil(IsGoSignal, Socket.State.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(RaceIntroSync)} / Client] No Go Signal Received, Bailing Out!.", LogCategory.Race);
                Dispose();
                return false;
            }

            _clientReadyToStartRace = false;
            return true;
        }

        private bool HostTrySyncRaceSkip()
        {
            var state = (HostState)Socket.State;
            bool TestAllReady() => _hostReadyToStartRaceMap.All(x => x.Value == true);

            // Send skip signal to clients if we are initializing the skip.
            if (!_skipRequested)
                Socket.SendToAllAndFlush(new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Broadcasting Skip Signal.", LogCategory.Race);

            _skipRequested = false;
            Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Waiting for ready messages.", LogCategory.Race);

            // Note: Don't use wait for all clients, because the messages may have already been sent by the clients.
            if (!Socket.PollUntil(TestAllReady, state.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] It's no use, let's get outta here!.", LogCategory.Race);
                Dispose();
                return false;
            }

            // Disable skip flags for everyone.
            foreach (var key in _hostReadyToStartRaceMap.Keys)
                _hostReadyToStartRaceMap[key] = false;

            Socket.SendToAllAndFlush(new ReliablePacket() { HasSyncStartReady = true }, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Sending {nameof(ReliablePacket.HasSyncStartReady)}.", LogCategory.Race);
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
