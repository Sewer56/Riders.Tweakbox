using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Components.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using System.Runtime;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    public class Random : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        private Dictionary<int, bool> _syncReady;

        private FramePacingController _framePacingController;
        private System.Random _itemPickupRandom;

        public Random(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            _framePacingController = IoC.Get<FramePacingController>();

            Event.SeedRandom += OnSeedRandom;
            Event.Random     += OnRandom;
            Event.ItemPickupRandom += OnItemPickupRandom;
            Event.OnCheckIfGiveAiRandomItems += OnChekIfGiveAiRandomItems;
            _itemPickupRandom = new System.Random();

            if (Socket.GetSocketType() == SocketType.Host)
            {
                _syncReady = new Dictionary<int, bool>(8);
                Socket.Listener.PeerConnectedEvent += OnPeerConnected;
                Socket.Listener.PeerDisconnectedEvent += OnPeerDisconnected;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.SeedRandom -= OnSeedRandom;
            Event.Random     -= OnRandom;
            Event.ItemPickupRandom -= OnItemPickupRandom;
            Event.OnCheckIfGiveAiRandomItems -= OnChekIfGiveAiRandomItems;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                Socket.Listener.PeerConnectedEvent -= OnPeerConnected;
                Socket.Listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            }
        }

        private Enum<AsmFunctionResult> OnChekIfGiveAiRandomItems()
        {
            Log.WriteLine($"[{nameof(Random)}] Overwriting to Give AI Random Item", LogCategory.RandomSeed);
            return true;
        }

        private void OnPeerConnected(NetPeer peer)
        {
            Log.WriteLine($"[{nameof(Random)} / Host] Peer Connected, Adding Entry: ", LogCategory.Random);
            _syncReady[peer.Id] = false;
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.WriteLine($"[{nameof(Random)} / Host] Peer Disconnected, Removing Entry: ", LogCategory.Random);
            _syncReady.Remove(peer.Id);
        }

        private int OnItemPickupRandom(IHook<Functions.RandFn> hook)
        {
            var result = _itemPickupRandom.Next();
            Log.WriteLine($"[{nameof(Random)}] Item Pickup Seed: {result}", LogCategory.RandomSeed);
            return result;
        }

        private int OnRandom(IHook<Functions.RandFn> hook)
        {
            var result = hook.OriginalFunction();
            Log.WriteLine($"[{nameof(Random)}] Current Seed: {result}", LogCategory.RandomSeed);
            return result;
        }

        private void OnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            Log.WriteLine($"[{nameof(Random)}] Calling Random Number Generator", LogCategory.Random);

            if (Socket.GetSocketType() == SocketType.Host)
                HostOnSeedRandom(seed, hook);
            else
                ClientOnSeedRandom(seed, hook);

            _framePacingController.ResetSpeedup();
        }

        private void HostOnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            // Local function(s)
            bool IsEveryoneReady() => _syncReady.All(x => x.Value == true);

            hook.OriginalFunction(seed);
            if (!Socket.PollUntil(IsEveryoneReady, Socket.State.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(Random)} / Host] It's no use, RNG seed sync failed, let's get outta here!.", LogCategory.Random);
                Socket.Dispose();
                return;
            }

            // Seed a new random value for item pickups.
            Log.WriteLine($"[{nameof(Random)} / Host] Seeding: {(int)seed}", LogCategory.Random);
            _itemPickupRandom = new System.Random((int) seed);

            // TODO: Handle error when time component is not available.
            var startTime = DateTime.UtcNow.AddMilliseconds(Socket.State.MaxLatency);
            Socket.TryGetComponent(out TimeSynchronization time);
            var serverStartTime = time.ToServerTime(startTime);
            Socket.SendToAllAndFlush(new ReliablePacket() { Random = new SRandSync(serverStartTime, (int)seed) }, DeliveryMethod.ReliableOrdered, $"[{nameof(Random)} / Host] Sending Random Seed {(int)seed}", LogCategory.Random);

            // Disable skip flags for everyone.
            foreach (var key in _syncReady.Keys)
                _syncReady[key] = false;

            Socket.WaitWithSpin(startTime, $"[{nameof(Random)} / Host] SRand Synchronized.", LogCategory.Random, 32);
        }

        private void ClientOnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            SRandSync srand = default;
            bool HandleSeedPacket(Packet<NetPeer> packet)
            {
                if (packet.Value.Value.GetPacketType() != PacketKind.Reliable)
                    return false;

                var reliable = packet.As<ReliablePacket>();
                if (!reliable.Random.HasValue)
                    return false;

                Log.WriteLine($"[{nameof(Random)} / Client] Received Random Seed, Seeding {reliable.Random.Value.Seed}", LogCategory.Random);
                srand = reliable.Random.Value;
                return true;
            }

            Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket() { Random = new SRandSync(default, (int)seed) }, DeliveryMethod.ReliableUnordered, $"[{nameof(Random)} / Client] Sending dummy random seed and waiting for host response.", LogCategory.Random);
            if (!Socket.TryWaitForMessage(Socket.Manager.FirstPeer, HandleSeedPacket, Socket.State.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(Random)} / Client] RNG Sync Failed.", LogCategory.Random);
                hook.OriginalFunction(seed);
                Socket.Dispose();
                return;
            }

            // TODO: Handle error when time component is not available.
            Log.WriteLine($"[{nameof(Random)} / Client] Seeding: {srand.Seed}", LogCategory.Random);
            Event.InvokeSeedRandom(srand.Seed);
            _itemPickupRandom = new System.Random(srand.Seed);

            Socket.TryGetComponent(out TimeSynchronization time);
            var localTime = time.ToLocalTime(srand.StartTime);
            Socket.WaitWithSpin(localTime, $"[{nameof(Random)} / Client] SRand Synchronized.", LogCategory.Random, 32);
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.GetPacketKind() == PacketKind.Reliable)
                HandleReliablePacket(packet.Source, packet.As<ReliablePacket>());
        }

        private void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                if (packet.Random.HasValue)
                {
                    // Packet contents ignored, it's just reuse to save on enum bit space.
                    Log.WriteLine($"[{nameof(Random)} / Host] Received Ready from Client.", LogCategory.Random);
                    _syncReady[peer.Id] = true;
                }
            }
        }
    }
}
