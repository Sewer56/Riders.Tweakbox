using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers.Interfaces;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Controllers
{
    /// <summary>
    /// Owned by <see cref="Netplay"/>
    /// </summary>
    public class NetplayController : IController
    {
        /// <summary>
        /// The current socket instance, either a <see cref="Client"/> or <see cref="Host"/>.
        /// </summary>
        public Socket Socket;

        public NetplayController() => Event.AfterEndScene += OnEndScene;

        /// <summary>
        /// True is currently connected, else false.
        /// </summary>
        public bool IsConnected() => Socket != null && Socket.IsConnected();

        /// <summary>
        /// Updates on every frame of the game.
        /// </summary>
        private void OnEndScene()
        {
            if (Socket != null)
            {
                Socket.Update();
                Socket.OnFrame();
            }
        }
    }
}
