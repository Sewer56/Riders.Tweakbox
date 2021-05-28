using System;
using DearImguiSharp;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class PauseMenu : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        internal PauseDialog Dialog { get; private set; }
        private Functions.SetEndOfGameTaskFn _setEndOfGameTask = Functions.SetEndOfGameTask.GetWrapper();

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

            Dialog.IsCompleted = false;
            Shell.AddDialog("Pause Menu", Dialog.Render, Dialog.OnClose);
            return 1;
        }

        /// <inheritdoc />
        public void Dispose() => Event.PauseGame -= PauseGame;

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            if (Socket.GetSocketType() == SocketType.Client)
            {
                if (packet.MessageType != MessageType.EndGame)
                    return;

                var message = packet.GetMessage<EndNetplayGame>();
                switch (message.Mode)
                {
                    case EndMode.Exit:
                        ExitRace();
                        break;
                    case EndMode.Restart:
                        RestartRace();
                        break;
                }
            }
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

        internal unsafe void HostSendAndExit(EndMode mode)
        {
            Socket.SendToAllAndFlush(ReliablePacket.Create(new EndNetplayGame(mode)), DeliveryMethod.ReliableOrdered);
            switch (mode)
            {
                case EndMode.Exit:
                    ExitRace();
                    break;
                case EndMode.Restart:
                    RestartRace();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal unsafe void RestartRace() => _setEndOfGameTask(EndOfGameMode.Restart);
        internal unsafe void ExitRace() => _setEndOfGameTask(EndOfGameMode.Exit);

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

            public unsafe void Render(ref bool isopened)
            {
                var socket = Owner.Socket;
                bool isHost = socket.GetSocketType() == SocketType.Host;
                if (isHost)
                {
                    if (ImGui.Button("Restart", Constants.Zero))
                    {
                        Owner.HostSendAndExit(EndMode.Restart);
                        isopened = false;
                        return;
                    }

                    if (ImGui.Button("Exit", Constants.Zero))
                    {
                        Owner.HostSendAndExit(EndMode.Exit);
                        isopened = false;
                        return;
                    }
                }

                if (ImGui.Button("Disconnect", Constants.Zero))
                {
                    isopened = false;
                    if (isHost)
                        socket.DisconnectAllWithMessage("Host has closed lobby.");
                     
                    socket.Dispose();
                }
            }
        }
    }
}
