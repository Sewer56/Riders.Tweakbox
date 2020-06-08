using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public class Client : ISocket
    {
        private NetManager _client;
        public Client(string ipAddress)
        {
            _client = new NetManager(new EventListener(this, true));
        }

        public void Dispose() => _client.Stop(true);
        public bool IsConnected() => _client.IsRunning;
        public bool IsHost() => false;
        public void Update() => _client.PollEvents();

        public void HandleReliablePacket(ReliablePacket packet)
        {
            
        }

        public void HandleUnreliablePacket(UnreliablePacket packet)
        {

        }
    }
}
