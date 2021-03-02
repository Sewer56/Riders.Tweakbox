using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.Components.Netplay.Components;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
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
        /// Host version of <see cref="State"/> or null if not applicable.
        /// </summary>
        public HostState HostState => State as HostState;

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
            Manager = new NetManager(Listener) { ChannelsCount = 64, AutoRecycle = true };
            ChannelAllocator = new LnlChannelAllocator(Manager.ChannelsCount);
            Controller = controller;
            Manager.EnableStatistics = true;
            Bandwidth = new BandwidthTracker(Manager);
            Config = config;
            Manager.PingInterval = PlayerData.LatencyUpdatePeriod;
        }

        protected void Initialize()
        {
            Manager.DisconnectTimeout = State.DisconnectTimeout;
#if DEBUG
            Manager.DisconnectTimeout = int.MaxValue;
#endif

            IoC.Kernel.Rebind<Socket>().ToConstant(this);

            // Server
            AddComponent(IoC.Get<Components.Server.ConnectionManager>());

            // Menus
            AddComponent(IoC.Get<Components.Menu.CourseSelect>());
            AddComponent(IoC.Get<Components.Menu.CharacterSelect>());
            AddComponent(IoC.Get<Components.Menu.RaceSettings>());

            // Gameplay
            AddComponent(IoC.Get<Components.Game.Attack>());
            AddComponent(IoC.Get<Components.Game.Race>());
            AddComponent(IoC.Get<Components.Game.RaceLapSync>());
            AddComponent(IoC.Get<Components.Game.RaceIntroSync>());
            AddComponent(IoC.Get<Components.Game.RacePlayerEventSync>());
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
        }

        /// <summary>
        /// Passes an <see cref="ReliablePacket"/> to be handled by all components of the Socket.
        /// </summary>
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer peer)
        {
            foreach (var component in Components.Values)
                component.HandleReliablePacket(ref packet, peer);
        }

        /// <summary>
        /// Passes an <see cref="UnreliablePacket"/> to be handled by all components of the Socket.
        /// </summary>
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer peer)
        {
            foreach (var component in Components.Values)
                component.HandleUnreliablePacket(ref packet, peer);
        }

        /// <summary>
        /// Gets the type of socket (Host/Client/Spectator) etc.
        /// </summary>
        public abstract SocketType GetSocketType();

        /// <summary>
        /// Executed at the end of each game frame.
        /// </summary>
        public void OnFrame()
        {
            State.FrameCounter += 1;
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="text">The text to log.</param>
        /// <param name="logCategory">Category under which the text should be logged.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush<T>(NetPeer exception, in T packet, DeliveryMethod method, string text, LogCategory logCategory, byte channelIndex = 0) where T : IPacket
        {
            using var serialized = packet.Serialize(out int numBytes);
            Log.WriteLine(text, logCategory);
            SendToAllExceptAndFlush(exception, serialized.Segment.Array, 0, numBytes, method, channelIndex);
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="packet">The packet to be transferred.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush<T>(NetPeer exception, in T packet, DeliveryMethod method, byte channelIndex = 0) where T : IPacket
        {
            using var serialized = packet.Serialize(out int numBytes);
            SendToAllExceptAndFlush(exception, serialized.Segment.Array, 0, numBytes, method, channelIndex);
        }

        /// <summary>
        /// Sends data to all peers except a certain peer.
        /// </summary>
        /// <param name="exception">The peer to not send data to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="start">Index of the first element of the array.</param>
        /// <param name="length">Length of the supplied array.</param>
        /// <param name="method">The channel to send the data in.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendToAllExceptAndFlush(NetPeer exception, byte[] data, int start, int length, DeliveryMethod method, byte channelIndex = 0)
        {
            for (var x = 0; x < Manager.ConnectedPeerList.Count; x++)
            {
                var peer = Manager.ConnectedPeerList[x];
                if (exception != null && peer.Id == exception.Id)
                    continue;

                peer.Send(data, start, length, channelIndex, method);
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
        public void Send<T>(NetPeer peer, in T message, DeliveryMethod method, byte channelIndex = 0) where T : IPacket
        {
            using var serialized = message.Serialize(out int numBytes);
            peer?.Send(serialized.Segment.Array, 0, numBytes, channelIndex, method);
        }

        /// <summary>
        /// Sends an individual packet to a specified peer.
        /// </summary>
        /// <param name="peer">The peer to send the message to.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The delivery method.</param>
        /// <param name="channelIndex">Index of the channel to send the message on.</param>
        public void SendAndFlush<T>(NetPeer peer, in T message, DeliveryMethod method, byte channelIndex = 0) where T : IPacket
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
        public void SendAndFlush<T>(NetPeer peer, in T message, DeliveryMethod method, string text, LogCategory logCategory, byte channelIndex = 0) where T : IPacket
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
        public void SendToAllAndFlush<T>(in T message, DeliveryMethod method, byte channelIndex = 0) where T : IPacket
        {
            using var serialized = message.Serialize(out int numBytes);
            Manager.SendToAll(serialized.Segment.Array, 0, numBytes, channelIndex, method);
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
        public void SendToAllAndFlush<T>(in T message, DeliveryMethod method, string text, LogCategory logCategory, byte channelIndex = 0) where T : IPacket
        {
            Log.WriteLine(text, logCategory);
            SendToAllAndFlush(message, method, channelIndex);
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
        /// Waits until a given condition is met.
        /// </summary>
        /// <param name="function">Waits until this condition returns true or the timeout expires.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        /// <returns>True if the condition is triggered, else false.</returns>
        public Task<bool> PollUntilAsync(Func<bool> function, int timeout, int sleepTime = 1, CancellationToken token = default)
        {
            return ActionWrappers.TryWaitUntilAsync(() =>
            {
                Update();
                return function();
            }, timeout, sleepTime, token);
        }

        /// <summary>
        /// Disconnects a client with a given message.
        /// </summary>
        /// <param name="peer">The client to disconnect.</param>
        /// <param name="message">The message to send.</param>
        public void DisconnectWithMessage(NetPeer peer, string message)
        {
            Log.WriteLine($"[Socket] Disconnecting peer with message: {message}", LogCategory.Socket);
            using var packet = ReliablePacket.Create(new Disconnect(message)).Serialize(out int numBytes);
            peer.Disconnect(packet.Segment.Array, 0, numBytes);
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
            Log.WriteLine($"[Socket] Start Time: {waitUntil}:{waitUntil.TimeOfDay.Milliseconds:000}", eventCategory);

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