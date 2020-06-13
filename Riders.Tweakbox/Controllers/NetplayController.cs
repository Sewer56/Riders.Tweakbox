using Riders.Tweakbox.Components.Netplay.Sockets;
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
        public Socket Socket;

        public NetplayController() => Event.AfterEndScene += OnEndScene;
        public void Disable() => Event.AfterEndScene -= OnEndScene;
        public void Enable() => Event.AfterEndScene += OnEndScene;

        /// <summary>
        /// True is currently connected, else false.
        /// </summary>
        public bool IsConnected() => Socket != null && Socket.IsConnected();

        /// <summary>
        /// Updates on every frame of the game.
        /// </summary>
        private void OnEndScene()
        {
            Socket?.Poll();
            Socket?.Update();
        }
    }
}
