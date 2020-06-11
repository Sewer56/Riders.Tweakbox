using System;
using System.Collections.Concurrent;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Debug = System.Diagnostics.Debug;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public TaskTracker Tracker { get; private set; } = IoC.GetConstant<TaskTracker>();
        public ConcurrentQueue<Packet<NetPeer>> PacketQueue { get; private set; } = new ConcurrentQueue<Packet<NetPeer>>();
        public ClientState State { get; private set; } = new ClientState();
        public MenuDeltaTracker Delta { get; private set; } = IoC.GetConstant<MenuDeltaTracker>();
        public EventHook Event { get; private set; } = IoC.GetConstant<EventHook>();
        private int _syncTimeout = 2000;
        
        public Client(string ipAddress, int port, string password, NetplayController controller) : base(controller)
        {
            Debug.WriteLine($"[Client] Joining Server on {ipAddress}:{port} with password {password}");


#if DEBUG
            Manager.DisconnectTimeout = int.MaxValue;
            _syncTimeout = 5000;
            Manager.SimulateLatency = false;
            Manager.SimulationMinLatency = 1000;
            Manager.SimulationMaxLatency = 1000;
#endif

            Manager.UpdateTime = 1;
            Manager.Start();
            Manager.Connect(ipAddress, port, password);

            // Add undo menu movement when connected.
            Delta.OnCourseSelectUpdated += DeltaOnCourseSelectUpdated;
            Delta.OnRuleSettingsUpdated += DeltaOnRuleSettingsUpdated;
            Event.OnSkipIntro += SendReadyOnSkipIntro;
            Event.OnCheckIfSkipIntro += SkipIntroIfRequestedByHost;
            Event.OnCharaSelect += OnCharaSelect;
            Event.OnStartRace += OnStartRace;
            Event.OnCheckIfStartRace += CheckIfStartRace;
        }

        public override void Dispose()
        {
            Debug.WriteLine($"[Client] Disposing of Socket, Disconnected");
            base.Dispose();
            Delta.OnCourseSelectUpdated -= DeltaOnCourseSelectUpdated;
            Delta.OnRuleSettingsUpdated -= DeltaOnRuleSettingsUpdated;
            Event.OnSkipIntro -= SendReadyOnSkipIntro;
            Event.OnCheckIfSkipIntro -= SkipIntroIfRequestedByHost;
            Event.OnCharaSelect -= OnCharaSelect;
            Event.OnStartRace -= OnStartRace;
            Event.OnCheckIfStartRace -= CheckIfStartRace;
        }

        public override bool IsHost() => false;
        public override void Update()
        {
            State.FrameCounter += 1;
            if (Manager.ConnectedPeersCount <= 0)
                return;

            // Process received packets.
            while (PacketQueue.TryDequeue(out var result))
            {
                if (result.Reliable.HasValue)
                    HandleReliable(result.Reliable.Value);

                if (result.Unreliable.HasValue)
                    HandleUnreliable(result.Unreliable.Value);
            }

            // Send update packets.
            switch (Tracker.LastTask)
            {
                case Tasks.CharacterSelect:
                    UpdateCharSelect();
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

        private void UpdateRaceRules()
        {
            var raceRules = Delta.Rule;
            if (!raceRules.IsDefault())
            {
                Debug.WriteLine($"[Client] Sending Race Rules Delta");
                var reliableMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(raceRules) };
                Manager.FirstPeer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.FirstPeer.Flush();
            }
        }

        private unsafe void UpdateCourseSelect()
        {
            var courseSelectLoop  = Delta.Course;
            if (!courseSelectLoop.IsDefault())
            {
                Debug.WriteLine($"[Client] Sending Course Select Delta");
                var reliableMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(courseSelectLoop) };
                Manager.FirstPeer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.FirstPeer.Flush();
            }
        }

        private unsafe void UpdateCharSelect()
        {
            var charSelectLoop = CharaSelectLoop.FromGame(Tracker.CharacterSelect);
            var reliableMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(charSelectLoop) };

            if (charSelectLoop.IsExitingMenu)
                Manager.FirstPeer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableOrdered);
            else
                Manager.FirstPeer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableSequenced);

            Manager.FirstPeer.Flush();
        }

        #region Server Message Handling
        private void HandleUnreliable(UnreliablePacket resultUnreliable)
        {

        }

        private void HandleReliable(ReliablePacket resultReliable)
        {
            if (resultReliable.MenuSynchronizationCommand.HasValue)
                HandleMenu(resultReliable.MenuSynchronizationCommand.Value);

            HandleReliableRace(resultReliable);
        }

        private void HandleReliableRace(ReliablePacket resultReliable)
        {
            // See Events for Go event.
            if (resultReliable.HasSyncStartSkip)
                State.SkipRequested = true;
        }

        private unsafe void HandleMenu(MenuSynchronizationCommand syncCommand)
        {
            var cmd = syncCommand.Command;
            switch (cmd)
            {
                case CharaSelectSync charaSelectSync:
                    if (!State.StartRequested)
                        State.CharaSelectSync.Enqueue(charaSelectSync);
                    else
                        Debug.WriteLine("[Client] Dropping Character Select Packet: Starting Race");
                    break;

                case CourseSelectSync courseSelectSync:
                    courseSelectSync.ToGame(Tracker.CourseSelect);
                    break;

                case RuleSettingsSync ruleSettingsSync:
                    ruleSettingsSync.ToGame(Tracker.RaceRules);
                    break;

                case CharaSelectStart charaSelectStart:
                    Debug.WriteLine("[Client] Got Start Request Flag from Host");
                    State.StartRequested = true;
                    break;
            }
        }

        private void HandleServerMessage(NetPeer peer, ServerMessage serverMessage)
        {
            var msg = serverMessage.Message;
            switch (msg)
            {
                case HostSetPlayerData hostSetPlayerData:
                    Debug.WriteLine($"[Client] Received Player Info");
                    State.PlayerInfo = hostSetPlayerData.Data;
                    break;

                case SetAntiCheat setAntiCheat:
                    Debug.WriteLine($"[Client] Received Anticheat Info");
                    State.AntiCheatMode = setAntiCheat.Cheats;
                    break;
            }
        }
        #endregion

        #region Events
        // This pair of methods undoes any movement done by the user in the menus.
        // We will instead receive the movements from the host and only send the menu inputs/differences over.
        // This incurs input lag but at least looks consistent.
        private unsafe void DeltaOnRuleSettingsUpdated(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (Manager.ConnectedPeersCount > 0)
                loop.Undo(task);
        }

        private unsafe void DeltaOnCourseSelectUpdated(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (Manager.ConnectedPeersCount > 0)
                loop.Undo(task);
        }

        private void OnCharaSelect(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            while (State.CharaSelectSync.TryDequeue(out var result))
            {
                result.ToGame(task);   
            }
        }

        private bool CheckIfStartRace() => State.StartRequested;

        private void OnStartRace()
        {
            if (!State.StartRequested)
            {
                // We started ourselves, tell host to rebroadcast.
                Debug.WriteLine("[Client] Sending Start flag to Host");
                var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(new CharaSelectStart()) };
                Manager.FirstPeer.Send(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.FirstPeer.Flush();
            }
            else
            {
                // Start triggered by request from host.
                State.StartRequested = false;
            }
        }

        private void SendReadyOnSkipIntro()
        {
            if (! State.SkipRequested)
            {
                Debug.WriteLine("[Client] Skipped intro ourselves, sending skip notification to host.");
                var msg = new ReliablePacket() { HasSyncStartSkip = true };
                Manager.FirstPeer.Send(msg.Serialize(), DeliveryMethod.ReliableOrdered);
                Manager.FirstPeer.Flush();
            }

            State.SkipRequested = false;
            Debug.WriteLine("[Client] Sending HasSyncStartReady.");
            var reliableMessage = new ReliablePacket() { HasSyncStartReady = true };
            Manager.FirstPeer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableOrdered);
            Manager.FirstPeer.Flush();

            // Otherwise wait for a message from the host.
            if (!PollUntil(IsGo, _syncTimeout * 2))
            {
                Debug.WriteLine("[Client] No Go Signal Received, Bailing Out!.");
                Dispose();
                return;
            }

            Success();

            // Wait for response.
            void Success()
            {
                Debug.WriteLine("[Client] Race Started.");
            }

            void Wait(DateTime startTime)
            {
                Debug.WriteLine("[Client] Waiting for race start.");
                Debug.WriteLine($"[Client] Time: {DateTime.UtcNow}");
                Debug.WriteLine($"[Client] Start Time: {startTime}");

                ActionWrappers.TryWaitUntil(() => DateTime.UtcNow > startTime, int.MaxValue);
            }

            bool IsGo()
            {
                // Might have already received "go" signal from host, in that case just wait.
                var goMessage = State.StartSyncGo;
                if (goMessage != null)
                {
                    Debug.WriteLine("[Client] SyncStartGo found.");
                    State.StartSyncGo = null;
                    Wait(goMessage.Value.StartTime);
                    Debug.WriteLine($"[Client] Assert SyncStartGo == null | {State.StartSyncGo == null}");
                    return true;
                }
                
                return false;
            }
        }

        private bool SkipIntroIfRequestedByHost() => State.SkipRequested;
        #endregion

        #region Overrides
        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            // Do not queue: Server Sync Command
            if (packet.ServerMessage != null)
            {
                HandleServerMessage(peer, packet.ServerMessage.Value);
                return;
            }

            if (packet.SyncStartGo.HasValue)
            {
                State.StartSyncGo = packet.SyncStartGo.Value;
                Debug.WriteLine($"[Client] Set SyncStartGo | {State.StartSyncGo}");
            }

            PacketQueue.Enqueue(new Packet<NetPeer>(peer, packet, null));
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            // TODO: [Client] Race / Handle Unreliable Packets
            PacketQueue.Enqueue(new Packet<NetPeer>(peer, null, packet));
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            Debug.WriteLine($"[Client] Connected to Host, Sending Player Data");
            
            // Inform host of player data.
            var playerData = IoC.GetConstant<NetplayImguiConfig>();
            var setPlayerData = new ClientSetPlayerData() { Data = playerData.ToHostPlayerData() };
            var message = new ServerMessage(setPlayerData);
            var packet = new ReliablePacket { ServerMessage = message };
            peer.Send(packet.Serialize(), DeliveryMethod.ReliableUnordered);
            peer.Flush();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override void OnConnectionRequest(ConnectionRequest request) { }
        #endregion
    }
}
