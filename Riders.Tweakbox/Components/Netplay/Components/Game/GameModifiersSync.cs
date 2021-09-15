using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public class GameModifiersSync : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    private GameModifiersController _modifiersController;

    public unsafe GameModifiersSync(Socket socket, GameModifiersController modifiersController)
    {
        Socket = socket;
        _modifiersController = modifiersController;
        _modifiersController.OnEditModifiers += OnEditModifiers;
    }

    /// <inheritdoc />
    public unsafe void Dispose() => _modifiersController.OnEditModifiers -= OnEditModifiers;

    private void OnEditModifiers()
    {
        if (Socket.GetSocketType() == SocketType.Host)
            HostSendSettingsToAll();
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        if (packet.MessageType != MessageType.ServerGameModifiers)
            return;

        if (Socket.GetSocketType() != SocketType.Client)
            return;

        var modifierController = IoC.GetSingleton<GameModifiersController>();
        modifierController.SetModifiers(packet.GetMessage<GameModifiers>());
        Log.WriteLine("Applied new Game Modifiers from Host");
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

    /// <summary>
    /// Sends updated settings to all clients.
    /// </summary>
    public void HostSendSettingsToAll()
    {
        using var message = ReliablePacket.Create(_modifiersController.Modifiers);
        Socket.SendToAllAndFlush(message, DeliveryMethod.ReliableOrdered, "Sending updated Settings to all Clients", LogCategory.Default);
    }

    /// <summary>
    /// Sends updated settings to all clients.
    /// </summary>
    public void HostSendSettingsToSinglePeer(NetPeer peer)
    {
        using var message = ReliablePacket.Create(_modifiersController.Modifiers);
        Socket.SendAndFlush(peer, message, DeliveryMethod.ReliableOrdered, "Sending updated Settings to Single Client", LogCategory.Default);
    }
}
