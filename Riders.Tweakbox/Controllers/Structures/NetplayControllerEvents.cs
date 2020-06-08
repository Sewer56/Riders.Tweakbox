namespace Riders.Tweakbox.Controllers.Structures
{
    public class NetplayControllerEvents
    {
        /// <summary>
        /// Called when a new player connects to the server.
        /// </summary>
        public ClientConnectionChanged ClientConnected { get; set; }

        /// <summary>
        /// Called when a player disconnects from the server.
        /// </summary>
        public ClientConnectionChanged ClientDisconnected { get; set; }

        /// <summary>
        /// Called when a player changes their name.
        /// </summary>
        public ClientNameChanged NameChanged { get; set; }

        #region Delegates
        public delegate void ClientConnectionChanged(string clientName);
        public delegate void ClientNameChanged(int clientId, string clientName);
        #endregion
    }
}
