using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers.Structures;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Owned by <see cref="Netplay"/>
    /// </summary>
    public class NetplayController
    {
        /// <summary>
        /// The current socket instance, either a <see cref="Client"/> or <see cref="Host"/>.
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// Events assigned to this controller.
        /// </summary>
        public NetplayControllerEvents Events { get; private set; } = new NetplayControllerEvents();

        public NetplayController() => Event.AfterSleep += Update;
        public void Disable() => Event.AfterSleep -= Update;
        public void Enable() => Event.AfterSleep += Update;

        /// <summary>
        /// True is currently connected, else false.
        /// </summary>
        public bool IsConnected() => Socket != null && Socket.IsConnected();

        /// <summary>
        /// Updates on every frame of the game.
        /// </summary>
        private void Update()
        {
            Socket?.Poll();
            Socket?.Update();
        }
    }
}
