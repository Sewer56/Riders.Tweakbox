using Riders.Tweakbox.Controllers;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public class Spectator : Client
    {
        /// <inheritdoc />
        public Spectator(string ipAddress, int port, string password, NetplayController controller) : base(ipAddress, port, password, controller) { }

        /// <inheritdoc />
        public override SocketType GetSocketType() => SocketType.Spectator;
    }
}
