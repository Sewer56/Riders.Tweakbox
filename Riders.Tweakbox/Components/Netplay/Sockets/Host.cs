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
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        /// <summary>
        /// Contains that is used by the server.
        /// </summary>
        public new HostState State => (HostState) base.State;

        public string Password { get; private set; }
        
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
            Event.AfterSetMovementFlagsOnInput += OnAfterSetMovementFlagsOnInput;
            Event.OnShouldRejectAttackTask += OnShouldRejectAttackTask;
            Event.OnStartAttackTask += OnStartAttackTask;
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
            Event.AfterSetMovementFlagsOnInput -= OnAfterSetMovementFlagsOnInput;
            Event.OnShouldRejectAttackTask -= OnShouldRejectAttackTask;
            Event.OnStartAttackTask -= OnStartAttackTask;
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
            var playerIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
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
                var customData = State.PlayerMap.GetCustomData(peer);
                customData.ReadyToStartRace = true;
            }

            if (packet.SetMovementFlags.HasValue)
            {
                var playerIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
                State.MovementFlagsSync[playerIndex] = new Timestamped<MovementFlagsMsg>(packet.SetMovementFlags.Value);
            }

            if (packet.SetAttack.HasValue)
            {
                var playerIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
                Debug.WriteLine($"[Host] Received Attack from {playerIndex} to hit {packet.SetAttack.Value.Target}");
                State.AttackSync[playerIndex] = new Timestamped<SetAttack>(packet.SetAttack.Value);
            }
        }

        private void HandleServerMessage(NetPeer peer, ServerMessage serverMessage)
        {
            var message = serverMessage.Message;
            switch (message)
            {
                case ClientSetPlayerData clientSetPlayerData:
                    State.PlayerMap.AddOrUpdatePlayerData(peer, clientSetPlayerData.Data);
                    UpdatePlayerMap();
                    break;
            }
        }

        private void HandleMenuMessage(NetPeer peer, MenuSynchronizationCommand syncCommand)
        {
            switch (syncCommand.Command)
            {
                case CharaSelectLoop charaSelectLoop:
                    State.CharaSelectLoop[State.PlayerMap.GetPlayerData(peer).PlayerIndex] = charaSelectLoop;
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
                    var excludeIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
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
            bool TestAllReady() => State.PlayerMap.GetCustomData().All(x => x.ReadyToStartRace);

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
            var data = State.PlayerMap.GetCustomData();
            foreach (var dt in data)
                dt.ReadyToStartRace = false;

            Wait(startTime.StartTime, "[Host] Race Started.");
            State.OnIntroCutsceneEnd();
        }

        private void OnRace(Task<byte, RaceTaskState>* task)
        {
            Poll();
            State.ApplyRaceSync();
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            State.RaceSync[0] = new Timestamped<UnreliablePacketPlayer>(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
            
            // Populate data for non-expired packets.
            var players = new UnreliablePacketPlayer[State.GetPlayerCount()];
            Array.Fill(players, new UnreliablePacketPlayer());
            for (int x = 0; x < players.Length; x++)
            {
                var sync = State.RaceSync[x];
                if (!sync.IsDiscard(State.MaxLatency))
                    players[x] = State.RaceSync[x];
            }

            // Broadcast data to all clients.
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var excludeIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
                var packet = new UnreliablePacket(players.Where((loop, x) => x != excludeIndex).ToArray());
                SendAndFlush(peer, packet, DeliveryMethod.Sequenced);
            }

            // Broadcast Attack Data
            if (State.HasAttacks())
            {
                Debug.WriteLine($"[Host] Sending Attack Matrix to Clients");
                foreach (var peer in Manager.ConnectedPeerList)
                {
                    var excludeIndex = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
                    var attacks      = State.AttackSync.Select(x => x.Value).Where((attack, x) => x != excludeIndex).ToArray();
                    if (!attacks.Any(x => x.IsValid))
                        continue;

                    for (var x = 0; x < attacks.Length; x++)
                    {
                        if (x < excludeIndex)
                            attacks[x].Target -= 1;
                    }

                    for (var x = 0; x < attacks.Length; x++)
                    {
                        var attack = attacks[x];
                        if (attack.IsValid)
                            Debug.WriteLine($"[Host] Send Attack Source ({x}), Target {attack.Target}");
                    }

                    var packed       = new AttackPacked().AsInterface().SetData(attacks);
                    SendAndFlush(peer, new ReliablePacket() { Attack = packed }, DeliveryMethod.ReliableOrdered);
                }
            }

            State.ProcessAttackTasks();
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            State.OnAfterSetMovementFlags(player);

            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (index == 0)
            {
                State.MovementFlagsSync[0] = new MovementFlagsMsg(player);
                foreach (var peer in Manager.ConnectedPeerList)
                {
                    var excludeIndex  = State.PlayerMap.GetPlayerData(peer).PlayerIndex;
                    var movementFlags = State.MovementFlagsSync.Where((timestamped, x) => x != excludeIndex).ToArray();
                    SendAndFlush(peer, new ReliablePacket() { MovementFlags = new MovementFlagsPacked().AsInterface().SetData(movementFlags, 0) } , DeliveryMethod.ReliableOrdered);
                }
            }

            return player;
        }

        private int OnShouldRejectAttackTask(Player* playerOne, Player* playerTwo, int a3) => State.ShouldRejectAttackTask(playerOne, playerTwo);
        private int OnStartAttackTask(Player* playerOne, Player* playerTwo, int a3)
        {
            // Send attack notification to host if not 
            if (!State.IsProcessingAttackPackets)
            {
                var p1Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerOne);
                if (p1Index != 0)
                    return 0;

                // Append to list of attacks to send out at frame end.
                var p2Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerTwo);
                Debug.WriteLine($"[Host] Set Attack on {p2Index}");
                State.AttackSync[0] = new Timestamped<SetAttack>(new SetAttack((byte) p2Index));
            }

            return 0;
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
            State.PlayerMap.RemovePeer(peer);
            UpdatePlayerMap();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency.
            var data = State.PlayerMap.GetCustomData(peer);
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

            if (!State.PlayerMap.HasEmptySlots())
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
                var message = State.PlayerMap.ToMessage(peer, State.SelfInfo);
                SendAndFlush(peer, new ReliablePacket(message), DeliveryMethod.ReliableUnordered);
            }

            State.PlayerInfo = State.PlayerMap.ToMessage(State.SelfInfo.PlayerIndex, State.SelfInfo).Data;
        }
        #endregion
    }
}
