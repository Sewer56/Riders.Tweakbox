using System;
using System.Net;
using System.Threading;
using MLAPI.Puncher.Client;
using MLAPI.Puncher.LiteNetLib;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
namespace Riders.Tweakbox.Components.Netplay.Sockets;

public unsafe class Host : Socket
{
    public override SocketType GetSocketType() => SocketType.Host;

    /// <summary>
    /// Contains that is used by the server.
    /// </summary>
    public new HostState State => (HostState)base.State;

    /// <summary>
    /// The client used for NAT Punching.
    /// </summary>
    public PuncherClient NatPunchClient;

    /// <summary>
    /// Cancellation server thread.
    /// </summary>
    private Thread _punchServerThread;

    public Host(NetplayEditorConfig config, NetplayController controller, TweakboxApi tweakboxApi) : base(controller, config, tweakboxApi)
    {
        if (!CanJoin)
            throw new Exception("You are only allowed to start hosting in the Course Select Menu");

        var hostSettings = config.Data.HostSettings;
        var socketSettings = hostSettings.SocketSettings;
        var punchServer = config.Data.PunchingServer;

        Log.WriteLine($"[Host] Hosting Server on {socketSettings.Port} with password {socketSettings.Password}", LogCategory.Socket);
        base.State = new HostState(config.ToPlayerData(), this);
        Manager.StartInManualMode(socketSettings.Port);

        if (punchServer.IsEnabled)
        {
            _punchServerThread = new Thread(RunPunchingServer);
            _punchServerThread.Start(this);
        }

        Initialize();
    }

    private void RunPunchingServer(object? obj)
    {
        var host = (Host)obj;
        var config = host.Config;
        var punchServerSettings = config.Data.PunchingServer;

        try
        {
            Log.WriteLine($"[{nameof(Host)}] Connecting to NAT Punch Server: {punchServerSettings.Host}:{punchServerSettings.Port}", LogCategory.Socket);
            host.NatPunchClient = new PuncherClient(punchServerSettings.Host, (ushort)punchServerSettings.Port);
            host.NatPunchClient.Transport = new LiteNetLibUdpTransport(Manager, Listener);

            Log.WriteLine($"[{nameof(Host)}] Listening for NAT Punches... Port: {host.Manager.LocalPort}", LogCategory.Socket);
            host.NatPunchClient.OnConnectorPunchSuccessful += endpoint => { Log.WriteLine($"[{nameof(Host)}] Successful NAT Punch from Client: {endpoint}", LogCategory.Socket); };
            host.NatPunchClient.ListenForPunches(new IPEndPoint(IPAddress.Any, host.Manager.LocalPort));
        }
        catch (Exception e)
        {
            Log.WriteLine($"[{nameof(Host)}] NAT Punch Server Failure: {e.Message}", LogCategory.Socket);
            host.NatPunchClient.Dispose();
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        NatPunchClient?.Dispose();
        base.Dispose();
    }
}
