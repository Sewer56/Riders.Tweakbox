using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNext;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Tweakbox.Api;
using Riders.Tweakbox.Components.Netplay.Components.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Utility;
namespace Riders.Tweakbox.Components.Netplay.Components.Server;

/// <summary>
/// Takes care of events such as connected/rejected and initial handshake
/// between client and host after a new connection has been established.
/// </summary>
public class ConnectionManager : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public CommonState State { get; set; }
    public HostState HostState => (HostState)State;
    public NetManager Manager { get; set; }
    public EventController Event { get; set; }
    public NetplayEditorConfig.HostSettings HostSettings { get; set; }
    public NetplayEditorConfig.ClientSettings ClientSettings { get; set; }

    private Dictionary<int, VersionInformation> _versionMap = new Dictionary<int, VersionInformation>();
    private Dictionary<int, VersionInformationEx> _versionExMap = new Dictionary<int, VersionInformationEx>();

    private VersionInformation _currentVersionInformation = new VersionInformation(Program.Version);
    private Logger _log = new Logger(LogCategory.Socket);
    private CustomGearController _customGearController = IoC.GetSingleton<CustomGearController>();
    private TweakboxApi _tweakboxApi;

    public ConnectionManager(Socket socket, EventController @event, TweakboxApi tweakboxApi)
    {
        Socket = socket;
        Event = @event;
        Manager = Socket.Manager;
        State = Socket.State;
        _tweakboxApi = tweakboxApi;

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

        _log.WriteLine($"[Host] Client {peer.EndPoint.Address} | {peer.Id}, waiting for message.");

        if (await HostValidateVersionInformation(peer))
            return;

        if (await HostValidateVersionInformationEx(peer))
            return;

        if (!await Socket.PollUntilAsync(HasReceivedPlayerData, Socket.State.DisconnectTimeout))
        {
            Socket.DisconnectWithMessage(peer, "Did not receive user data from client. (Name, Number of Players etc.)");
            return;
        }

        unsafe
        {
            _customGearController.GetCustomGearNames(out var loadedGears, out _);
            using var gameData = GameData.FromGame(loadedGears);
            using var courseSelectSync = CourseSelectSync.FromGame(Event.CourseSelect);
            Socket.SendAndFlush(peer, ReliablePacket.Create(gameData), DeliveryMethod.ReliableUnordered, "[Host] Received user data, uploading game data.", LogCategory.Socket);
            Socket.SendAndFlush(peer, ReliablePacket.Create(courseSelectSync), DeliveryMethod.ReliableUnordered, "[Host] Sending course select data for initial sync.", LogCategory.Socket);

            if (Socket.TryGetComponent(out GameModifiersSync mod))
                mod.HostSendSettingsToSinglePeer(peer);
        }
    }

    private async Task<bool> HostValidateVersionInformationEx(NetPeer peer)
    {
        bool HasReceivedVersionInformationEx() => _versionExMap.ContainsKey(peer.Id);
        if (!await Socket.PollUntilAsync(HasReceivedVersionInformationEx, Socket.State.DisconnectTimeout))
        {
            Socket.DisconnectWithMessage(peer, "Did not receive Tweakbox extended version information from client.");
            return true;
        }

        unsafe
        {
            var versionEx = GetCurrentVersionInformationEx();
            _versionExMap.Remove(peer.Id, out VersionInformationEx infoEx);
            if (!versionEx.Verify(infoEx, out string errors))
            {
                Socket.DisconnectWithMessage(peer, $"Client extended version information does not match host's.\n{errors}");
                return true;
            }
        }

        return false;
    }

    private async Task<bool> HostValidateVersionInformation(NetPeer peer)
    {
        bool HasReceivedVersionInformation() => _versionMap.ContainsKey(peer.Id);
        if (!await Socket.PollUntilAsync(HasReceivedVersionInformation, Socket.State.DisconnectTimeout))
        {
            Socket.DisconnectWithMessage(peer, "Did not receive Tweakbox version information from client.");
            return true;
        }

        _versionMap.Remove(peer.Id, out VersionInformation info);
        if (!_currentVersionInformation.Verify(info))
        {
            Socket.DisconnectWithMessage(peer, $"Client version does not match host version. Client version: {info.TweakboxVersion}, Host Version: {_currentVersionInformation.TweakboxVersion}");
            return true;
        }

        return false;
    }

    private unsafe void OnClientPeerConnected(NetPeer peer)
    {
        Socket.SendAndFlush(peer, ReliablePacket.Create(_currentVersionInformation), DeliveryMethod.ReliableOrdered, "[Client] Connected to Host, Sending Version Information", LogCategory.Socket);
        Socket.SendAndFlush(peer, ReliablePacket.Create(GetCurrentVersionInformationEx()), DeliveryMethod.ReliableOrdered, "[Client] Sending Extended Version Information", LogCategory.Socket);

        using var packet = ReliablePacket.Create(new ClientSetPlayerData() { Data = State.SelfInfo });
        Socket.SendAndFlush(peer, packet, DeliveryMethod.ReliableOrdered, "[Client] Connected to Host, Sending Player Data", LogCategory.Socket);
    }

    private void OnHostPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        HostState.ClientMap.RemovePeer(peer);
        HostUpdatePlayerMap();
    }

    private unsafe VersionInformationEx GetCurrentVersionInformationEx()
    {
        return new VersionInformationEx()
        {
            GameMode = (VersionInformationEx.RaceMode)Event.CourseSelect->TaskData->RaceMode,
            Mods = _tweakboxApi.LoadedMods.ToArray()
        };
    }

    /// <summary>
    /// Force disconnects the client/host with a given reason.
    /// </summary>
    private void ForceDisconnect(string reason)
    {
        Shell.AddDialog("Disconnected from Game", reason);
        Socket.Dispose();
    }

    private void OnClientPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        try
        {
            // According to docs I should check for DisconnectReason.RemoteConnectionClose but it seems other reasons can be assigned?.
            if (disconnectInfo.AdditionalData != null && disconnectInfo.AdditionalData.AvailableBytes > 0)
            {
                var reader = disconnectInfo.AdditionalData;
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
            _log.WriteLine(message);
            request.RejectForce();
        }

        _log.WriteLine($"[Host] Received Connection Request");
        if (Event.LastTask != Tasks.CourseSelect)
        {
            Reject("[Host] Rejected Connection | Not on Course Select");
        }
        else
        {
            _log.WriteLine($"[Host] Accepting if Password Matches");
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
        else if (packet.MessageType == MessageType.VersionEx)
        {
            _versionExMap[source.Id] = packet.GetMessage<VersionInformationEx>();
        }
    }

    private void ClientHandleReliablePacket(ref ReliablePacket packet)
    {
        if (packet.MessageType == MessageType.HostSetPlayerData)
        {
            var data = packet.GetMessage<HostSetPlayerData>();
            _log.WriteLine($"[Client] Received Player Info");
            State.SetClientInfo(data.Data.Slice(0, data.NumElements));
            State.SelfInfo.PlayerIndex = data.PlayerIndex;
            State.SelfInfo.ClientIndex = data.ClientIndex;
        }
        else if (packet.MessageType == MessageType.GameData)
        {
            var data = packet.GetMessage<GameData>();
            data.ToGame(strings =>
            {
                if (_customGearController.HasAllGears(strings, out var missingGears))
                {
                    _customGearController.Reload(strings);
                    return true;
                }
                else
                {
                    ForceDisconnect($"Client is missing custom gears being used by the host.\nGear List:\n\n{string.Join("\n", missingGears)}");
                    return false;
                }
            });
        }
    }

    private void HostUpdatePlayerMap()
    {
        // Note: Don't use separate channel because some other events e.g. Chat Messages
        //       may depend on message order.

        // Update Player Map.
        for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
        {
            var peer = Manager.ConnectedPeerList[x];
            var message = HostState.ClientMap.ToMessage(peer);
            Socket.Send(peer, ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered);
        }

        Socket.Update();
        State.SetClientInfo(HostState.ClientMap.ToMessage(State.SelfInfo.PlayerIndex, State.SelfInfo.ClientIndex).Data);
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
