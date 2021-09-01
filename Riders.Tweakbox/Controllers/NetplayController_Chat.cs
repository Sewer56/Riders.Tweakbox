using Riders.Tweakbox.Components.Netplay.Components.Server;
using Riders.Tweakbox.Components.Netplay.Menus;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Structures;
using Sewer56.Imgui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sewer56.Imgui.Utilities.Pivots;

namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Contains the chat related part of the netplay controller.
/// </summary>
public partial class NetplayController
{
    /// <summary>
    /// Allows you to chat with other people in the same server.
    /// </summary>
    public ChatMenu Chat { get; set; }

    private ChatService _chatService;
    private LogRenderer _logRenderer = new LogRenderer("Netplay Controller Log Renderer");
    private Logger _log = new Logger(LogCategory.NetplayChat);

    private void InitializeChatComponent()
    {
        Chat = new ChatMenu(GetPlayerName, SendMessage, () => IsConnected());
        _logRenderer.LogPosition = Pivots.Pivot.Bottom;
        Shell.AddCustom(RenderChatPopups);
    }

    private bool RenderChatPopups()
    {
        _logRenderer.Render();
        return true;
    }

    private void InitializeChat(Socket socket)
    {
        Chat.Clear();
        socket.TryGetComponent(out _chatService);
        if (_chatService != null)
            _chatService.OnReceiveMessage += WriteToMenuOnMessageReceive;
    }

    private void DisposeChat()
    {
        Chat.Clear();
        if (_chatService != null)
            _chatService.OnReceiveMessage -= WriteToMenuOnMessageReceive;
    }

    private void WriteToMenuOnMessageReceive(in ChatMessageEvent message)
    {
        var formatted = Chat.FormatMessage(message.Source, message.Text);
        _log.WriteLine(formatted);
        _logRenderer.Log(new LogItem(formatted));
        Chat.AddMessageUnformatted(formatted);
    }

    private string GetPlayerName()
    {
        if (Socket == null)
            return "";

        return Socket.State.SelfInfo.Name;
    }

    private void SendMessage(string message)
    {
        if (Socket == null)
            return;

        _chatService?.SendMessage(message);
    }
}
