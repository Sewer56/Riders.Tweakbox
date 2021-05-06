using System.Net;
using LiteNetLib;
using MLAPI.Puncher.LiteNetLib;

namespace MLAPI.Puncher.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            PuncherServer server = new PuncherServer();
            var listener = new EventBasedNetListener();
            var manager  = new NetManager(listener);
            manager.UnsyncedEvents = true;

            server.Transport = new LiteNetLibUdpTransport(manager, listener);
            server.Start(new IPEndPoint(IPAddress.Any, 6776));
        }
    }
}
