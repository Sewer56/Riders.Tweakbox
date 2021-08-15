using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Controllers.ObjectLayoutController;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Parser.Layout.Enums;
using Sewer56.SonicRiders.Parser.Layout.Objects.ItemBox;
using Sewer56.SonicRiders.Structures.Gameplay;
namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public class GameModifiers : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    public EventController Event { get; set; }

    public ServerGameModifiers Modifiers;
    private ObjectLayoutController _layoutController;

    public unsafe GameModifiers(Socket socket, EventController @event, ObjectLayoutController layout)
    {
        Socket = socket;
        Event = @event;
        _layoutController = layout;

        Event.ShouldSpawnTurbulence += ShouldSpawnTurbulence;
        Event.ShouldKillTurbulence += ShouldKillTurbulence;
        Event.ForceTurbulenceType += ForceTurbulenceType;
        _layoutController.OnLoadLayout += OnLoadLayout;
    }

    /// <inheritdoc />
    public unsafe void Dispose()
    {
        Event.ShouldSpawnTurbulence -= ShouldSpawnTurbulence;
        Event.ShouldKillTurbulence -= ShouldKillTurbulence;
        Event.ForceTurbulenceType -= ForceTurbulenceType;
        _layoutController.OnLoadLayout -= OnLoadLayout;
    }

    private void OnLoadLayout(ref InMemoryLayoutFile layout)
    {
        for (int x = 0; x < layout.Objects.Count; x++)
        {
            ref var obj = ref layout.Objects[x];
            if (obj.Type != ObjectId.oItemBox)
                continue;

            if (Modifiers.ReplaceAirMaxBox && obj.Attribute == (int)ItemBoxAttribute.AirMax)
                obj.Attribute = (int)Modifiers.AirMaxReplacement;

            if (Modifiers.ReplaceRing100Box && obj.Attribute == (int)ItemBoxAttribute.Ring100)
                obj.Attribute = (int)Modifiers.Ring100Replacement;
        }
    }

    private int ForceTurbulenceType(byte currentType)
    {
        if (Modifiers.DisableSmallTurbulence && currentType == 1)
            return 2;

        return currentType;
    }

    private unsafe bool ShouldKillTurbulence(Player* player, IHook<Functions.ShouldKillTurbulenceFn> hook) => !Modifiers.AlwaysTurbulence && hook.OriginalFunction(player);
    private unsafe bool ShouldSpawnTurbulence(Player* player, IHook<Functions.ShouldGenerateTurbulenceFn> hook) => Modifiers.AlwaysTurbulence || hook.OriginalFunction(player);

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        if (packet.MessageType != MessageType.ServerGameModifiers)
            return;

        if (Socket.GetSocketType() != SocketType.Client)
            return;

        Modifiers = packet.GetMessage<ServerGameModifiers>();
        Log.WriteLine("Applied new Game Modifiers from Host");
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }

    /// <summary>
    /// Sends updated settings to all clients.
    /// </summary>
    public void HostSendSettingsToAll()
    {
        using var message = ReliablePacket.Create(Modifiers);
        Socket.SendToAllAndFlush(message, DeliveryMethod.ReliableOrdered, "Sending updated Settings to all Clients", LogCategory.Default);
    }

    /// <summary>
    /// Sends updated settings to all clients.
    /// </summary>
    public void HostSendSettingsToSinglePeer(NetPeer peer)
    {
        using var message = ReliablePacket.Create(Modifiers);
        Socket.SendAndFlush(peer, message, DeliveryMethod.ReliableOrdered, "Sending updated Settings to Single Client", LogCategory.Default);
    }
}
