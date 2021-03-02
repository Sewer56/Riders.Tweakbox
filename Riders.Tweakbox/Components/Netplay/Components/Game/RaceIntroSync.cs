using System;
using System.Collections.Generic;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using StructLinq;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class RaceIntroSync : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public CommonState State { get; private set; }

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
            State  = Socket.State;
            _framePacingController = IoC.Get<FramePacingController>();

            Event.OnCheckIfSkipIntro += OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro += OnRaceSkipIntro;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                _hostReadyToStartRaceMap = new Dictionary<int, bool>(8);
                Socket.Listener.PeerConnectedEvent += OnPeerConnected;
                Socket.Listener.PeerDisconnectedEvent += OnPeerDisconnected;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.OnCheckIfSkipIntro -= OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro -= OnRaceSkipIntro;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                Socket.Listener.PeerConnectedEvent -= OnPeerConnected;
                Socket.Listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Peer Connected, Adding Entry.", LogCategory.Random);
            _hostReadyToStartRaceMap[peer.Id] = false;
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Peer Disconnected, Removing Entry.", LogCategory.Random);
            _hostReadyToStartRaceMap.Remove(peer.Id);
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
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            if (packet.MessageType != MessageType.StartSync)
                return;

            var message = packet.GetMessage<StartSync>();
            switch (message.SyncType)
            {
                case StartSyncType.Skip:
                {
                    _skipRequested = true;
                    if (Socket.GetSocketType() != SocketType.Host)
                        return;

                    // Re-broadcast skip message to clients.
                    using var reliable = ReliablePacket.Create(new StartSync() { SyncType = StartSyncType.Skip });
                    Socket.SendToAllExceptAndFlush(source, reliable, DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Received Skip from Client, Rebroadcasting.", LogCategory.Race);
                    break;
                }

                case StartSyncType.Ready when Socket.GetSocketType() == SocketType.Host:
                    Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Received {nameof(StartSyncType.Ready)} from Client.", LogCategory.Race);
                    _hostReadyToStartRaceMap[source.Id] = true;
                    break;

                case StartSyncType.Ready:
                    _clientReadyToStartRace = true;
                    break;

                case StartSyncType.Null:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool ClientTrySyncRaceSkip()
        {
            bool IsGoSignal() => _clientReadyToStartRace;

            // Get packet
            var message = new StartSync() { SyncType = StartSyncType.Skip };

            // Send skip message to host.
            if (!_skipRequested)
                Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Skipped intro ourselves, sending skip notification to host.", LogCategory.Race);

            _skipRequested = false;
            message.SyncType = StartSyncType.Ready;
            Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Client] Sending {nameof(StartSyncType.Ready)}.", LogCategory.Race);

            if (!Socket.PollUntil(IsGoSignal, Socket.State.DisconnectTimeout))
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
            bool TestAllReady() => _hostReadyToStartRaceMap.ToStructEnumerable().All(x => x.Value == true, x => x);

            // Get packet
            var message = new StartSync() { SyncType = StartSyncType.Skip };

            // Send skip signal to clients if we are initializing the skip.
            if (!_skipRequested)
                Socket.SendToAllAndFlush(ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Broadcasting Skip Signal.", LogCategory.Race);

            _skipRequested = false;
            Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] Waiting for ready messages.", LogCategory.Race);

            // Note: Don't use wait for all clients, because the messages may have already been sent by the clients.
            if (!Socket.PollUntil(TestAllReady, state.DisconnectTimeout))
            {
                Log.WriteLine($"[{nameof(RaceIntroSync)} / Host] It's no use, let's get outta here!.", LogCategory.Race);
                Dispose();
                return false;
            }

            // Disable skip flags for everyone.
            foreach (var key in _hostReadyToStartRaceMap.Keys)
                _hostReadyToStartRaceMap[key] = false;

            message.SyncType = StartSyncType.Ready;
            Socket.SendToAllAndFlush(ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered, $"[{nameof(RaceIntroSync)} / Host] Sending {nameof(StartSyncType.Ready)}.", LogCategory.Race);
            return true;
        }

        /// <summary>
        /// Sets all players to CPU/Human post race start trigger.
        /// </summary>
        private unsafe void OnIntroCutsceneEnd()
        {
            for (int x = State.NumLocalPlayers; x < Constants.MaxRidersNumberOfPlayers; x++)
            {
                Player.Players[x].IsAiLogic = PlayerType.CPU;
                Player.Players[x].IsAiVisual = PlayerType.CPU;
            }

            // Show visual elements for all players shown on screen.
            for (int x = 0; x < *Sewer56.SonicRiders.API.State.NumberOfCameras; x++)
            {
                Player.Players[x].IsAiVisual = PlayerType.Human;
            }
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
