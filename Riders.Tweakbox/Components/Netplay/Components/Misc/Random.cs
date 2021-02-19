﻿using System;
using System.Collections.Generic;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Netplay.Messages.Reliable.Structs;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Functions = Sewer56.SonicRiders.Functions.Functions;

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
        private byte _randomChannel;
        private DeliveryMethod _randomDeliveryMethod = DeliveryMethod.ReliableOrdered;

        private SRandSync? _currentSync;

        public Random(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            _framePacingController = IoC.Get<FramePacingController>();
            _randomChannel = (byte)Socket.ChannelAllocator.GetChannel(_randomDeliveryMethod);

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
            Socket.ChannelAllocator.ReleaseChannel(_randomDeliveryMethod, _randomChannel);
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
            Log.WriteLine($"[{nameof(Random)} / Host] Peer Connected, Adding Entry.", LogCategory.Random);
            _syncReady[peer.Id] = false;
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.WriteLine($"[{nameof(Random)} / Host] Peer Disconnected, Removing Entry.", LogCategory.Random);
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
            hook.OriginalFunction(seed);
            if (!Socket.PollUntil(IsEveryoneReady, Socket.State.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(Random)} / Host] It's no use, RNG seed sync failed, let's get outta here!.", LogCategory.Random);
                Socket.Dispose();
                return;
            }

            // Disable skip flags for everyone.
            foreach (var key in _syncReady.Keys)
                _syncReady[key] = false;

            // Seed a new random value for item pickups.
            Log.WriteLine($"[{nameof(Random)} / Host] Seeding: {(int)seed}", LogCategory.Random);
            _itemPickupRandom = new System.Random((int) seed);

            // TODO: Handle error when time component is not available.
            var startTime = DateTime.UtcNow.AddMilliseconds(Socket.State.MaxLatency);
            Socket.SendToAllAndFlush(ReliablePacket.Create(new SRandSync(startTime, (int)seed)), _randomDeliveryMethod, $"[{nameof(Random)} / Host] Sending Random Seed {(int)seed}", LogCategory.Random, _randomChannel);
            Socket.WaitWithSpin(startTime, $"[{nameof(Random)} / Host] SRand Synchronized.", LogCategory.Random, 32);
        }

        private void ClientOnSeedRandom(uint seed, IHook<Functions.SRandFn> hook)
        {
            Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(new SRandSync(default, (int)seed)), _randomDeliveryMethod, $"[{nameof(Random)} / Client] Sending dummy random seed and waiting for host response.", LogCategory.Random, _randomChannel);
            if (!Socket.PollUntil(SyncAvailable, Socket.State.HandshakeTimeout))
            {
                Log.WriteLine($"[{nameof(Random)} / Client] RNG Sync Failed.", LogCategory.Random);
                hook.OriginalFunction(seed);
                Socket.Dispose();
                return;
            }

            // TODO: Handle error when time component is not available.
            var srand = _currentSync.Value;
            _currentSync = null;

            Log.WriteLine($"[{nameof(Random)} / Client] Seeding: {srand.Seed}", LogCategory.Random);
            Event.InvokeSeedRandom(srand.Seed);
            _itemPickupRandom = new System.Random(srand.Seed);

            Socket.TryGetComponent(out TimeSynchronization time);
            var localTime = time.ToLocalTime(srand.StartTime);
            Socket.WaitWithSpin(localTime, $"[{nameof(Random)} / Client] SRand Synchronized.", LogCategory.Random, 32);
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            if (packet.MessageType != MessageType.SRand)
                return;

            if (Socket.GetSocketType() == SocketType.Host)
            {
                // Packet contents ignored.
                Log.WriteLine($"[{nameof(Random)} / Host] Received Ready from Client.", LogCategory.Random);
                _syncReady[source.Id] = true;
            }
            else
            {
                _currentSync = packet.GetMessage<SRandSync>();
            }
        }

        /// <summary>
        /// Returns true if everyone is ready to start the race, else false.
        /// </summary>
        private bool IsEveryoneReady()
        {
            foreach (var value in _syncReady.Values)
            {
                if (!value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// True if a sync message has been received, else false.
        /// </summary>
        private bool SyncAvailable() => _currentSync.HasValue;

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
