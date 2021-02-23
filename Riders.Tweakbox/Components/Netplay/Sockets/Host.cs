using System;
using System.Net;
using System.Threading;
using MLAPI.Puncher.Client;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Tasks.Base;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        public override SocketType GetSocketType() => SocketType.Host;

        /// <summary>
        /// Contains that is used by the server.
        /// </summary>
        public new HostState State => (HostState) base.State;

        /// <summary>
        /// The client used for NAT Punching.
        /// </summary>
        public PuncherClient NatPunchClient;

        /// <summary>
        /// Cancellation server thread.
        /// </summary>
        private Thread _punchServerThread;

        public Host(NetplayConfig config, NetplayController controller) : base(controller, config)
        {
            var hostSettings   = config.Data.HostSettings;
            var socketSettings = hostSettings.SocketSettings;
            var punchServer    = config.Data.PunchingServer;

            Log.WriteLine($"[Host] Hosting Server on {socketSettings.Port} with password {socketSettings.Password.Text}", LogCategory.Socket);
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
            var host = (Host) obj;
            var config = host.Config;
            var punchServerSettings = config.Data.PunchingServer;

            try
            {
                Log.WriteLine($"[{nameof(Host)}] Connecting to NAT Punch Server: {punchServerSettings.Host.Text}:{punchServerSettings.Port}", LogCategory.Socket);
                host.NatPunchClient = new PuncherClient(punchServerSettings.Host.Text, (ushort) punchServerSettings.Port);

                Log.WriteLine($"[{nameof(Host)}] Listening for NAT Punches...", LogCategory.Socket);
                host.NatPunchClient.OnConnectorPunchSuccessful += OnPunchSuccessful;
                host.NatPunchClient.ListenForPunches(new IPEndPoint(IPAddress.Any, 0));
            }
            catch (Exception e)
            {
                Log.WriteLine($"[{nameof(Host)}] NAT Punch Server Failure: {e.Message}", LogCategory.Socket);
                host.NatPunchClient.Dispose();
            }
        }

        private void OnPunchSuccessful(IPEndPoint endpoint) => Log.WriteLine($"[{nameof(Host)}] Successful NAT Punch from Client: {endpoint}", LogCategory.Socket);

        /// <inheritdoc />
        public override void Dispose()
        {
            NatPunchClient?.Dispose();
            base.Dispose();
        }
    }
}
