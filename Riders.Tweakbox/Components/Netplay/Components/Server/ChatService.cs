using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;

namespace Riders.Tweakbox.Components.Netplay.Components.Server;

/// <summary>
/// Provides a public API for sharing messages across the lobby.
/// </summary>
public class ChatService : INetplayComponent, IChatService
{
    public Socket Socket { get; set; }

    internal CommonState State { get; set; }

    internal HostState HostState => (HostState)State;

    /// <summary>
    /// Alows you to receive events when a message is received from another client.
    /// </summary>
    public event OnChatMessage OnReceiveMessage;

    public ChatService(Socket socket)
    {
        Socket = socket;
        State = Socket.State;
    }

    public void Dispose() { }

    /// <summary>
    /// Sends a message to other clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage(string message)
    {
        var packet = ReliablePacket.Create(new ChatMessage(State.SelfInfo.ClientIndex, message));
        if (Socket.GetSocketType() == SocketType.Host)
            Socket.SendToAllAndFlush(packet, DeliveryMethod.ReliableOrdered);
        else
            Socket.SendAndFlush(Socket.Manager.FirstPeer, packet, DeliveryMethod.ReliableOrdered);
    }

    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        if (packet.MessageType != MessageType.ChatMessage)
            return;

        var message     = packet.GetMessage<ChatMessage>();
        var clientIndex = message.SourceIndex;

        if (!State.ClientIndexToDataMap.TryGetValue(clientIndex, out var clientData))
            return;

        OnReceiveMessage?.Invoke(new ChatMessageEvent(clientData.Name, message.Message));

        // Retransmit if host.
        if (Socket.GetSocketType() == SocketType.Host)
            Socket.SendToAllExceptAndFlush(source, packet, DeliveryMethod.ReliableOrdered);
    }

    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
public interface IChatService
{
    /// <summary>
    /// Alows you to receive events when a message is received from another client.
    /// </summary>
    event OnChatMessage OnReceiveMessage;

    /// <summary>
    /// Sends a message to other clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void SendMessage(string message);
}

public delegate void OnChatMessage(in ChatMessageEvent message);

public struct ChatMessageEvent
{
    /// <summary>
    /// The name of the player/entity from who the message originated from.
    /// </summary>
    public string Source;

    /// <summary>
    /// The text associated with the message.
    /// </summary>
    public string Text;

    public ChatMessageEvent(string source, string text)
    {
        Source = source;
        Text = text;
    }
}