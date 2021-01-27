using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Components;
using Riders.Tweakbox.Components.Netplay.Components.Misc;
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
        public Dictionary<Type, INetplayComponent> Components = new Dictionary<Type, INetplayComponent>();

        /// <summary>
        /// Provides C# events for in-game events such as changing a menu value.
        /// </summary>
        public EventController Event = IoC.Get<EventController>();

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
        /// Used for allocating channels for reliable packets.
        /// </summary>
        public LnlChannelAllocator ChannelAllocator;

        /// <summary>
        /// The netplay configuration with which this socket was created with.
        /// </summary>
        public NetplayConfig Config { get; set; }

        private Stopwatch _stopWatch = new Stopwatch();
        private bool _isDisposed = false;

        /// <summary>
        /// Constructs the current socket.
        /// </summary>
        public Socket(NetplayController controller, NetplayConfig config)
        {
            _stopWatch.Start();
            Listener = new NetworkEventListener(this);
            Manager = new NetManager(Listener) { ChannelsCount = 64 };
            ChannelAllocator = new LnlChannelAllocator(Manager.ChannelsCount);
            Controller = controller;
            Manager.EnableStatistics = true;
            Bandwidth = new BandwidthTracker(Manager);
            Config = config;

#if DEBUG
            Manager.DisconnectTimeout = int.MaxValue;

            var badInternet = config.Data.BadInternet;
            if (badInternet.IsEnabled)
            {
                if (badInternet.PacketLoss > 0 && badInternet.PacketLoss <= 100)
                {
                    Manager.SimulatePacketLoss = true;
                    Manager.SimulationPacketLossChance = badInternet.PacketLoss;

                    Log.WriteLine($"Simulating Packet Loss (bool/Loss): {Manager.SimulatePacketLoss}, {Manager.SimulationPacketLossChance}", LogCategory.Socket);
                }

                if (badInternet.MinLatency > 0 && badInternet.MaxLatency > badInternet.MinLatency)
                {
                    Manager.SimulateLatency = true;
                    Manager.SimulationMaxLatency = badInternet.MaxLatency;
                    Manager.SimulationMinLatency = badInternet.MinLatency;

                    Log.WriteLine($"Simulating Latency (bool/Min/Max): {Manager.SimulateLatency}, {Manager.SimulationMinLatency}, {Manager.SimulationMaxLatency}", LogCategory.Socket);
                }
            }
#endif
        }

        protected void Initialize()
        {
            IoC.Kernel.Rebind<Socket>().ToConstant(this);

            // Menus
            AddComponent(IoC.Get<Components.Menu.CourseSelect>());
            AddComponent(IoC.Get<Components.Menu.CharacterSelect>());
            AddComponent(IoC.Get<Components.Menu.RaceSettings>());

            // Gameplay
            AddComponent(IoC.Get<Components.Game.Attack>());
            AddComponent(IoC.Get<Components.Game.Race>());
            AddComponent(IoC.Get<Components.Game.RaceLapSync>());
            AddComponent(IoC.Get<Components.Game.RaceIntroSync>());
            AddComponent(IoC.Get<Components.Game.SetupRace>());

            // Misc
            AddComponent(IoC.Get<Components.Misc.TimeSynchronization>());
            AddComponent(IoC.Get<Components.Misc.Random>());
            AddComponent(IoC.Get<Components.Misc.LatencyUpdate>());
        }

        /// <summary>
        /// Disposes of the current class instance.
        /// </summary>
        public virtual void Dispose()
        {
            _isDisposed = true;
            foreach (var component in Components.Values)
                component.Dispose();

            Controller.Socket = null;
            Manager.Stop(true);
            Listener.Dispose();
        }

        /// <summary>
        /// True if the host is connected to at least 1 user, else false.
        /// </summary>
        public bool IsConnected() => Manager.IsRunning && Manager.ConnectedPeersCount > 0;

        /// <summary>
        /// Updates the current socket state.
        /// </summary>
        public void Update()
        {
            if (_isDisposed) 
                return;

            var elapsedMilliseconds = (int)_stopWatch.ElapsedMilliseconds != 0 ? _stopWatch.ElapsedMilliseconds : 1;
            Manager.ManualReceive();
            Manager.ManualUpdate((int) elapsedMilliseconds);
            _stopWatch.Restart();
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
                    Log.WriteLine($"[Socket] Discarding Unknown Packet Due to Latency", LogCategory.Socket);
                }
                else
                {
                    HandlePacket(packet);
                    foreach (var component in Components.Values)
                        component.HandlePacket(packet);
                }

                // Dispose of packet contents.
                packet.Value.Value.Dispose();
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

        /// <summary>On peer connection requested</summary>
        /// <param name="request">Request information (EndPoint, internal id, additional data)</param>
        /// <returns>Ignored, just there to reduce code count.</returns>
        public abstract bool OnConnectionRequest(ConnectionRequest request);

        /// <summary>
        /// Executed at the end of each game frame.
        /// </summary>
        public virtual void OnFrame()
        {
            State.FrameCounter += 1;
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="text">The text to print to the console.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush(NetPeer exception, ReliablePacket packet, DeliveryMethod method, string text, byte channelIndex = 0)
        {
            Trace.WriteLine(text);
            SendToAllExceptAndFlush(exception, packet.Serialize(), method, channelIndex);
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush(NetPeer exception, IPacket packet, DeliveryMethod method, byte channelIndex = 0) => SendToAllExceptAndFlush(exception, packet.Serialize(), method, channelIndex);

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush(NetPeer exception, byte[] data, DeliveryMethod method, byte channelIndex = 0)
        {
            for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
            {
                var peer = Manager.ConnectedPeerList[x];
                if (exception != null && peer.Id == exception.Id)
                    continue;

                peer.Send(data, channelIndex, method);
            }

            Update();
        }

        /// <summary>
        /// Sends an individual packet to a specified peer.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void Send(NetPeer peer, IPacket message, DeliveryMethod method, byte channelIndex = 0)
        {
            peer?.Send(message.Serialize(), channelIndex, method);
        }

        /// <summary>
        /// Sends an individual packet to a specified peer.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendAndFlush(NetPeer peer, IPacket message, DeliveryMethod method, byte channelIndex = 0)
        {
            Send(peer, message, method, channelIndex);
            Update();
        }

        /// <summary>
        /// Sends an individual packet to a specified peer, and logs a given message to the configured trace listeners.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="text">The text to log.</param>
        /// <param name="logCategory">Category under which the text should be logged.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendAndFlush(NetPeer peer, IPacket message, DeliveryMethod method, string text, LogCategory logCategory, byte channelIndex = 0)
        {
            Log.WriteLine(text, logCategory);
            SendAndFlush(peer, message, method, channelIndex);
        }

        /// <summary>
        /// Sends an individual packet to all peers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllAndFlush(IPacket message, DeliveryMethod method, byte channelIndex = 0)
        {
            Manager.SendToAll(message.Serialize(), channelIndex, method);
            Update();
        }

        /// <summary>
        /// Sends an individual packet to all peers and logs a given message to the configured trace listeners.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="text">The text to log.</param>
        /// <param name="logCategory">Category under which the text should be logged.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllAndFlush(IPacket message, DeliveryMethod method, string text, LogCategory logCategory, byte channelIndex = 0)
        {
            Log.WriteLine(text, logCategory);
            SendToAllAndFlush(message, method, channelIndex);
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
            var result = ActionWrappers.TryWaitUntil(HasReceived, timeout, sleepTime, token);
            Listener.OnQueuePacket -= TestPacket;
            return result;

            // Message Test
            bool HasReceived()
            {
                Update();
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
            var result = ActionWrappers.TryWaitUntil(HasReceivedAll, timeout, sleepTime, token);
            Listener.OnQueuePacket -= TestPacket;
            return result;

            // Message Test
            bool HasReceivedAll()
            {
                Update();
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
                Update();
                return function();
            }, timeout, sleepTime, token);
        }

        /// <summary>
        /// Waits for a specific event to occur.
        /// </summary>
        /// <param name="waitUntil">Thread will block until this specified time.</param>
        /// <param name="eventName">(Optional) name of the event</param>
        /// <param name="eventCategory">(Optional) Category of the event.</param>
        /// <param name="spinTime">The amount of time in milliseconds before event expired to start spinning.</param>
        public void WaitWithSpin(DateTime waitUntil, string eventName = "", LogCategory eventCategory = LogCategory.Socket, int spinTime = 100)
        {
            // TODO: Negotiation between multiple clients on current time so WaitUntil matches.
            Log.WriteLine($"[Socket] Waiting for event ({eventName}).", eventCategory);
            Log.WriteLine($"[Socket] Time: {DateTime.UtcNow}", eventCategory);
            Log.WriteLine($"[Socket] Start Time: {waitUntil}", eventCategory);

            // TODO: More accurate waiting. This isn't frame perfect and subject to thread context switch.
            ActionWrappers.TryWaitUntil(() =>
            {
                // Check if already done.
                var timeLeft = waitUntil - DateTime.UtcNow;
                if (timeLeft.Milliseconds < 0)
                    return true;

                // Check if should sleep.
                if (timeLeft.Milliseconds > spinTime) 
                    return false;

                // Spin for remaining time.
                do { timeLeft = waitUntil - DateTime.UtcNow; } 
                while (timeLeft.Ticks > 0);

                return true;
            }, int.MaxValue);
        }

        /// <summary>
        /// Tries to retrieve a component of a specified type from the socket.
        /// </summary>
        public bool TryGetComponent<TComponent>(out TComponent value) where TComponent : INetplayComponent
        {
            var result = Components.TryGetValue(typeof(TComponent), out var val);
            value = (TComponent) val;
            return result;
        }

        /// <summary>
        /// Adds an individual component to this socket.
        /// </summary>
        private void AddComponent<TComponent>(TComponent value) where TComponent : INetplayComponent
        {
            Components[typeof(TComponent)] = value;
        }
    }
}