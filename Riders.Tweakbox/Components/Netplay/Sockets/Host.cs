using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public class Host : ISocket
    {
        private NetManager _host;
        public Host(string ipAddress)
        {
            _host = new NetManager(new EventListener(this, false));
        }

        public void Dispose() => _host.Stop(true);
        public bool IsConnected() => _host.IsRunning;
        public bool IsHost() => false;
        public void Update() => _host.PollEvents();

        public void HandleReliablePacket(ReliablePacket packet)
        {
            
        }

        public void HandleUnreliablePacket(UnreliablePacket packet)
        {

        }
    }
}
