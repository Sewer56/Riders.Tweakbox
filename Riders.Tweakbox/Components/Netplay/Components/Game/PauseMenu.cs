using DearImguiSharp;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Functions;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class PauseMenu : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        internal PauseDialog Dialog { get; private set; }

        public PauseMenu(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            Dialog = new PauseDialog()
            {
                IsCompleted = true,
                Owner = this
            };

            Event.PauseGame += PauseGame;
        }

        private int PauseGame(int a1, int a2, byte a3, IHook<Functions.PauseGameFn> hook)
        {
            // To handle our "pause", we add a menu task to the shell.
            if (!Dialog.IsCompleted)
                return 0;

            Shell.AddDialog("Pause Menu", Dialog.Render, Dialog.OnClose);
            return 1;
        }

        /// <inheritdoc />
        public void Dispose() => Event.PauseGame -= PauseGame;

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

        internal class PauseDialog
        {
            /// <summary>
            /// True if the dialog is closed, else false.
            /// </summary>
            public bool IsCompleted = false;

            /// <summary>
            /// Informs of dialog exit.
            /// </summary>
            public void OnClose() => IsCompleted = true;

            /// <summary>
            /// The component that owns this instance.
            /// </summary>
            public PauseMenu Owner;

            public void Render(ref bool isopened)
            {
                if (ImGui.Button("Quit and Disconnect", Constants.Zero))
                {
                    var socket = Owner.Socket;
                    isopened = false;
                    if (socket.GetSocketType() == SocketType.Host)
                    {
                        socket.DisconnectAllWithMessage("Host has closed lobby.");
                        socket.Dispose();
                    }
                    else
                    {
                        socket.Dispose();
                    }
                }
            }
        }
    }
}
