using System;
using System.Runtime.InteropServices;
using DearImguiSharp;
using LiteNetLib;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Managed;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class PauseDialogOverride : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        internal MessageDialog PauseDialog { get; private set; }
        internal MessageDialog EndOfRaceDialog { get; private set; }

        private Functions.SetEndOfGameTaskFn _setEndOfGameTask = Functions.SetEndOfGameTask.GetWrapper();

        /// <summary>
        /// Replaces unused code to set Tag Results screen task
        /// to instead call common function to set Results Screen task
        /// with invalid parameter (03). We will intercept this in <see cref="SetEndOfRaceDialog"/>
        /// </summary>
        private PatchCollection _useCustomResultsScreenForTagAndSurvival;
        private MessageDialogTask _restartDialogTask;

        public unsafe PauseDialogOverride(Socket socket, EventController @event, Assembler asm)
        {
            MakeCustomResultsScreenPatchCollection(asm);
            Socket = socket;
            Event  = @event;
            PauseDialog = new MessageDialog()
            {
                IsCompleted = true,
                Owner = this
            };

            EndOfRaceDialog = new MessageDialog()
            {
                IsCompleted = true,
                Owner = this
            };

            Event.PauseGame += PauseGame;
            Event.SetEndOfRaceDialog += SetEndOfRaceDialog;
            _useCustomResultsScreenForTagAndSurvival.Enable();
        }

        /// <inheritdoc />
        public unsafe void Dispose()
        {
            Event.PauseGame -= PauseGame;
            Event.SetEndOfRaceDialog -= SetEndOfRaceDialog;
            _useCustomResultsScreenForTagAndSurvival.Disable();
        }

        private unsafe Task* SetEndOfRaceDialog(EndOfRaceDialogMode mode, IHook<Functions.SetEndOfRaceDialogTaskFn> hook)
        {
            EndOfRaceDialog.IsCompleted = false;
            Shell.AddDialog("Finished!", EndOfRaceDialog.Render, EndOfRaceDialog.OnClose, showClose: false);
            _restartDialogTask = new MessageDialogTask(EndOfRaceDialog);
            return _restartDialogTask.NativeTask;
        }

        private int PauseGame(int a1, int a2, byte a3, IHook<Functions.PauseGameFn> hook)
        {
            // To handle our "pause", we add a menu task to the shell.
            if (!PauseDialog.IsCompleted)
                return 0;

            PauseDialog.IsCompleted = false;
            Shell.AddDialog("Paused", PauseDialog.Render, PauseDialog.OnClose);
            return 1;
        }

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

        /// <summary>
        /// A native game task which does nothing except checking the state of a given message dialog.
        /// </summary>
        internal unsafe class MessageDialogTask : ManagedTask
        {
            public MessageDialog Dialog;

            public MessageDialogTask(MessageDialog dialog)
            {
                Dialog = dialog;
                Construct(Action, 0xEAD3);
            }

            void Action()
            {
                if (Dialog.IsCompleted) 
                    Functions.KillTask.GetWrapper()();
            }
        }

        internal class MessageDialog
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
            public PauseDialogOverride Owner;

            /// <summary>
            /// Renders the contents of the window.
            /// </summary>
            /// <param name="isopened">Controls whether the window should be opened.</param>
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

        #region Custom Results Patches

        private void MakeCustomResultsScreenPatchCollection(Assembler asm)
        {
            _useCustomResultsScreenForTagAndSurvival = new PatchCollection(new[]
            {
                // Tag (unused)
                new Patch(0x0043B570, asm.Assemble(new[]
                {
                    "use32",
                    "org 0x0043B570",
                    "push 03",
                    "call 0x0043ABD0",
                    "mov ecx,[esp+0x18]",
                    "add esp,0x0C",
                    "mov [ecx+0x18],eax",
                    "jmp 0x0043B6B7"
                })),

                // Survival Mode (used)
                new Patch(0x004166CB, asm.Assemble(new[]
                {
                    "use32",
                    "org 0x004166CB",
                    "push 03",
                    "call 0x0043ABD0",
                    "add esp,0x04",
                    "jmp 0x004166DF"
                })),

                // Tag (used)
                new Patch(0x00416D49, asm.Assemble(new[]
                {
                    "use32",
                    "org 0x00416D49",
                    "push 03",
                    "call 0x0043ABD0",
                    "add esp, 0x04",
                    "jmp 0x00416D5D"
                })),
            });
        }

        #endregion
    }
}
