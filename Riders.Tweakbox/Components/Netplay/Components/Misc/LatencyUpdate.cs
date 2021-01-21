using System;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    public class LatencyUpdate : INetplayComponent
    {
        private const int UpdateIntervalMs = 1000;

        /// <inheritdoc />
        public Socket Socket { get; set; }
        public NetManager Manager { get; set; }
        private Timer _synchronizeTimer;
        
        public LatencyUpdate(Socket socket)
        {
            Socket  = socket;
            Manager = socket.Manager;

            Socket.Listener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdate;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                _synchronizeTimer = new Timer(UpdateClientLatencies, null, 0, UpdateIntervalMs);
            }
        }

        private void UpdateClientLatencies(object? _)
        {
            try
            {
                var state = (HostState)Socket.State;
                var clientMap = state.ClientMap;
                for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
                {
                    var peer         = Manager.ConnectedPeerList[x];
                    var hostIndex    = 0;
                    var excludeIndex = clientMap.GetPlayerData(peer).PlayerIndex;
                    var latencies    = clientMap.GetPlayerData().Where(x => x.PlayerIndex != excludeIndex && x.PlayerIndex != hostIndex).Select(x => (short) x.Latency);
                    Socket.Send(peer, new ReliablePacket(new HostUpdateLatency(latencies.ToArray())), DeliveryMethod.ReliableOrdered);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine($"[{nameof(LatencyUpdate)} / Host] Failed to send updated client latency. {e.Message} | {e.StackTrace}", LogCategory.Socket);
            }
        }

        /// <inheritdoc />
        public void Dispose() => _synchronizeTimer?.Dispose();

        private void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                // Update latency of client.
                var hostState = (HostState) Socket.State;
                var data = hostState.ClientMap.GetPlayerData(peer);
                if (data != null)
                    data.Latency = latency;
            }
            else
            {
                Socket.State.PlayerInfo[0].Latency = latency;
            }
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> pkt)
        {
            if (!pkt.TryGetPacket(Socket.State.MaxLatency, out ReliablePacket packet))
                return;

            if (Socket.GetSocketType() == SocketType.Host) 
                return;

            if (!packet.ServerMessage.HasValue)
                return;

            var serverMessage = packet.ServerMessage.Value;
            switch (serverMessage.Message)
            {
                // Latency to host handled in socket code!
                case HostUpdateLatency hostUpdateLatency:
                    try
                    {
                        for (int x = 0; x < hostUpdateLatency.Data.Length; x++)
                            Socket.State.PlayerInfo[x + 1].Latency = hostUpdateLatency.Data[x];
                    }
                    catch (Exception e)
                    {
                        Log.WriteLine($"[{nameof(LatencyUpdate)}] Failed to update client latency. Index out of bounds? {e.Message} | {e.StackTrace}", LogCategory.Socket);
                    }
                    break;
            }
        }
    }
}
