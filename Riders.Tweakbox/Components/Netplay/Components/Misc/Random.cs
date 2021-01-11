using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc
{
    public class Random : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        private Dictionary<int, bool> _syncReady; 

        public Random(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;

            Event.SeedRandom += OnSeedRandom;
            Event.Random     += OnRandom;

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
            
            if (Socket.GetSocketType() == SocketType.Host)
            {
                Socket.Listener.PeerConnectedEvent -= OnPeerConnected;
                Socket.Listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            }
        }

        private void OnPeerConnected(NetPeer peer) => _syncReady[peer.Id] = false;
        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => _syncReady.Remove(peer.Id);

        private int OnRandom(IHook<Functions.RandFn> hook)
        {
            var result = hook.OriginalFunction();
            Trace.WriteLine($"[{nameof(Random)}] Current Seed: {result}");
            return result;
        }

        private void OnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                HostOnSeedRandom(seed, hook);
            else
                ClientOnSeedRandom(seed, hook);
        }

        private void HostOnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            var state = (HostState)Socket.State;
            hook.OriginalFunction(seed);

            if (!Socket.PollUntil(IsEveryoneReady, Socket.State.HandshakeTimeout))
            {
                Trace.WriteLine($"[{nameof(Random)}] It's no use, RNG seed sync failed, let's get outta here!.");
                Socket.Dispose();
                return;
            }

            Socket.SendToAllAndFlush(new ReliablePacket() { Random = new Seed((int)seed) }, DeliveryMethod.ReliableSequenced, $"[{nameof(Random)} / Host] Sending Random Seed {(int)seed}");

            // Disable skip flags for everyone.
            foreach (var key in _syncReady.Keys)
                _syncReady[key] = false;

            // Local function(s)
            bool IsEveryoneReady()
            {
                return _syncReady.All(x => x.Value == true);
            }
        }

        private void ClientOnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            bool HandleSeedPacket(Packet<NetPeer> packet)
            {
                if (packet.Value.Value.GetPacketType() != PacketKind.Reliable)
                    return false;

                var reliable = packet.As<ReliablePacket>();
                if (!reliable.Random.HasValue)
                    return false;

                Trace.WriteLine($"[{nameof(Random)} / Client] Received Random Seed, Seeding {reliable.Random.Value.Value}");
                Event.InvokeSeedRandom(reliable.Random.Value.Value);
                return true;
            }

            Socket.SendToAllAndFlush(new ReliablePacket() { Random = new Seed((int)seed) }, DeliveryMethod.ReliableSequenced, $"[{nameof(Random)} / Client] Sending dummy random seed and waiting for host response.");
            if (!Socket.TryWaitForMessage(Socket.Manager.FirstPeer, HandleSeedPacket, Socket.State.HandshakeTimeout))
            {
                hook.OriginalFunction(seed);
                Dispose();
            }
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
                    Trace.WriteLine("[Host] Received SRandSyncReady from Client.");
                    _syncReady[peer.Id] = true;
                }
            }
        }
    }
}
