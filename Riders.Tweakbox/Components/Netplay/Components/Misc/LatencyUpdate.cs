﻿using System;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Misc.Log;
using StructLinq;
namespace Riders.Tweakbox.Components.Netplay.Components.Misc;

public class LatencyUpdate : INetplayComponent
{
    private const int UpdateIntervalMs = ClientData.LatencyUpdatePeriod;

    /// <inheritdoc />
    public Socket Socket { get; set; }
    public NetManager Manager { get; set; }
    private Timer _synchronizeTimer;

    private Logger _log = new Logger(LogCategory.Menu);

    public LatencyUpdate(Socket socket)
    {
        Socket = socket;
        Manager = socket.Manager;

        Socket.Listener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdate;

        if (Socket.GetSocketType() == SocketType.Host)
            _synchronizeTimer = new Timer(UpdateClientLatencies, null, 0, UpdateIntervalMs);
    }

    /// <inheritdoc />
    public void Dispose() => _synchronizeTimer?.Dispose();

    private void UpdateClientLatencies(object? _)
    {
        try
        {
            var state = (HostState)Socket.State;

            // Return if peers don't have any other peers to know latencies of.
            if (Manager.ConnectedPeerList.Count <= 1)
                return;

            for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
            {
                var peer = Manager.ConnectedPeerList[x];
                if (!state.ClientMap.TryGetPlayerData(peer, out var playerData))
                    continue;

                var excludeIndex = playerData.PlayerIndex;
                var latencies = state.ClientInfo.ToStructEnumerable()
                    .Where(x => x.PlayerIndex != excludeIndex, x => x)
                    .Select(x => (short)x.Latency, x => x).ToArray();

                Socket.Send(peer, ReliablePacket.Create(new HostUpdateClientLatency(latencies)), DeliveryMethod.ReliableOrdered);
            }
        }
        catch (Exception e)
        {
            _log.WriteLine($"[{nameof(LatencyUpdate)} / Host] Failed to send updated client latency. {e.Message} | {e.StackTrace}");
        }
    }

    private void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        try
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                // Update latency of client.
                var hostState = (HostState)Socket.State;
                var data = hostState.ClientMap.GetPlayerData(peer);
                data?.UpdateLatency(latency);
            }
            else
            {
                // Update ping to host.
                Socket.State.GetHostData().UpdateLatency(latency);
            }
        }
        catch (Exception e)
        {
            _log.WriteLine($"[{nameof(LatencyUpdate)}] Failed to update client latency. Index out of bounds? {e.Message} | {e.StackTrace}");
        }
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        if (Socket.GetSocketType() == SocketType.Host)
            return;

        switch (packet.MessageType)
        {
            case MessageType.HostUpdateClientPing:
            {
                var latencies = packet.GetMessage<HostUpdateClientLatency>();
                try
                {
                    // Fill in from player 2, as player 1 will be host.
                    for (int x = 0; x < latencies.NumElements; x++)
                        Socket.State.ClientInfo[x + 1].UpdateLatency(latencies.Data[x]);
                }
                catch (Exception e)
                {
                    _log.WriteLine($"[{nameof(LatencyUpdate)}] Failed to update client latency. Index out of bounds? {e.Message} | {e.StackTrace}");
                }
                break;
            }
        }
    }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
