using System;
using System.Collections.Generic;
using DotNext;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Components.Server
{
    /// <summary>
    /// Takes care of events such as connected/rejected and initial handshake
    /// between client and host after a new connection has been established.
    /// </summary>
    public class ConnectionManager : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public CommonState State { get; set; }
        public HostState HostState => (HostState) State;
        public NetManager Manager { get; set; }
        public EventController Event { get; set; }
        public NetplayConfig.HostSettings HostSettings { get; set; }
        public NetplayConfig.ClientSettings ClientSettings { get; set; }

        private Dictionary<int, VersionInformation> _versionMap = new Dictionary<int, VersionInformation>();
        private VersionInformation _currentVersionInformation = new VersionInformation(typeof(Program).Assembly.GetName().Version.ToString());

        public ConnectionManager(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;
            Manager = Socket.Manager;
            State = Socket.State;

            HostSettings = Socket.Config.Data.HostSettings;
            ClientSettings = Socket.Config.Data.ClientSettings;

            Socket.Listener.PeerConnectedEvent += OnPeerConnected;
            Socket.Listener.PeerDisconnectedEvent += OnPeerDisconnected;
            Socket.Listener.ConnectionRequestEvent += OnConnectionRequest;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Socket.Listener.PeerConnectedEvent -= OnPeerConnected;
            Socket.Listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            Socket.Listener.ConnectionRequestEvent -= OnConnectionRequest;
        }

        private async void OnHostPeerConnected(NetPeer peer)
        {
            bool HasReceivedPlayerData() => HostState.ClientMap.TryGetPlayerData(peer, out _);
            bool HasReceivedVersionInformation() => _versionMap.ContainsKey(peer.Id);

            Log.WriteLine($"[Host] Client {peer.EndPoint.Address} | {peer.Id}, waiting for message.", LogCategory.Socket);

            if (!await Socket.PollUntilAsync(HasReceivedVersionInformation, Socket.State.DisconnectTimeout))
            {
                Socket.DisconnectWithMessage(peer, "Did not receive Tweakbox version information from client.");
                return;
            }

            _versionMap.Remove(peer.Id, out VersionInformation info);
            if (!info.Verify(_currentVersionInformation))
            {
                Socket.DisconnectWithMessage(peer, $"Client version does not match host version. Client version: {info.TweakboxVersion}, Host Version: {_currentVersionInformation.TweakboxVersion}");
                return;
            }

            if (!await Socket.PollUntilAsync(HasReceivedPlayerData, Socket.State.DisconnectTimeout))
            {
                Socket.DisconnectWithMessage(peer, "Did not receive user data from client. (Name, Number of Players etc.)");
                return;
            }

            unsafe
            {
                using var gameData = GameData.FromGame();
                using var courseSelectSync = CourseSelectSync.FromGame(Event.CourseSelect);
                Socket.SendAndFlush(peer, ReliablePacket.Create(gameData), DeliveryMethod.ReliableUnordered, "[Host] Received user data, uploading game data.", LogCategory.Socket);
                Socket.SendAndFlush(peer, ReliablePacket.Create(courseSelectSync), DeliveryMethod.ReliableUnordered, "[Host] Sending course select data for initial sync.", LogCategory.Socket);
            }
        }

        private void OnClientPeerConnected(NetPeer peer)
        {
            Socket.SendAndFlush(peer, ReliablePacket.Create(_currentVersionInformation), DeliveryMethod.ReliableOrdered, "[Client] Connected to Host, Sending Version Information", LogCategory.Socket);

            using var packet = ReliablePacket.Create(new ClientSetPlayerData() { Data = State.SelfInfo });
            Socket.SendAndFlush(peer, packet, DeliveryMethod.ReliableOrdered, "[Client] Connected to Host, Sending Player Data", LogCategory.Socket);
        }

        private void OnHostPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            HostState.ClientMap.RemovePeer(peer);
            HostUpdatePlayerMap();
        }

        private void OnClientPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            try
            {
                // According to docs I should check for DisconnectReason.RemoteConnectionClose but it seems other reasons can be assigned?.
                if (disconnectInfo.AdditionalData != null && disconnectInfo.AdditionalData.AvailableBytes > 0)
                {
                    var reader   = disconnectInfo.AdditionalData;
                    var rawBytes = reader.RawData.AsSpan(reader.UserDataOffset, reader.UserDataSize);
                    using var reliable = new ReliablePacket();
                    reliable.Deserialize(rawBytes);
                    Shell.AddDialog("Disconnected from Host", reliable.GetMessage<Disconnect>().Reason);
                }
                else
                {
                    Shell.AddDialog("Disconnected from Host", "Disconnected for unknown reason. Most likely, your internet connection dropped.");
                }
            }
            finally
            {
                Socket.Dispose();
            }
        }

        private void OnHostConnectionRequest(ConnectionRequest request)
        {
            void Reject(string message)
            {
                Log.WriteLine(message, LogCategory.Socket);
                request.RejectForce();
            }

            Log.WriteLine($"[Host] Received Connection Request", LogCategory.Socket);
            if (Event.LastTask != Tasks.CourseSelect)
            {
                Reject("[Host] Rejected Connection | Not on Course Select");
            }
            else
            {
                Log.WriteLine($"[Host] Accepting if Password Matches", LogCategory.Socket);
                request.AcceptIfKey(HostSettings.Password);
            }
        }

        private void HostHandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            if (packet.MessageType == MessageType.ClientSetPlayerData)
            {
                var data = packet.GetMessage<ClientSetPlayerData>();
                if (!HostState.ClientMap.TryAddOrUpdatePeer(source, data.Data, out string failReason))
                {
                    Socket.DisconnectWithMessage(source, $"Failed to add client to session: {failReason}");
                    return;
                }

                HostUpdatePlayerMap();
            }
            else if (packet.MessageType == MessageType.Version)
            {
                _versionMap[source.Id] = packet.GetMessage<VersionInformation>();
            }
        }

        private void ClientHandleReliablePacket(ref ReliablePacket packet)
        {
            if (packet.MessageType == MessageType.HostSetPlayerData)
            {
                var data = packet.GetMessage<HostSetPlayerData>();
                Log.WriteLine($"[Client] Received Player Info", LogCategory.Socket);
                State.PlayerInfo = data.Data.Slice(0, data.NumElements);
                State.SelfInfo.PlayerIndex = data.Index;
            }
            else if (packet.MessageType == MessageType.GameData)
            {
                var data = packet.GetMessage<GameData>();
                data.ToGame();
            }
        }

        private void HostUpdatePlayerMap()
        {
            // Update Player Map.
            for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
            {
                var peer = Manager.ConnectedPeerList[x];
                var message = HostState.ClientMap.ToMessage(peer);
                Socket.Send(peer, ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered);
            }

            Socket.Update();
            State.PlayerInfo = HostState.ClientMap.ToMessage(State.SelfInfo.PlayerIndex).Data;
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                HostHandleReliablePacket(ref packet, source);
            else
                ClientHandleReliablePacket(ref packet);
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

        #region Socket Event Dispatchers
        private void OnPeerConnected(NetPeer peer)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                OnHostPeerConnected(peer);
            else
                OnClientPeerConnected(peer);
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                OnHostConnectionRequest(request);
            else
                request.AcceptIfKey(ClientSettings.Password);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                OnHostPeerDisconnected(peer, disconnectInfo);
            else
                OnClientPeerDisconnected(peer, disconnectInfo);
        }
        #endregion

    }
}