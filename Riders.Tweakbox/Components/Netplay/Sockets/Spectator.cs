using Riders.Tweakbox.Controllers;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public class Spectator : Client
    {
        /// <inheritdoc />
        public Spectator(NetplayConfig config, NetplayController controller) : base(config, controller) { }

        /// <inheritdoc />
        public override SocketType GetSocketType() => SocketType.Spectator;
    }
}
