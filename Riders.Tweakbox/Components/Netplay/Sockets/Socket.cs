﻿using System;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public abstract class Socket : IDisposable
    {
        /// <summary>
        /// Instance of the network manager.
        /// </summary>
        public NetManager Manager;

        /// <summary>
        /// Listener to network events.
        /// </summary>
        public EventListener Listener;

        /// <summary>
        /// Constructs the current socket.
        /// </summary>
        public Socket()
        {
            Listener = new EventListener(this);
            Manager = new NetManager(Listener);
        }

        /// <summary>
        /// Disposes of the current class instance.
        /// </summary>
        public virtual void Dispose() => Manager.Stop(true);

        /// <summary>
        /// True if the host is connected to at least 1 user, else false.
        /// </summary>
        public bool IsConnected() => Manager.IsRunning && Manager.ConnectedPeersCount > 0;

        /// <summary>
        /// Updates the current socket state.
        /// </summary>
        public void Poll() => Manager.PollEvents();

        /// <summary>
        /// Updates the current socket state.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// True if the socket is a host, else false.
        /// </summary>
        public abstract bool IsHost();

        /// <summary>
        /// Handles an individual reliable packet of data.
        /// </summary>
        public abstract void HandleReliablePacket(NetPeer peer, ReliablePacket packet);

        /// <summary>
        /// Handles an individual unreliable packet of data.
        /// </summary>
        public abstract void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet);

        /// <summary>
        /// New remote peer connected to host, or client connected to remote host
        /// </summary>
        /// <param name="peer">Connected peer object</param>
        public abstract void OnPeerConnected(NetPeer peer);

        /// <summary>Peer disconnected</summary>
        /// <param name="peer">disconnected peer</param>
        /// <param name="disconnectInfo">additional info about reason, errorCode or data received with disconnect message</param>
        public abstract void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);

        /// <summary>Latency information updated</summary>
        /// <param name="peer">Peer with updated latency</param>
        /// <param name="latency">latency value in milliseconds</param>
        public abstract void OnNetworkLatencyUpdate(NetPeer peer, int latency);

        /// <summary>On peer connection requested</summary>
        /// <param name="request">Request information (EndPoint, internal id, additional data)</param>
        public abstract void OnConnectionRequest(ConnectionRequest request);

        /// <summary>
        /// Waits for a message with a specified predicate
        /// </summary>
        /// <param name="peer">The peer to test.</param>
        /// <param name="predicate">Return true if packet you have waited for has returned, else false.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="sleepTime">Minimum between checks of condition.</param>
        /// <param name="token">Allows for task cancellation if necessary.</param>
        /// <returns>True if the message has been received, otherwise false.</returns>
        public bool TryWaitForMessage(NetPeer peer, Func<Packet, bool> predicate, int timeout = 1000, int sleepTime = 1, CancellationToken token = default)
        {
            bool received = false;
            Listener.OnHandlePacket += TestPacket;
            ActionWrappers.TryWaitUntil(HasReceived, timeout, 1, token);
            Listener.OnHandlePacket -= TestPacket;
            return HasReceived();

            // Message Test
            bool HasReceived() => received == true;
            void TestPacket(NetPeer eventPeer, Packet packet)
            {
                if (eventPeer.Id == peer.Id)
                    received = predicate(packet);
            }
        }

        /// <summary>
        /// Waits for a message from all given peers with a specified predicate.
        /// </summary>
        /// <param name="peers">The peers to wait for.</param>
        /// <param name="predicate">Return true if packet you have waited for has returned, else false.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="sleepTime">Minimum between checks of condition.</param>
        /// <param name="token">Allows for task cancellation if necessary.</param>
        /// <returns>True if the message has been received, otherwise false.</returns>
        public bool TryWaitForMessages(NetPeer[] peers, Func<Packet, bool> predicate, int timeout = 1000, int sleepTime = 1, CancellationToken token = default)
        {
            bool[] received = new bool[peers.Length];
            Listener.OnHandlePacket += TestPacket;
            ActionWrappers.TryWaitUntil(HasReceivedAll, timeout, 1, token);
            Listener.OnHandlePacket -= TestPacket;
            return HasReceivedAll();

            // Message Test
            bool HasReceivedAll() => received.All(x => x == true);
            void TestPacket(NetPeer eventPeer, Packet packet)
            {
                var index = peers.IndexOf(x => x.Id == eventPeer.Id);
                if (index == -1)
                    return;

                received[index] = predicate(packet);
            }
        }
    }
}