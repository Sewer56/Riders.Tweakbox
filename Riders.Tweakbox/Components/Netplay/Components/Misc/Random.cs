﻿using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Tweakbox.Components.Netplay.Components.Game;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using Functions = Sewer56.SonicRiders.Functions.Functions;
using Riders.Tweakbox.Misc.Log;
using Riders.Tweakbox.Services.Common;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.ObjectLayout;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc;

public unsafe class Random : INetplayComponent
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
    private Logger _logRandom = new Logger(LogCategory.Random);
    private Logger _logRandomSeed = new Logger(LogCategory.RandomSeed);

    private ObjectLayoutService _layoutService;

    public Random(Socket socket, EventController eventController)
    {
        Socket = socket;
        _framePacingController = IoC.Get<FramePacingController>();
        _layoutService = IoC.Get<ObjectLayoutService>();
        _randomChannel = (byte)Socket.ChannelAllocator.GetChannel(_randomDeliveryMethod);

        EventController.SeedRandom += OnSeedRandom;
        EventController.Random += OnRandom;
        EventController.ItemPickupRandom += OnItemPickupRandom;
        EventController.OnCheckIfGiveAiRandomItems += OnCheckIfGiveAiRandomItems;
        Event = eventController;
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
        EventController.SeedRandom -= OnSeedRandom;
        EventController.Random -= OnRandom;
        EventController.ItemPickupRandom -= OnItemPickupRandom;
        EventController.OnCheckIfGiveAiRandomItems -= OnCheckIfGiveAiRandomItems;

        if (Socket.GetSocketType() == SocketType.Host)
        {
            Socket.Listener.PeerConnectedEvent -= OnPeerConnected;
            Socket.Listener.PeerDisconnectedEvent -= OnPeerDisconnected;
        }
    }

    private Enum<AsmFunctionResult> OnCheckIfGiveAiRandomItems()
    {
        _logRandomSeed.WriteLine($"[{nameof(Random)}] Overwriting to Give AI Random Item");
        return true;
    }

    private void OnPeerConnected(NetPeer peer)
    {
        _logRandom.WriteLine($"[{nameof(Random)} / Host] Peer Connected, Adding Entry.");
        _syncReady[peer.Id] = false;
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _logRandom.WriteLine($"[{nameof(Random)} / Host] Peer Disconnected, Removing Entry.");
        _syncReady.Remove(peer.Id);
    }

    private int OnItemPickupRandom(IHook<Functions.RandFnPtr> hook)
    {
        var result = _itemPickupRandom.Next();
        _logRandomSeed.WriteLine($"[{nameof(Random)}] Item Pickup Seed: {result}");
        return result;
    }

    private int OnRandom(IHook<Functions.RandFnPtr> hook)
    {
        var result = hook.OriginalFunction.Value.Invoke();
        _logRandomSeed.WriteLine($"[{nameof(Random)}] Current Seed: {result}");
        return result;
    }

    private void OnSeedRandom(uint seed, IHook<Functions.SRandFnPtr> hook)
    {
        _logRandom.WriteLine($"[{nameof(Random)}] Calling Random Number Generator");

        if (Socket.GetSocketType() == SocketType.Host)
            HostOnSeedRandom(seed, hook);
        else
            ClientOnSeedRandom(seed, hook);

        _framePacingController.ResetSpeedup();
    }

    private void HostOnSeedRandom(uint seed, IHook<Functions.SRandFnPtr> hook)
    {
        // Local function(s)
        hook.OriginalFunction.Value.Invoke(seed);
        if (!Socket.PollUntil(IsEveryoneReady, Socket.State.DisconnectTimeout))
        {
            // Disconnect those who are not ready.
            foreach (var pair in _syncReady)
            {
                if (pair.Value == true)
                    continue;

                var peer = Socket.Manager.GetPeerById(pair.Key);
                if (peer != null & Socket.HostState.ClientMap.TryGetPlayerData(peer, out var data))
                    Socket.DisconnectWithMessage(peer, $"Your client hasn't reported in time to sync RNG. Timeout: {Socket.State.DisconnectTimeout}ms");
            }

            return;
        }

        // Disable skip flags for everyone.
        foreach (var key in _syncReady.Keys)
            _syncReady[key] = false;

        // Seed a new random value for item pickups.
        _logRandom.WriteLine($"[{nameof(Random)} / Host] Seeding: {(int)seed}");
        _itemPickupRandom = new System.Random((int)seed);
        IFileService.SeedAll((int)seed);

        // Multiply the highest recently recorded Round Trip Time and multiply by 2 in case of spike.
        // Should be good enough as long as RecentLatencies is a list long enough.
        if (Socket.HostState.ClientInfo.Length > 0)
        {
            var timeOffset = (Socket.HostState.ClientInfo.Max(x => x.RecentLatencies.Max(y => y.Value + 0.5)) * 2) * 2;
            _logRandom.WriteLine($"[{nameof(Random)} / Host] Time Offset: {timeOffset}ms");
            var startTime = DateTime.UtcNow.AddMilliseconds(timeOffset);
            Socket.SendToAllAndFlush(ReliablePacket.Create(new SRandSync(startTime, (int)seed)), _randomDeliveryMethod, $"[{nameof(Random)} / Host] Sending Random Seed {(int)seed}", LogCategory.Random, _randomChannel);
            Socket.WaitWithSpin(startTime, $"[{nameof(Random)} / Host] SRand Synchronized.", LogCategory.Random, 32);
        }

        ResetRaceComponent();
    }

    private void ClientOnSeedRandom(uint seed, IHook<Functions.SRandFnPtr> hook)
    {
        Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(new SRandSync(default, (int)seed)), _randomDeliveryMethod, $"[{nameof(Random)} / Client] Sending dummy random seed and waiting for host response.", LogCategory.Random, _randomChannel);
        if (!Socket.PollUntil(SyncAvailable, Socket.State.DisconnectTimeout))
        {
            _logRandom.WriteLine($"[{nameof(Random)} / Client] RNG Sync Failed.");
            hook.OriginalFunction.Value.Invoke(seed);
            Socket.Dispose();
            return;
        }

        var srand = _currentSync.Value;
        _currentSync = null;

        _logRandom.WriteLine($"[{nameof(Random)} / Client] Seeding: {srand.Seed}");
        Event.InvokeSeedRandom(srand.Seed);
        _itemPickupRandom = new System.Random(srand.Seed);
        IFileService.SeedAll((int)srand.Seed);

        Socket.TryGetComponent(out TimeSynchronization time);
        var localTime = time.ToLocalTime(srand.StartTime);
        Socket.WaitWithSpin(localTime, $"[{nameof(Random)} / Client] SRand Synchronized.", LogCategory.Random, 32);
        ResetRaceComponent();
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        if (packet.MessageType != MessageType.SRand)
            return;

        if (Socket.GetSocketType() == SocketType.Host)
        {
            // Packet contents ignored.
            _logRandom.WriteLine($"[{nameof(Random)} / Host] Received Ready from Client.");
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

    private void ResetRaceComponent()
    {
        if (Socket.TryGetComponent(out Race race))
        {
            race.Reset();
            Socket.State.FrameCounter = 0;
        }
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
