using System;
using System.Net;
using MLAPI.Puncher.Client;
using MLAPI.Puncher.LiteNetLib;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc.Log;

namespace Riders.Tweakbox.Components.Netplay.Sockets;

/// <inheritdoc />
public class Client : Socket
{
    public override SocketType GetSocketType() => SocketType.Client;

    public Client(NetplayEditorConfig config, NetplayController controller, TweakboxApi api) : base(controller, config, api)
    {
        if (!CanJoin)
            throw new Exception("You are only allowed to join the host in the Course Select Menu");

        Manager.StartInManualMode(0);
        State = new CommonState(config.ToPlayerData(), this);

        // Connect to host.
        var punchSettings = config.Data.PunchingServer;
        var clientSettings = config.Data.ClientSettings;
        var socketSettings = clientSettings.SocketSettings;

        if (punchSettings.IsEnabled)
        {
            _log.WriteLine($"[{nameof(Client)}] Connecting via NAT Punch Server: {punchSettings.Host}:{punchSettings.Port}");
            using PuncherClient connectPeer = new PuncherClient(punchSettings.Host, (ushort)punchSettings.Port);
            connectPeer.Transport = new LiteNetLibUdpTransport(Manager, Listener);
            connectPeer.ServerRegisterResponseTimeout = punchSettings.ServerTimeout;
            connectPeer.PunchResponseTimeout = punchSettings.PunchTimeout;

            if (connectPeer.TryPunch(IPAddress.Parse(clientSettings.IP), out IPEndPoint connectResult))
            {
                _log.WriteLine($"[{nameof(Client)}] NAT Punch Success! Connecting {connectResult}");
                Manager.Connect(connectResult, socketSettings.Password);
            }
            else
            {
                _log.WriteLine($"[{nameof(Client)}] NAT Punch Failed, Trying Direct IP");
                ConnectToIp(clientSettings.IP, socketSettings.Port, socketSettings.Password);
            }
        }
        else
        {
            ConnectToIp(clientSettings.IP, socketSettings.Port, socketSettings.Password);
        }

        HostIp = clientSettings.IP;
        Initialize();
    }

    private void ConnectToIp(string ip, int port, string password)
    {
        _log.WriteLine($"[{nameof(Client)}] Connecting via Direct IP: {ip}:{port}");
        Manager.Connect(ip, port, password);
    }
}
