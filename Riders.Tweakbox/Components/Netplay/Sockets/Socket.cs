using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Components;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public abstract class Socket : IDisposable
    {
        /// <summary>
        /// Instance of the network manager.
        /// </summary>
        public NetManager Manager;

        /// <summary>
        /// Individual Netplay components associated with this socket.
        /// </summary>
        public INetplayComponent[] Components;

        /// <summary>
        /// Provides C# events for in-game events such as changing a menu value.
        /// </summary>
        public EventController Event = IoC.GetConstant<EventController>();

        /// <summary>
        /// Provides shared state for the client and host.
        /// </summary>
        public CommonState State;

        /// <summary>
        /// Contains a queue of all packets received from the clients.
        /// </summary>
        public ConcurrentQueue<Packet<NetPeer>> Queue = new ConcurrentQueue<Packet<NetPeer>>();

        /// <summary>
        /// Listener to network events.
        /// </summary>
        public NetworkEventListener Listener;

        /// <summary>
        /// Controller owning this socket.
        /// </summary>
        public NetplayController Controller;

        /// <summary>
        /// Gets the bandwidth used per second by the socket.
        /// </summary>
        public BandwidthTracker Bandwidth;

        /// <summary>
        /// Constructs the current socket.
        /// </summary>
        public Socket(NetplayController controller)
        {
            Listener = new NetworkEventListener(this);
            Manager = new NetManager(Listener);
            Controller = controller;
            Manager.EnableStatistics = true;
            Bandwidth = new BandwidthTracker(Manager);

            #if DEBUG
            Manager.DisconnectTimeout = int.MaxValue;
            State.HandshakeTimeout = 5000;
            #endif
            
            Components = new INetplayComponent[]
            {
                // Menus
                IoC.Get<Components.Menu.CourseSelect>(),
                IoC.Get<Components.Menu.CharacterSelect>(),
                IoC.Get<Components.Menu.RaceSettings>(),

                // Gameplay
                IoC.Get<Components.Game.Attack>(),
                IoC.Get<Components.Game.Race>(),
                IoC.Get<Components.Game.RaceEvents>(),
                IoC.Get<Components.Game.RaceStartSync>(),
                IoC.Get<Components.Game.SetupRace>(),

                // Misc
                IoC.Get<Components.Misc.Random>(),
            };
        }

        /// <summary>
        /// Disposes of the current class instance.
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var component in Components)
                component.Dispose();

            Controller.Socket = null;
            Manager.Stop(true);
        }

        /// <summary>
        /// True if the host is connected to at least 1 user, else false.
        /// </summary>
        public bool IsConnected() => Manager.IsRunning && Manager.ConnectedPeersCount > 0;

        /// <summary>
        /// Updates the current socket state.
        /// </summary>
        public void Poll()
        {
            Manager.Flush();
            Manager.PollEvents();
            HandlePackets();
        }

        /// <summary>
        /// Dequeues all packets and sends them to the appropriate packet handler.
        /// </summary>
        public void HandlePackets()
        {
            while (Queue.TryDequeue(out var packet))
            {
                if (packet.IsDiscard(State.MaxLatency))
                {
                    Debug.WriteLine($"[Socket] Discarding Unknown Packet Due to Latency");
                    continue;
                }

                HandlePacket(packet);
                foreach (var component in Components)
                    component.HandlePacket(packet);
            }
        }

        /// <summary>
        /// Handles an individual packet of data.
        /// </summary>
        /// <param name="packet">The packet in question.</param>
        public abstract void HandlePacket(Packet<NetPeer> packet);

        /// <summary>
        /// Gets the type of socket (Host/Client/Spectator) etc.
        /// </summary>
        public abstract SocketType GetSocketType();

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
        /// <returns>Ignored, just there to reduce code count.</returns>
        public abstract bool OnConnectionRequest(ConnectionRequest request);

        /// <summary>
        /// Executed at the end of each game frame.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="text">The text to print to the console.</param>
        public void SendToAllExcept(NetPeer exception, ReliablePacket packet, DeliveryMethod method, string text)
        {
            Debug.WriteLine(text);
            SendToAllExcept(exception, packet.Serialize(), method);
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        public void SendToAllExcept(NetPeer exception, IPacket packet, DeliveryMethod method) => SendToAllExcept(exception, packet.Serialize(), method);

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="method">The channel to send the data in.</param>
        public void SendToAllExcept(NetPeer exception, byte[] data, DeliveryMethod method)
        {
            foreach (var peer in Manager.ConnectedPeerList)
            {
                if (exception != null && peer.Id == exception.Id)
                    continue;

                peer.Send(data, method);
                peer.Flush();
            }
        }

        /// <summary>
        /// Sends an individual packet to a specified peer.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        public void SendAndFlush(NetPeer peer, IPacket message, DeliveryMethod method)
        {
            if (peer == null)
                return;

            peer.Send(message.Serialize(), method);
            peer.Flush();
        }

        /// <summary>
        /// Sends an individual packet to a specified peer, and logs a given message to the configured trace listeners.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="text">The text to log.</param>
        public void SendAndFlush(NetPeer peer, IPacket message, DeliveryMethod method, string text)
        {
            if (peer == null)
                return;

            Debug.WriteLine(text);
            SendAndFlush(peer, message, method);
        }

        /// <summary>
        /// Sends an individual packet to all peers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        public void SendToAllAndFlush(IPacket message, DeliveryMethod method)
        {
            Manager.SendToAll(message.Serialize(), method);
            Manager.Flush();
        }

        /// <summary>
        /// Sends an individual packet to all peers and logs a given message to the configured trace listeners.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="text">The text to log.</param>
        public void SendToAllAndFlush(IPacket message, DeliveryMethod method, string text)
        {
            Debug.WriteLine(text);
            SendToAllAndFlush(message, method);
        }

        /// <summary>
        /// Waits for a message with a specified predicate
        /// </summary>
        /// <param name="peer">The peer to test.</param>
        /// <param name="predicate">Return true if packet you have waited for has returned, else false.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="sleepTime">Minimum between checks of condition.</param>
        /// <param name="token">Allows for task cancellation if necessary.</param>
        /// <returns>True if the message has been received, otherwise false.</returns>
        public bool TryWaitForMessage(NetPeer peer, Func<Packet<NetPeer>, bool> predicate, int timeout = 1000, int sleepTime = 1, CancellationToken token = default)
        {
            bool received = false;
            Listener.OnQueuePacket += TestPacket;
            var result = ActionWrappers.TryWaitUntil(HasReceived, timeout, 1, token);
            Listener.OnQueuePacket -= TestPacket;
            return result;

            // Message Test
            bool HasReceived()
            {
                Poll();
                return received == true;
            }

            void TestPacket(Packet<NetPeer> packet)
            {
                if (packet.Source.Id != peer.Id)
                    return;

                if (predicate(packet))
                    received = true;
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
        public bool TryWaitForMessages(NetPeer[] peers, Func<Packet<NetPeer>, bool> predicate, int timeout = 1000, int sleepTime = 1, CancellationToken token = default)
        {
            bool[] received = new bool[peers.Length];
            Listener.OnQueuePacket += TestPacket;
            var result = ActionWrappers.TryWaitUntil(HasReceivedAll, timeout, 1, token);
            Listener.OnQueuePacket -= TestPacket;
            return result;

            // Message Test
            bool HasReceivedAll()
            {
                var peer = peers.FirstOrDefault();
                peer?.NetManager.PollEvents();
                peer?.NetManager.Flush();
                return received.All(x => x == true);
            }

            void TestPacket(Packet<NetPeer> packet)
            {
                var index = peers.IndexOf(x => x.Id == packet.Source.Id);
                if (index == -1)
                    return;

                if (predicate(packet))
                    received[index] = true;
            }
        }

        /// <summary>
        /// Waits until a given condition is met.
        /// </summary>
        /// <param name="function">Waits until this condition returns true or the timeout expires.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        /// <returns>True if the condition is triggered, else false.</returns>
        public bool PollUntil(Func<bool> function, int timeout, int sleepTime = 1, CancellationToken token = default)
        {
            return ActionWrappers.TryWaitUntil(() =>
            {
                Poll();
                return function();
            }, timeout, sleepTime, token);
        }

        /// <summary>
        /// Waits for a specific event to occur.
        /// </summary>
        /// <param name="waitUntil">Thread will block until this specified time.</param>
        /// <param name="eventName">(Optional) name of the event</param>
        public void Wait(DateTime waitUntil, string eventName = "")
        {
            Debug.WriteLine($"[Socket] Waiting for event ({eventName}).");
            Debug.WriteLine($"[Socket] Time: {DateTime.UtcNow}");
            Debug.WriteLine($"[Socket] Start Time: {waitUntil}");
            ActionWrappers.TryWaitUntil(() => DateTime.UtcNow > waitUntil, int.MaxValue);
        }
    }
}