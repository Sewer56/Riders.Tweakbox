namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <summary>
    /// The type assigned to the current socket.
    /// </summary>
    public enum SocketType
    {
        Host,
        Client,

        /// <summary>
        /// Replace Player 1 with Host. Other players with rest.
        /// </summary>
        Spectator
    }
}