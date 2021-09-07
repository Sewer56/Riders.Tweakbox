using System;
using System.Runtime.InteropServices;
using DearImguiSharp;
using EnumsNET;
using LiteNetLib;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Components.Game.Dialog;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Input.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Managed;
using Constants = Sewer56.Imgui.Misc.Constants;
using Task = Sewer56.SonicRiders.Structures.Tasks.Base.Task;

namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public class PauseDialogOverride : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public EventController Event { get; set; }

    /// <summary>
    /// Sets the additional result screen items to be rendered.
    /// </summary>
    public Action AdditionalResultsItems { get; set; }
    
    /// <summary>
    /// Sets the additional pause items to be rendered.
    /// </summary>
    public Action AdditionalPauseItems { get; set; }

    internal PauseDialog PauseDialog { get; private set; }
    internal PauseDialog EndOfRaceDialog { get; private set; }

    private Functions.SetEndOfGameTaskFn _setEndOfGameTask = Functions.SetEndOfGameTask.GetWrapper();

    /// <summary>
    /// Replaces unused code to set Tag Results screen task
    /// to instead call common function to set Results Screen task
    /// with invalid parameter (03). We will intercept this in <see cref="SetEndOfRaceDialog"/>
    /// </summary>
    private PatchCollection _useCustomResultsScreenForTagAndSurvival;
    private PauseDialogTask _restartDialogTask;

    public unsafe PauseDialogOverride(Socket socket,  Assembler asm)
    {
        MakeCustomResultsScreenPatchCollection(asm);
        Socket = socket;
        PauseDialog = new PauseDialog()
        {
            IsCompleted = true,
            Owner = this
        };

        EndOfRaceDialog = new PauseDialog()
        {
            IsCompleted = true,
            Owner = this
        };

        EventController.PauseGame += PauseGame;
        EventController.SetEndOfRaceDialog += SetEndOfRaceDialog;
        _useCustomResultsScreenForTagAndSurvival.Enable();
    }

    /// <inheritdoc />
    public unsafe void Dispose()
    {
        EventController.PauseGame -= PauseGame;
        EventController.SetEndOfRaceDialog -= SetEndOfRaceDialog;
        _useCustomResultsScreenForTagAndSurvival.Disable();
    }

    private unsafe Task* SetEndOfRaceDialog(EndOfRaceDialogMode mode, IHook<Functions.SetEndOfRaceDialogTaskFnPtr> hook)
    {
        EndOfRaceDialog.Initialize(true, mode == EndOfRaceDialogMode.GrandPrix, AdditionalResultsItems);
        Shell.AddDialog("Finished!", EndOfRaceDialog.Render, EndOfRaceDialog.OnClose, showClose: false);
        _restartDialogTask = new PauseDialogTask(EndOfRaceDialog);

        // Kill the dialog if already restarting.
        // We are creating the dialog anyway because we don't want to return a null ptr.
        if (*State.EndOfGameMode != EndOfGameMode.Null)
            _restartDialogTask.Dialog.IsCompleted = true;

        return _restartDialogTask.NativeTask;
    }

    private int PauseGame(int a1, int a2, byte a3, IHook<Functions.PauseGameFnPtr> hook)
    {
        // To handle our "pause", we add a menu task to the shell.
        if (!PauseDialog.IsCompleted)
            return 0;

        PauseDialog.Initialize(false, false, AdditionalPauseItems);
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
            ExecuteEndMode(message.Mode);
            EndOfRaceDialog.IsCompleted = true;
        }
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

    internal unsafe void HostSendAndExit(EndMode mode)
    {
        Socket.SendToAllAndFlush(ReliablePacket.Create(new EndNetplayGame(mode)), DeliveryMethod.ReliableOrdered);
        ExecuteEndMode(mode);
    }

    public void ExecuteEndMode(EndMode mode)
    {
        switch (mode)
        {
            case EndMode.Exit:
                ExitRace();
                break;
            case EndMode.Restart:
                RestartRace();
                break;
            case EndMode.NextTrack:
                NextTrack();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    internal unsafe void NextTrack() => _setEndOfGameTask(EndOfGameMode.GrandPrixNextTrack);
    internal unsafe void RestartRace() => _setEndOfGameTask(EndOfGameMode.Restart);
    internal unsafe void ExitRace() => _setEndOfGameTask(EndOfGameMode.Exit);

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
