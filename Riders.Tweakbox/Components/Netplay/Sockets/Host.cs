using System;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        public string Password { get; private set; }
        public TaskTracker Tracker { get; private set; } = IoC.GetConstant<TaskTracker>();
        public PlayerMap<PlayerState> PlayerMap { get; private set; } = new PlayerMap<PlayerState>();
        public MenuDeltaTracker Delta { get; private set; } = IoC.GetConstant<MenuDeltaTracker>();
        public EventHook Event { get; private set; } = IoC.GetConstant<EventHook>();
        public ClientState State { get; private set; } = new ClientState();
        private int _syncTimeout = 2000;

        public Host(int port, string password, NetplayController controller) : base(controller)
        {
            Debug.WriteLine($"[Host] Hosting Server on {port} with password {password}");

#if DEBUG
            _syncTimeout = 5000;
            Manager.DisconnectTimeout = int.MaxValue;
            Manager.SimulateLatency = false;
            Manager.SimulationMinLatency = 1000;
            Manager.SimulationMaxLatency = 1000;
#endif

            Manager.UpdateTime = 1;
            Password = password;
            Manager.Start(port);

            Event.OnSkipIntro += WaitForClientReadySignalsAndGo;
            Event.OnCheckIfSkipIntro += SkipIntroIfRequested;
            Event.OnStartRace += OnStartRace;
            Event.OnCheckIfStartRace += CheckIfStartRace;
        }

        public override unsafe void Dispose()
        {
            Manager.DisconnectAll();
            base.Dispose();

            Event.OnCheckIfSkipIntro -= SkipIntroIfRequested;
            Event.OnSkipIntro -= WaitForClientReadySignalsAndGo;
            Event.OnStartRace -= OnStartRace;
            Event.OnCheckIfStartRace -= CheckIfStartRace;
        }

        public override bool IsHost() => true;
        public override void Update()
        {
            switch (Tracker.LastTask)
            {
                case Tasks.CharacterSelect:
                    UpdateCharacterSelect();
                    break;
                case Tasks.CourseSelect:
                    UpdateCourseSelect();
                    break;
                case Tasks.RaceRules:
                    UpdateRaceRules();
                    break;
                case Tasks.Race:
                    UpdateRace();
                    break;
            }
        }

        private void UpdateRace()
        {

        }

        private void UpdateCharacterSelect()
        {
            var task = Tracker.CharacterSelect;
            var data = PlayerMap.GetData();

            // Get All Chara Loops in Player Order
            var allCharaLoops = new CharaSelectLoop[Constants.MaxNumberOfPlayers];
            allCharaLoops[0]  = CharaSelectLoop.FromGame(task);

            foreach (var dat in data) 
                allCharaLoops[dat.Host.PlayerIndex] = dat.Custom.CharaSelectLoop;

            // Apply Sync Message to Self
            var charaLoopSpan = allCharaLoops.AsSpan();
            new CharaSelectSync(charaLoopSpan.Slice(1).ToArray()).ToGame(task);

            // Send personalized message to each peer with everyone except them.
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var excludeIndex = PlayerMap.GetPlayerData(peer).PlayerIndex;
                var selectSync  = new CharaSelectSync(allCharaLoops.Where((loop, x) => x != excludeIndex).ToArray());
                var syncMessage = new MenuSynchronizationCommand(selectSync);
                var message     = new ReliablePacket() { MenuSynchronizationCommand = syncMessage };

                if (selectSync.ContainsExit())
                    peer.Send(message.Serialize(), DeliveryMethod.ReliableOrdered);
                else
                    peer.Send(message.Serialize(), DeliveryMethod.ReliableSequenced);

                peer.Flush();
            }
        }

        private void UpdateRaceRules()
        {
            var task = Tracker.RaceRules;

            // Calculate Sync Data based off of Client Info
            var allData = PlayerMap.GetCustomData();
            var loop = new RuleSettingsLoop();
            foreach (var data in allData)
                loop = loop.Add(data.GetEraseRuleSettingsLoop());

            if (!loop.IsDefault() || !Delta.Rule.IsDefault())
            {
                var sync = RuleSettingsSync.FromGame(task);
                sync.Merge(loop);
                sync.ToGame(task);

                // Send to all peers.
                var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(sync) };
                Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.Flush();
            }
        }

        private unsafe void UpdateCourseSelect()
        {
            var task = Tracker.CourseSelect;
           
            // Exit if character disconnected.
            if (task->TaskStatus == CourseSelectTaskState.Closing)
            {
                Dispose();
                return;
            }

            // Calculate Sync Data based off of Client Info
            var allData = PlayerMap.GetCustomData();
            var loop    = new CourseSelectLoop();
            foreach (var data in allData)
                loop = loop.Add(data.GetEraseCourseLoop());

            if (!loop.IsDefault() || !Delta.Course.IsDefault())
            {
                var sync = CourseSelectSync.FromGame(task);
                sync.Merge(loop);
                sync.ToGame(task);

                var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(sync) };
                Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.Flush();
            }
            
            // Cleanup for next frame.
            // Nothing to do here.
        }

        private void HandleMenuSyncMessage(NetPeer peer, MenuSynchronizationCommand syncCommand)
        {
            var data = PlayerMap.GetCustomData(peer);
            data.SetLoopCommand(syncCommand.Command, State.StartRequested);

            if (syncCommand.Command is CharaSelectStart startCommand)
            {
                Debug.WriteLine("[Host] Got Start Request Flag from Client");
                State.StartRequested = true;
                var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(new CharaSelectStart()) };
                SendToAllExcept(peer, serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
            }
        }

        private void HandleReliableRace(NetPeer peer, ReliablePacket packet)
        {
            if (packet.HasSyncStartSkip)
            {
                State.SkipRequested = true;
                Debug.WriteLine("[Host] Received Skip from Client, Rebroadcasting.");
                var serverMessage = new ReliablePacket() { HasSyncStartSkip = true };
                SendToAllExcept(peer, serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
            }

            if (packet.HasSyncStartReady)
            {
                Debug.WriteLine("[Host] Received HasSyncStartReady from Client.");
                var customData = PlayerMap.GetCustomData(peer);
                customData.ReadyToStartRace = true;
            }
        }

        private void HandleServerMessage(NetPeer peer, ServerMessage serverMessage)
        {
            var message = serverMessage.Message;
            switch (message)
            {
                case ClientSetPlayerData clientSetPlayerData:
                    PlayerMap.AddOrUpdatePlayerData(peer, clientSetPlayerData.Data);
                    UpdatePlayerMap();
                    break;
            }
        }

        /// <summary>
        /// Sends a personalized player map (excluding the player). 
        /// </summary>
        public void UpdatePlayerMap()
        {
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var message = PlayerMap.ToMessage(peer);
                var serverMessage = new ServerMessage(message);
                var reliableMessage = new ReliablePacket() { ServerMessage = serverMessage };
                peer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableUnordered);
                peer.Flush();
            }
        }

        /// <summary>
        /// Sends game data to a peer one time.
        /// </summary>
        private void SendGameData(NetPeer peer)
        {
            var gameData = GameData.FromGame();
            var serverMessage = new ReliablePacket() { GameData = gameData };
            peer.Send(serverMessage.Serialize(), DeliveryMethod.ReliableUnordered);
            peer.Flush();
        }

        #region Events
        private void WaitForClientReadySignalsAndGo()
        {
            // Send skip signal to clients.
            if (!State.SkipRequested)
            {
                Debug.WriteLine("[Host] Broadcasting Skip Signal.");
                var serverMessage = new ReliablePacket() { HasSyncStartSkip = true };
                Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.Flush();
            }

            State.SkipRequested = false;
            Debug.WriteLine("[Host] Waiting for ready messages.");
            // Note to self: Don't use wait for all clients, because the messages may have already been sent by the clients.
            if (!ActionWrappers.TryWaitUntil(TestAllReady, _syncTimeout * 2))
            {
                Debug.WriteLine("[Host] It's no use, let's get outta here!.");
                Manager.DisconnectAll();
                return;
            }

            // Send go signal to players and wait.
            Debug.WriteLine("[Host] Sending GO signal.");
            var secondFromNow = DateTime.UtcNow.AddSeconds(1);
            var reliableMessage = new ReliablePacket() { SyncStartGo = new SyncStartGo(secondFromNow) };
            Manager.SendToAll(reliableMessage.Serialize(), DeliveryMethod.ReliableOrdered);
            Manager.Flush();

            // Disable skip flags for everyone.
            var data = PlayerMap.GetCustomData();
            foreach (var dt in data)
                dt.ReadyToStartRace = false;

            Wait(secondFromNow);
            Debug.WriteLine("[Host] Race Started.");

            // Helper Functions.
            void Wait(DateTime startTime)
            {
                Debug.WriteLine("[Host] Waiting for race start.");
                Debug.WriteLine($"[Host] Time: {DateTime.UtcNow}");
                Debug.WriteLine($"[Host] Start Time: {startTime}");

                ActionWrappers.TryWaitUntil(() => DateTime.UtcNow > startTime, int.MaxValue);
            }

            bool TestAllReady()
            {
                Manager.PollEvents();
                Manager.Flush();
                return PlayerMap.GetCustomData().All(x => x.ReadyToStartRace);
            }
        }

        
        private void OnStartRace()
        {
            // We skipped the intro ourselves, pass the news along to the clients. 
            if (! State.StartRequested)
            {
                Debug.WriteLine("[Host] Sending Race Start Flag to Clients");
                var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(new CharaSelectStart()) };
                Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.Flush();
            }
            else
            {
                // Start triggered by request from client.
                State.StartRequested = false;
                Debug.WriteLine("[Host] Start triggered by request from client");
            }
        }

        private bool CheckIfStartRace() => State.StartRequested;
        private bool SkipIntroIfRequested() => State.SkipRequested;
        #endregion

        #region Overrides
        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            if (packet.MenuSynchronizationCommand != null) 
                HandleMenuSyncMessage(peer, packet.MenuSynchronizationCommand.Value);

            if (packet.ServerMessage != null)
                HandleServerMessage(peer, packet.ServerMessage.Value);

            HandleReliableRace(peer, packet);
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            // TODO: [Host] Race / Handle Unreliable Packets
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            // Wait for user data acknowledgement.
            Debug.WriteLine($"[Host] Client {peer.EndPoint.Address} | {peer.Id}, waiting for message.");
            if (!TryWaitForMessage(peer, CheckIfUserData, _syncTimeout))
            {
                Debug.WriteLine($"[Host] Disconnecting client, did not receive user data.");
                peer.Disconnect();
                return;
            }

            // Player map updated in handler.
            Debug.WriteLine($"[Host] Received user data, uploading game data.");
            SendGameData(peer);
            bool CheckIfUserData(Packet<NetPeer> packet) => packet.Reliable?.ServerMessage?.MessageKind == ServerMessageType.ClientSetPlayerData;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PlayerMap.RemovePeer(peer);
            UpdatePlayerMap();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency.
            var data = PlayerMap.GetCustomData(peer);
            if (data != null)
                data.Latency = latency;
        }

        public override void OnConnectionRequest(ConnectionRequest request)
        {
            // Reject if not in course select menu.
            Debug.WriteLine($"[Host] Received Connection Request | Last Task {Tracker.LastTask}");
            if (Tracker.LastTask != Tasks.CourseSelect)
            {
                request.Reject();
                Debug.WriteLine($"[Host] Rejected Connection | Not on Course Select");
                return;
            }

            if (!PlayerMap.HasEmptySlots())
            {
                request.Reject();
                Debug.WriteLine($"[Host] Rejected Connection | No Empty Slots");
                return;
            }

            // Accept on password.
            Debug.WriteLine($"[Host] Accepting if Password Matches");
            request.AcceptIfKey(Password);
        }
        #endregion
    }
}
