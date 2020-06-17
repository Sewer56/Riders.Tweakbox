using System;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using PlayerState = Riders.Tweakbox.Components.Netplay.Sockets.Helpers.PlayerState;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        /// <summary>
        /// Contains that is used by the server.
        /// </summary>
        public new HostState State => (HostState) base.State;

        public string Password { get; private set; }
        public PlayerMap<PlayerState> PlayerMap { get; private set; } = new PlayerMap<PlayerState>();
        
        public Host(int port, string password, NetplayController controller) : base(controller)
        {
            Debug.WriteLine($"[Host] Hosting Server on {port} with password {password}");

            base.State = new HostState(IoC.GetConstant<NetplayImguiConfig>().ToHostPlayerData());
            Password = password;
            Manager.Start(port);

            Event.OnCharacterSelect += OnCharaSelect;
            Event.OnCheckIfExitCharaSelect += MenuCheckIfExitCharaSelect;
            Event.OnExitCharaSelect += MenuOnExitCharaSelect;

            Event.OnCheckIfStartRace += MenuCheckIfStartRace;
            Event.OnStartRace += MenuOnMenuStartRace;
            Event.OnSetupRace += OnSetupRace;

            Event.OnCheckIfSkipIntro += OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro += OnSkipRaceIntro;

            Event.OnRaceSettings += OnRuleSettings;
            Event.AfterRaceSettings += OnAfterRuleSettings;
            State.Delta.OnRuleSettingsUpdated += OnRuleSettingsChanged;

            Event.OnCourseSelect += OnCourseSelect;
            Event.AfterCourseSelect += OnAfterCourseSelect;
            Event.OnCourseSelectSetStage += OnCourseSelectSetStage;
            State.Delta.OnCourseSelectUpdated += OnCourseSelectChanged;

            Event.OnRace += OnRace;
            Event.AfterRace += AfterRace;
        }

        public override unsafe void Dispose()
        {
            Manager.DisconnectAll();
            base.Dispose();

            Event.OnCharacterSelect -= OnCharaSelect;
            Event.OnCheckIfExitCharaSelect -= MenuCheckIfExitCharaSelect;
            Event.OnExitCharaSelect -= MenuOnExitCharaSelect;

            Event.OnCheckIfStartRace -= MenuCheckIfStartRace;
            Event.OnStartRace -= MenuOnMenuStartRace;
            Event.OnSetupRace -= OnSetupRace;

            Event.OnCheckIfSkipIntro -= OnCheckIfRaceSkipIntro;
            Event.OnRaceSkipIntro -= OnSkipRaceIntro;

            Event.OnRaceSettings -= OnRuleSettings;
            Event.AfterRaceSettings -= OnAfterRuleSettings;
            State.Delta.OnRuleSettingsUpdated -= OnRuleSettingsChanged;

            Event.OnCourseSelect -= OnCourseSelect;
            Event.AfterCourseSelect -= OnAfterCourseSelect;
            Event.OnCourseSelectSetStage -= OnCourseSelectSetStage;
            State.Delta.OnCourseSelectUpdated -= OnCourseSelectChanged;

            Event.OnRace -= OnRace;
            Event.AfterRace -= AfterRace;
        }

        public override bool IsHost() => true;

        public override void Update()
        {
            base.Update();
            State.FrameCounter += 1;
        }

        public override void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.As<IPacket>().GetPacketType() == PacketKind.Reliable)
                HandleReliable(packet.Source, packet.As<ReliablePacket>());

            else if (packet.As<IPacket>().GetPacketType() == PacketKind.Unreliable)
                HandleUnreliable(packet.Source, packet.As<UnreliablePacket>());
        }

        private void HandleUnreliable(NetPeer peer, UnreliablePacket result)
        {
            // TODO: Maybe support for multiple local players in the future.
            var playerIndex = PlayerMap.GetPlayerData(peer).PlayerIndex;
            var player  = result.Players[0];
            State.RaceSync[playerIndex] = player;
        }

        private void HandleReliable(NetPeer peer, ReliablePacket packet)
        {
            if (packet.MenuSynchronizationCommand.HasValue)
                HandleMenuMessage(peer, packet.MenuSynchronizationCommand.Value);

            if (packet.ServerMessage.HasValue)
                HandleServerMessage(peer, packet.ServerMessage.Value);

            // All remaining messages.
            if (packet.HasSyncStartSkip)
            {
                State.SkipRequested = true;
                SendToAllExcept(peer, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, "[Host] Received Skip from Client, Rebroadcasting.");
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

        private void HandleMenuMessage(NetPeer peer, MenuSynchronizationCommand syncCommand)
        {
            switch (syncCommand.Command)
            {
                case CharaSelectLoop charaSelectLoop:
                    State.CharaSelectLoop[PlayerMap.GetPlayerData(peer).PlayerIndex] = charaSelectLoop;
                    break;
                case CourseSelectLoop courseSelectLoop:
                    State.CourseSelectLoop.Enqueue(courseSelectLoop);
                    break;
                case CourseSelectSetStage courseSelectSetStage:
                    *Sewer56.SonicRiders.API.State.Level = (Levels)courseSelectSetStage.StageId;
                    State.ReceivedSetStageFlag = true;
                    SendToAllExcept(peer, new ReliablePacket(courseSelectSetStage), DeliveryMethod.ReliableOrdered, "[Host] Received CharaSelect Stage Set Flag, Rebroadcasting");
                    break;
                case RuleSettingsLoop ruleSettingsLoop:
                    State.RuleSettingsLoop.Enqueue(ruleSettingsLoop);
                    break;
                case CharaSelectExit exitCommand:
                    State.CharaSelectExit = exitCommand.Type;
                    SendToAllExcept(peer, new ReliablePacket(new CharaSelectExit(exitCommand.Type)), DeliveryMethod.ReliableOrdered, "[Host] Got Start / Exit Request Flag, Rebroadcasting");
                    break;
            }
        }

        #region Events: On/After Events
        private void OnCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            // Note: For host, do opposite, set, sync, then resend if changed.
            State.Delta.Set(task);
            var sync = CourseSelectSync.FromGame(task);
            var loop = State.GetCourseSelect();
            sync.Merge(loop);

            State.CourseSelectSync = new Volatile<Timestamped<CourseSelectSync>>(sync);
            State.SyncCourseSelect(task);
        }

        private void OnAfterCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task) => State.Delta.Update(task);
        private unsafe void OnCourseSelectChanged(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (Manager.ConnectedPeersCount <= 0)
                return;

            SendToAllAndFlush(new ReliablePacket(CourseSelectSync.FromGame(task)), DeliveryMethod.ReliableOrdered);
        }

        private void OnCourseSelectSetStage()
        {
            if (!State.ReceivedSetStageFlag)
                SendToAllAndFlush(new ReliablePacket(new CourseSelectSetStage((byte)*Sewer56.SonicRiders.API.State.Level)), DeliveryMethod.ReliableOrdered, "[Host] Sending CharaSelect Stage Set Flag");

            State.ReceivedSetStageFlag = false;
        }

        private void OnRuleSettings(Task<RaceRules, RaceRulesTaskState>* task)
        {
            State.Delta.Set(task);
            var sync = RuleSettingsSync.FromGame(task);
            var loop = State.GetRuleSettings();
            sync.Merge(loop);

            State.RuleSettingsSync = new Volatile<Timestamped<RuleSettingsSync>>(sync);
            State.SyncRuleSettings(task);
        }

        private void OnAfterRuleSettings(Task<RaceRules, RaceRulesTaskState>* task) => State.Delta.Update(task);
        private unsafe void OnRuleSettingsChanged(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (Manager.ConnectedPeersCount <= 0)
                return;

            SendToAllAndFlush(new ReliablePacket(RuleSettingsSync.FromGame(task)), DeliveryMethod.ReliableOrdered);
        }

        private void OnCharaSelect(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            State.ReceivedSetStageFlag = false;

            // Calculate sync data from client info.
            var sync = State.GetCharacterSelect();
            sync[0]  = CharaSelectLoop.FromGame(task);
            State.CharaSelectSync = new Volatile<Timestamped<CharaSelectSync>>(new CharaSelectSync(sync.Where((loop, x) => x != 0).ToArray()));
            State.SyncCharaSelect(task);

            // If not starting, transmit updated sync.
            if (State.CharaSelectExit == ExitKind.Null)
            {
                foreach (var peer in Manager.ConnectedPeerList)
                {
                    var excludeIndex = PlayerMap.GetPlayerData(peer).PlayerIndex;
                    var selectSync = new CharaSelectSync(sync.Where((loop, x) => x != excludeIndex).ToArray());
                    SendAndFlush(peer, new ReliablePacket(selectSync), DeliveryMethod.ReliableSequenced);
                }
            }
        }

        private bool MenuCheckIfExitCharaSelect() => State.CharaSelectExit == ExitKind.Exit;
        private void MenuOnExitCharaSelect()
        {
            // We started ourselves, tell host to rebroadcast.
            if (State.CharaSelectExit != ExitKind.Exit)
                SendToAllAndFlush(new ReliablePacket(new CharaSelectExit(ExitKind.Exit)), DeliveryMethod.ReliableOrdered, "[Host] Sending CharaSelect Exit flag to Clients");

            State.CharaSelectExit = ExitKind.Null;
        }

        private bool MenuCheckIfStartRace() => State.CharaSelectExit == ExitKind.Start;
        private void MenuOnMenuStartRace()
        {
            // We skipped the intro ourselves, pass the news along to the clients. 
            if (State.CharaSelectExit != ExitKind.Start)
                SendToAllAndFlush(new ReliablePacket(new CharaSelectExit(ExitKind.Start)), DeliveryMethod.ReliableOrdered, "[Host] Sending CharaSelect Start Flag to Clients");

            State.CharaSelectExit = ExitKind.Null;
            State.OnCharacterSelectStartRace();
        }

        private void OnSetupRace(Task<TitleSequence, TitleSequenceTaskState>* task) => State.OnSetupRace(task);
        private bool OnCheckIfRaceSkipIntro() => State.SkipRequested;
        private void OnSkipRaceIntro()
        {
            bool TestAllReady() => PlayerMap.GetCustomData().All(x => x.ReadyToStartRace);

            // Send skip signal to clients.
            if (!State.SkipRequested) 
                SendToAllAndFlush(new ReliablePacket() { HasSyncStartSkip = true } , DeliveryMethod.ReliableOrdered, "[Host] Broadcasting Skip Signal.");

            State.SkipRequested = false;
            Debug.WriteLine("[Host] Waiting for ready messages.");

            // Note to self: Don't use wait for all clients, because the messages may have already been sent by the clients.
            if (!PollUntil(TestAllReady, HandshakeTimeout))
            {
                Debug.WriteLine("[Host] It's no use, let's get outta here!.");
                Manager.DisconnectAll();
                return;
            }

            var startTime = new SyncStartGo(State.MaxLatency);
            SendToAllAndFlush(new ReliablePacket() { SyncStartGo = startTime }, DeliveryMethod.ReliableOrdered, "[Host] Sending Race Start Signal.");
            
            // Disable skip flags for everyone.
            var data = PlayerMap.GetCustomData();
            foreach (var dt in data)
                dt.ReadyToStartRace = false;

            Wait(startTime.StartTime, "[Host] Race Started.");
            State.OnIntroCutsceneEnd();
        }

        private void OnRace(Task<byte, RaceTaskState>* task) => State.OnRace();
        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            State.RaceSync[0] = new Timestamped<UnreliablePacketPlayer>(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
            
            // Populate data for non-expired packets.
            var players = new UnreliablePacketPlayer[State.RaceSync.Length];
            Array.Fill(players, new UnreliablePacketPlayer());
            for (int x = 0; x < State.RaceSync.Length; x++)
            {
                var sync = State.RaceSync[x];
                if (!sync.IsDiscard(State.MaxLatency))
                    players[x] = State.RaceSync[x];
            }

            // Broadcast data to all clients.
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var excludeIndex = PlayerMap.GetPlayerData(peer).PlayerIndex;
                var packet = new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray());
                SendAndFlush(peer, packet, DeliveryMethod.Sequenced);
            }
        }
        #endregion

        #region Socket Events
        public override void OnPeerConnected(NetPeer peer)
        {
            bool CheckIfUserData(Packet<NetPeer> packet)
            {
                if (packet.As<IPacket>().GetPacketType() != PacketKind.Reliable)
                    return false;

                return packet.As<ReliablePacket>().ServerMessage?.MessageKind == ServerMessageType.ClientSetPlayerData;
            }

            // Handle player handshake here!
            Debug.WriteLine($"[Host] Client {peer.EndPoint.Address} | {peer.Id}, waiting for message.");
            if (!TryWaitForMessage(peer, CheckIfUserData, HandshakeTimeout))
            {
                Debug.WriteLine($"[Host] Disconnecting client, did not receive user data.");
                peer.Disconnect();
                return;
            }
            
            SendAndFlush(peer, new ReliablePacket() { GameData = GameData.FromGame() }, DeliveryMethod.ReliableUnordered, "[Host] Received user data, uploading game data.");
            SendAndFlush(peer, new ReliablePacket(CourseSelectSync.FromGame(Event.CourseSelect)), DeliveryMethod.ReliableUnordered, "[Host] Sending course select data for initial sync.");
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

        public override bool OnConnectionRequest(ConnectionRequest request)
        {
            bool Reject(string message)
            {
                Debug.WriteLine(message);
                request.Reject();
                return false;
            }

            Debug.WriteLine($"[Host] Received Connection Request");
            if (Event.LastTask != Tasks.CourseSelect)
                return Reject("[Host] Rejected Connection | Not on Course Select");

            if (!PlayerMap.HasEmptySlots())
                return Reject($"[Host] Rejected Connection | No Empty Slots");

            Debug.WriteLine($"[Host] Accepting if Password Matches");
            return request.AcceptIfKey(Password) != null;
        }
        #endregion

        #region Utility Functions
        /// <summary>
        /// Sends a personalized player map (excluding the player). 
        /// </summary>
        public void UpdatePlayerMap()
        {
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var message = PlayerMap.ToMessage(peer, State.SelfInfo);
                SendAndFlush(peer, new ReliablePacket(message), DeliveryMethod.ReliableUnordered);
            }

            State.PlayerInfo = PlayerMap.ToMessage(State.SelfInfo.PlayerIndex, State.SelfInfo).Data;
        }
        #endregion
    }
}
