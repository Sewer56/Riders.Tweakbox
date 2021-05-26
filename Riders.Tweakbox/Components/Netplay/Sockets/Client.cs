using System;
using System.Net;
using MLAPI.Puncher.Client;
using MLAPI.Puncher.LiteNetLib;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public override SocketType GetSocketType() => SocketType.Client;

        public Client(NetplayEditorConfig config, NetplayController controller, TweakboxApi api) : base(controller, config, api)
        {
            if (Event.LastTask != Tasks.CourseSelect)
                throw new Exception("You are only allowed to join the host in the Course Select Menu");

            Manager.StartInManualMode(0);
            State = new CommonState(config.ToPlayerData(), this);

            // Connect to host.
            var punchSettings  = config.Data.PunchingServer;
            var clientSettings = config.Data.ClientSettings;
            var socketSettings = clientSettings.SocketSettings;

            if (punchSettings.IsEnabled)
            {
                Log.WriteLine($"[{nameof(Client)}] Connecting via NAT Punch Server: {punchSettings.Host}:{punchSettings.Port}", LogCategory.Socket);
                using PuncherClient connectPeer = new PuncherClient(punchSettings.Host, (ushort) punchSettings.Port);
                connectPeer.Transport = new LiteNetLibUdpTransport(Manager, Listener);
                connectPeer.ServerRegisterResponseTimeout = punchSettings.ServerTimeout;
                connectPeer.PunchResponseTimeout = punchSettings.PunchTimeout;

                if (connectPeer.TryPunch(IPAddress.Parse(clientSettings.IP), out IPEndPoint connectResult))
                {
                    Log.WriteLine($"[{nameof(Client)}] NAT Punch Success! Connecting {connectResult}", LogCategory.Socket);
                    Manager.Connect(connectResult, socketSettings.Password);
                }
                else
                {
                    Log.WriteLine($"[{nameof(Client)}] NAT Punch Failed, Trying Direct IP", LogCategory.Socket);
                    ConnectToIp(clientSettings.IP, socketSettings.Port, socketSettings.Password);
                }
            }
            else
            {
                ConnectToIp(clientSettings.IP, socketSettings.Port, socketSettings.Password);
            }

            Initialize();
        }

        private void ConnectToIp(string ip, int port, string password)
        {
            Log.WriteLine($"[{nameof(Client)}] Connecting via Direct IP: {ip}:{port}", LogCategory.Socket);
            Manager.Connect(ip, port, password);
        }
    }
}
