using System;
using DearImguiSharp;
using EnumsNET;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Sewer56.Imgui.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Input.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Managed;

namespace Riders.Tweakbox.Components.Netplay.Components.Game.Dialog;

/// <summary>
/// A native game task which does nothing except checking the state of a given message dialog.
/// </summary>
internal unsafe class PauseDialogTask : ManagedTask
{
    public PauseDialog Dialog;

    public PauseDialogTask(PauseDialog dialog)
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

internal class PauseDialog
{
    /// <summary>
    /// True if the dialog is closed, else false.
    /// </summary>
    public bool IsCompleted = false;

    /// <summary>
    /// Shows the "next track" button.
    /// </summary>
    public bool ShowNextTrack = false;

    /// <summary>
    /// True if this dialog is for end of race, else false.
    /// </summary>
    public bool IsEndOfRace = false;

    /// <summary>
    /// Informs of dialog exit.
    /// </summary>
    public void OnClose() => IsCompleted = true;

    /// <summary>
    /// The component that owns this instance.
    /// </summary>
    public PauseDialogOverride Owner;

    /// <summary>
    /// Optional function used to render additional items to the screen.
    /// </summary>
    public Action RenderAdditionalItems { get; private set; }

    private bool _isFirstFrame;

    /// <summary>
    /// Utility method for quickly setting the relevant properties.
    /// </summary>
    public void Initialize(bool isEndOfRace, bool showNextTrackOption, Action renderAdditionalItems)
    {
        IsCompleted = false;
        IsEndOfRace = isEndOfRace;
        RenderAdditionalItems = renderAdditionalItems;
        ShowNextTrack = showNextTrackOption;
        _isFirstFrame = true;
    }

    /// <summary>
    /// Renders the contents of the window.
    /// </summary>
    /// <param name="isOpened">Controls whether the window should be opened.</param>
    public unsafe void Render(ref bool isOpened) => RenderInternal(ref isOpened);

    private unsafe bool RenderInternal(ref bool isOpened)
    {
        RenderAdditionalItems();
        var socket = Owner.Socket;
        bool isHost = socket.GetSocketType() == SocketType.Host;
        if (isHost)
        {
            if (ShowNextTrack && ImGui.Button("Next Track", Constants.Zero))
                return HostCommitMode(EndMode.NextTrack, ref isOpened);

            if (ImGui.Button("Restart", Constants.Zero))
                return HostCommitMode(EndMode.Restart, ref isOpened);

            if (ImGui.Button("Exit", Constants.Zero))
                return HostCommitMode(EndMode.Exit, ref isOpened);
        }

        if (!isHost && IsEndOfRace)
            ImGui.Text("Waiting for Host to Make Decision.");

        string disconnectString = IsEndOfRace ? "Disconnect & Quit" : "Disconnect";
        if (ImGui.Button(disconnectString, Constants.Zero))
        {
            isOpened = false;
            if (isHost)
                socket.DisconnectAllWithMessage("Host has closed lobby.");

            socket.Dispose();

            if (IsEndOfRace)
                Owner.ExecuteEndMode(EndMode.Exit);
        }

        if (!IsEndOfRace && !_isFirstFrame && AnyPlayerPressedPause())
            IsCompleted = true;

        if (IsCompleted)
            isOpened = false;

        _isFirstFrame = false;
        return true;
    }

    private bool HostCommitMode(EndMode mode, ref bool isOpened)
    {
        Owner.HostSendAndExit(mode);
        isOpened = false;
        return isOpened;
    }

    private bool AnyPlayerPressedPause()
    {
        for (int x = 0; x < Player.Inputs.Count; x++)
        {
            ref var input = ref Player.Inputs[x];
            if (input.ButtonsPressed.HasAllFlags(Buttons.Start))
                return true;
        }

        return false;
    }
}