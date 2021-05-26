using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Sewer56.Imgui.Shell;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    public class DisconnectOnCourseLeave : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }

        public DisconnectOnCourseLeave(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;
            Event.OnExitCourseSelect += OnExitCourseSelect;
        }

        /// <inheritdoc />
        public void Dispose() => Event.OnExitCourseSelect -= OnExitCourseSelect;

        private void OnExitCourseSelect()
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                Shell.AddDialog("Lobby Ended", "You left Course Select. Lobby has ended.");
                Socket.DisconnectAllWithMessage("Host has ended lobby.");
            }
            else
            {
                Shell.AddDialog("Disconnected.", "You have been automatically disconnected for leaving Course Select.");
            }


            Socket.Dispose();
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
