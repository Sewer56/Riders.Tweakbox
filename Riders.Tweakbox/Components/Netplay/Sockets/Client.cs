using System;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Unreliable;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;
using Debug = System.Diagnostics.Debug;
using PlayerState = Sewer56.SonicRiders.Structures.Enums.PlayerState;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public Client(string ipAddress, int port, string password, NetplayController controller) : base(controller)
        {
            Debug.WriteLine($"[Client] Joining Server on {ipAddress}:{port} with password {password}");
            if (Event.LastTask != Tasks.CourseSelect)
                throw new Exception("You are only allowed to join the host in the Course Select Menu");

            State = new CommonState(IoC.GetConstant<NetplayImguiConfig>().ToHostPlayerData());
            Manager.Start();
            Manager.Connect(ipAddress, port, password);

            // Add undo menu movement when connected.
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
            //Event.SetNewPlayerStateHandler += State.SetPlayerStateHandler;
        }

        public override void Dispose()
        {
            Debug.WriteLine($"[Client] Disposing of Socket, Disconnected");
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
            //Event.SetNewPlayerStateHandler -= State.SetPlayerStateHandler;
        }

        public override bool IsHost() => false;
        public override void Update()
        {
            base.Update();
            State.FrameCounter += 1;
        }

        public override void HandlePacket(Packet<NetPeer> packet)
        {
            if (packet.As<IPacket>().GetPacketType() == PacketKind.Reliable)
                HandleReliable(packet.As<ReliablePacket>());

            else if (packet.As<IPacket>().GetPacketType() == PacketKind.Unreliable)
                HandleUnreliable(packet.As<UnreliablePacket>());
        }

        private void HandleUnreliable(UnreliablePacket result)
        {
            var players = result.Players;

            // Fill in from player 2.
            for (int x = 0; x < players.Length; x++)
                State.RaceSync[x + 1] = players[x];
        }

        private void HandleReliable(ReliablePacket packet)
        {
            if (packet.MenuSynchronizationCommand.HasValue)
                HandleMenuMessage(packet.MenuSynchronizationCommand.Value);

            if (packet.ServerMessage.HasValue)
                HandleServerMessage(packet.ServerMessage.Value);

            // All remaining messages.
            if (packet.HasSyncStartSkip)
                State.SkipRequested = true;

            if (packet.GameData.HasValue)
            {
                Debug.WriteLine($"[Client] Received Game Data, Applying");
                packet.GameData.Value.ToGame();
            }

            if (packet.SyncStartGo.HasValue)
            {
                Debug.WriteLine($"[Client] Set SyncStartGo");
                State.StartSyncGo = packet.SyncStartGo.Value;
            }

            if (packet.MovementFlags.HasValue)
            {
                packet.MovementFlags.Value.AsInterface().ToArray(State.MovementFlagsSync, MovementFlagsPacked.NumberOfEntries - 1, 0, 1);
            }

            if (packet.Attack.HasValue)
            {
                Debug.WriteLine($"[Client] Received Attack data from host");
                var value   = packet.Attack.Value;
                var attacks = new SetAttack[State.AttackSync.Length];
                value.AsInterface().ToArray(attacks, attacks.Length - 1, 0, 1);
                for (var x = 0; x < attacks.Length; x++)
                    State.AttackSync[x] = new Timestamped<SetAttack>(attacks[x]);
            }
        }

        private unsafe void HandleMenuMessage(MenuSynchronizationCommand syncCommand)
        {
            var cmd = syncCommand.Command;
            switch (cmd)
            {
                case CharaSelectSync charaSelectSync:
                    State.CharaSelectSync = new Volatile<Timestamped<CharaSelectSync>>(charaSelectSync);
                    break;

                case CourseSelectSync courseSelectSync:
                    State.CourseSelectSync = new Volatile<Timestamped<CourseSelectSync>>(courseSelectSync);
                    break;

                case CourseSelectSetStage courseSelectSetStage:
                    Debug.WriteLine("[Client] Received CharaSelect Stage Set Flag");
                    *Sewer56.SonicRiders.API.State.Level = (Levels) courseSelectSetStage.StageId;
                    State.ReceivedSetStageFlag = true;
                    break;

                case RuleSettingsSync ruleSettingsSync:
                    State.RuleSettingsSync = new Volatile<Timestamped<RuleSettingsSync>>(ruleSettingsSync);
                    break;

                case CharaSelectExit charaSelectExit:
                    Debug.WriteLine("[State] Got Start/Exit Request Flag");
                    State.CharaSelectExit = charaSelectExit.Type;
                    break;
            }
        }

        private void HandleServerMessage(ServerMessage serverMessage)
        {
            var msg = serverMessage.Message;
            switch (msg)
            {
                case HostSetPlayerData hostSetPlayerData:
                    Debug.WriteLine($"[Client] Received Player Info");
                    State.PlayerInfo = hostSetPlayerData.Data;
                    State.SelfInfo.PlayerIndex = hostSetPlayerData.Index;
                    break;

                case SetAntiCheat setAntiCheat:
                    Debug.WriteLine($"[Client] Received Anticheat Info");
                    State.AntiCheatMode = setAntiCheat.Cheats;
                    break;
            }
        }

        #region Events: On/After Events
        private void OnCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            // Note: For host, do opposite, set, sync, then resend if changed.
            State.SyncCourseSelect(task);
            State.Delta.Set(task);
        }

        private void OnAfterCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task) => State.Delta.Update(task);
        private unsafe void OnCourseSelectChanged(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (Manager.ConnectedPeersCount <= 0)
                return;

            loop.Undo(task);
            SendAndFlush(Manager.FirstPeer, new ReliablePacket(loop), DeliveryMethod.ReliableOrdered);
        }

        private void OnCourseSelectSetStage()
        {
            if (!State.ReceivedSetStageFlag)
                SendAndFlush(Manager.FirstPeer, new ReliablePacket(new CourseSelectSetStage((byte)*Sewer56.SonicRiders.API.State.Level)), DeliveryMethod.ReliableOrdered, "[Client] Sending CharaSelect Stage Set Flag");

            State.ReceivedSetStageFlag = false;
        }

        private void OnRuleSettings(Task<RaceRules, RaceRulesTaskState>* task)
        {
            State.SyncRuleSettings(task);
            State.Delta.Set(task);
        }

        private void OnAfterRuleSettings(Task<RaceRules, RaceRulesTaskState>* task) => State.Delta.Update(task);
        private unsafe void OnRuleSettingsChanged(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (Manager.ConnectedPeersCount <= 0)
                return;

            loop.Undo(task);
            SendAndFlush(Manager.FirstPeer, new ReliablePacket(loop), DeliveryMethod.ReliableOrdered);
        }

        private void OnCharaSelect(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            State.ReceivedSetStageFlag = false;
            State.SyncCharaSelect(task);
            if (State.CharaSelectExit == ExitKind.Null)
                SendAndFlush(Manager.FirstPeer, new ReliablePacket(CharaSelectLoop.FromGame(task)), DeliveryMethod.ReliableSequenced);
        }

        private bool MenuCheckIfExitCharaSelect() => State.CharaSelectExit == ExitKind.Exit;
        private void MenuOnExitCharaSelect()
        {
            // We started ourselves, tell host to rebroadcast.
            if (State.CharaSelectExit != ExitKind.Exit)
                SendAndFlush(Manager.FirstPeer, new ReliablePacket(new CharaSelectExit(ExitKind.Exit)), DeliveryMethod.ReliableOrdered, "[Client] Sending CharaSelect Exit flag to Host");

            State.CharaSelectExit = ExitKind.Null;
        }

        private bool MenuCheckIfStartRace() => State.CharaSelectExit == ExitKind.Start;
        private void MenuOnMenuStartRace()
        {
            // We started ourselves, tell host to rebroadcast.
            if (State.CharaSelectExit != ExitKind.Start)
                SendAndFlush(Manager.FirstPeer, new ReliablePacket(new CharaSelectExit(ExitKind.Start)), DeliveryMethod.ReliableOrdered, "[Client] Sending CharaSelect Start flag to Host");

            State.CharaSelectExit = ExitKind.Null;
            State.OnCharacterSelectStartRace();
        }

        private void OnSetupRace(Task<TitleSequence, TitleSequenceTaskState>* task) => State.OnSetupRace(task);
        private bool OnCheckIfRaceSkipIntro() => State.SkipRequested;
        private void OnSkipRaceIntro()
        {
            SyncStartGo goMessage = default;
            bool IsGoSignal()
            {
                goMessage = State.StartSyncGo.Get();
                return !goMessage.IsDefault();
            }

            if (!State.SkipRequested)
                SendAndFlush(Manager.FirstPeer, new ReliablePacket() { HasSyncStartSkip = true }, DeliveryMethod.ReliableOrdered, "[Client] Skipped intro ourselves, sending skip notification to host.");

            State.SkipRequested = false;
            SendAndFlush(Manager.FirstPeer, new ReliablePacket() { HasSyncStartReady = true }, DeliveryMethod.ReliableOrdered, "[Client] Sending HasSyncStartReady.");

            if (!PollUntil(IsGoSignal, HandshakeTimeout))
            {
                Debug.WriteLine("[Client] No Go Signal Received, Bailing Out!.");
                Dispose();
                return;
            }

            Wait(goMessage.StartTime);
            State.OnIntroCutsceneEnd();
            Debug.WriteLine("[Client] Race Started.");
        }

        private void OnRace(Task<byte, RaceTaskState>* task)
        {
            Poll();
            State.ApplyRaceSync();
        }

        private void AfterRace(Task<byte, RaceTaskState>* task)
        {
            var packet = new UnreliablePacket(UnreliablePacketPlayer.FromGame(0, State.FrameCounter));
            SendAndFlush(Manager.FirstPeer, packet, DeliveryMethod.Sequenced);

            State.ProcessAttackTasks();
        }

        private Player* OnAfterSetMovementFlagsOnInput(Player* player)
        {
            State.OnAfterSetMovementFlags(player);

            var index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
            if (index == 0) 
                SendAndFlush(Manager.FirstPeer, new ReliablePacket() { SetMovementFlags = new MovementFlagsMsg(player) }, DeliveryMethod.ReliableOrdered);

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

                var p2Index = Sewer56.SonicRiders.API.Player.GetPlayerIndex(playerTwo);
                Debug.WriteLine($"[Client] Send Attack on {p2Index}");
                SendAndFlush(Manager.FirstPeer, new ReliablePacket() { SetAttack = new SetAttack((byte)State.GetRemotePlayerIndex(p2Index)) }, DeliveryMethod.ReliableOrdered);
            }

            return 0;
        }

        #endregion

        #region Overrides
        public override void OnPeerConnected(NetPeer peer) => SendAndFlush(peer, new ReliablePacket(new ClientSetPlayerData(State.SelfInfo)), DeliveryMethod.ReliableUnordered, "[Client] Connected to Host, Sending Player Data");
        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => Dispose();

        // Ignored
        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public override bool OnConnectionRequest(ConnectionRequest request) { return true; }
        #endregion
    }
}
