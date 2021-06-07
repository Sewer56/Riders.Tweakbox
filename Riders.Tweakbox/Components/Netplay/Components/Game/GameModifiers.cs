using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public class GameModifiers : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }

        public ServerGameModifiers Modifiers;

        public unsafe GameModifiers(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;

            Event.ShouldSpawnTurbulence += ShouldSpawnTurbulence;
            Event.ShouldKillTurbulence += ShouldKillTurbulence;
            Event.ForceTurbulenceType += ForceTurbulenceType;
        }

        /// <inheritdoc />
        public unsafe void Dispose()
        {
            Event.ShouldSpawnTurbulence -= ShouldSpawnTurbulence;
            Event.ShouldKillTurbulence -= ShouldKillTurbulence;
            Event.ForceTurbulenceType -= ForceTurbulenceType;
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
}
