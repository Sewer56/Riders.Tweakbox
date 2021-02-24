using System;
using System.Net;
using MLAPI.Puncher.Client;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public override SocketType GetSocketType() => SocketType.Client;

        public Client(NetplayConfig config, NetplayController controller) : base(controller, config)
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
                Log.WriteLine($"[{nameof(Client)}] Connecting via NAT Punch Server: {punchSettings.Host.Text}:{punchSettings.Port}", LogCategory.Socket);
                using PuncherClient connectPeer = new PuncherClient(punchSettings.Host.Text, (ushort) punchSettings.Port);
                
                if (connectPeer.TryPunch(IPAddress.Parse(clientSettings.IP.Text), out IPEndPoint connectResult))
                {
                    Log.WriteLine($"[{nameof(Client)}] NAT Punch Success! Connecting {connectResult}", LogCategory.Socket);
                    Manager.Connect(connectResult, socketSettings.Password.Text);
                }
                else
                {
                    Log.WriteLine($"[{nameof(Client)}] NAT Punch Failed, Trying Direct IP", LogCategory.Socket);
                    ConnectToIp(clientSettings.IP.Text, socketSettings.Port, socketSettings.Password.Text);
                }
            }
            else
            {
                ConnectToIp(clientSettings.IP.Text, socketSettings.Port, socketSettings.Password.Text);
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
