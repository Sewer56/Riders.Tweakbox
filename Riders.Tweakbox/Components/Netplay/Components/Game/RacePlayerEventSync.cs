using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Gameplay;
using StructLinq;
using Constants = Riders.Netplay.Messages.Misc.Constants;
using Extensions = Riders.Tweakbox.Components.Netplay.Helpers.Extensions;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    /// <summary>
    /// Synchronizes "player events" such as boosting and placing tornadoes.
    /// </summary>
    public unsafe class RacePlayerEventSync : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public CommonState State { get; set; }

        /// <summary>
        /// Contains available events for each player.
        /// </summary>
        private BoostTornado[] _events = new BoostTornado[Constants.MaxNumberOfPlayers + 1];

        private const DeliveryMethod _eventDeliveryMethod = DeliveryMethod.ReliableOrdered;

        public RacePlayerEventSync(Socket socket, EventController @event)
        {
            Socket = socket;
            Event = @event;
            State = socket.State;

            Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
        }

        public void Reset()
        {
            Array.Fill(_events, new Timestamped<BoostTornado>());
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            // ReSharper disable once VariableHidesOuterVariable
            bool IsLastPlayer(int playerIndex) => playerIndex == *Sewer56.SonicRiders.API.State.NumberOfRacers - 1;

            Socket.Update();
            int playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

            // Populate Local Changes
            if (State.IsLocal(playerIndex))
                _events[playerIndex] = new BoostTornado(player);

            // Send Changes if Host/Client
            if (Socket.GetSocketType() == SocketType.Host && IsLastPlayer(playerIndex) && HasAnyChanges())
            {
                Span<byte> excludeIndexBuffer = stackalloc byte[Constants.MaxNumberOfLocalPlayers];
                for (var peerId = 0; peerId < Socket.Manager.ConnectedPeerList.Count; peerId++)
                {
                    // Calculate some preliminary data.
                    var peer = Socket.Manager.ConnectedPeerList[peerId];
                    if (!((HostState) State).ClientMap.Contains(peer))
                        continue;

                    var excludeIndices = Extensions.GetExcludeIndices((HostState)State, peer, excludeIndexBuffer);

                    // Get all attacks sans those made by players and their local players;
                    // then check if there are any attacks they should be made aware of.
                    using var events = Extensions.GetItemsWithoutIndices(_events.AsSpan(0, State.GetPlayerCount()), excludeIndices);
                    var eventsArr = events.Segment.Array;
                    if (!eventsArr.ToStructEnumerable().Any(x => x.HasValue(), x => x))
                        continue;

                    // Transmit Packet Information
                    using var boostTornado = new BoostTornadoPacked();
                    boostTornado.Set(eventsArr, events.Length);
                    Socket.Send(peer, ReliablePacket.Create(boostTornado), _eventDeliveryMethod);
                }

                Log.WriteLine($"[{nameof(RacePlayerEventSync)} / Host] Player Event Matrix Sent", LogCategory.PlayerEvent);
                Socket.Update();
            }
            else if (Socket.GetSocketType() == SocketType.Client && State.IsLocal(playerIndex) && _events[playerIndex].HasValue())
            {
                // Send value update notification to host.
                using var packed = new BoostTornadoPacked().CreatePooled(State.NumLocalPlayers);
                packed.Elements[playerIndex] = _events[playerIndex];
                Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(packed), _eventDeliveryMethod);

                Log.WriteLine($"[{nameof(RacePlayerEventSync)} / Client] Player Event Matrix Sent", LogCategory.PlayerEvent);
            }

            // Synchronize with Host/Client
            // This discards the value, hence we do this last.
            return ApplyMovementEvents(player);
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            try
            {
                if (packet.MessageType != MessageType.BoostTornado)
                    return;

                // Index of first player to fill.
                int playerIndex = Socket.GetSocketType() switch
                {
                    SocketType.Host => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
                    SocketType.Client => State.NumLocalPlayers,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var message     = packet.GetMessage<BoostTornadoPacked>();
                var elements    = message.Elements;
                var numElements = message.NumElements;

                for (int x = 0; x < numElements; x++)
                    _events[playerIndex + x].Merge(elements[x]);
            }
            catch (Exception ex)
            {
                Log.WriteLine($"[{nameof(RacePlayerEventSync)}] Warning: Failed to update Boost/Tornado/Event Sync | {ex.Message}", LogCategory.PlayerEvent);
            }
        }

        /// <summary>
        /// Handles all movement flags to be applied to the client.
        /// </summary>
        private Player* ApplyMovementEvents(Player* player)
        {
            try
            {
                var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);

                if (State.IsLocal(index))
                    return player;

                _events[index].ToGameAndReset(player);
            }
            catch (Exception e)
            {
                Log.WriteLine($"[{nameof(RacePlayerEventSync)}] Failed to Apply Movement Flags {e.Message} {e.StackTrace}", LogCategory.PlayerEvent);
            }

            return player;
        }

        /// <summary>
        /// True if there are any attacks to be transmitted, else false.
        /// </summary>
        private bool HasAnyChanges()
        {
            for (int x = 0; x < _events.Length; x++)
            {
                if (_events[x].HasValue())
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}