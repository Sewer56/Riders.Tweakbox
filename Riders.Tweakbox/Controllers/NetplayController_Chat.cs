using Riders.Tweakbox.Components.Netplay.Components.Server;
using Riders.Tweakbox.Components.Netplay.Menus;
using Riders.Tweakbox.Components.Netplay.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private void WriteToMenuOnMessageReceive(in ChatMessageEvent message) => Chat.AddMessage(message.Source, message.Text);

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
