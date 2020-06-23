using System;
using System.Diagnostics;
using System.Linq;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Riders.Netplay.Messages.Unreliable;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class CommonState
    {
        public CommonState(HostPlayerData selfInfo)
        {
            SelfInfo = selfInfo;
            ResetRace();
        }

        private void ResetRace()
        {
            Array.Fill(RaceSync, new Timestamped<UnreliablePacketPlayer>());
            Array.Fill(MovementFlagsSync, new Timestamped<MovementFlagsMsg>());
            Array.Fill(AttackSync, new Timestamped<SetAttack>(new SetAttack(false, 0)));
        }

        /// <summary>
        /// Contains information about own player.
        /// </summary>
        public HostPlayerData SelfInfo;

        /// <summary>
        /// Current frame counter for the client/server.
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// Packets older than this will be discarded.
        /// </summary>
        public int MaxLatency = 1000;

        /// <summary>
        /// The currently enabled anti-cheat settings.
        /// </summary>
        public CheatKind AntiCheatMode;

        /// <summary>
        /// Contains information about other players.
        /// </summary>
        public HostPlayerData[] PlayerInfo = new HostPlayerData[0];

        /// <summary>
        /// Sync data for course select.
        /// It is applied to the game at the start of the course select function if not default/null.
        /// </summary>
        public Volatile<Timestamped<CourseSelectSync>> CourseSelectSync = new Volatile<Timestamped<CourseSelectSync>>(new Timestamped<CourseSelectSync>());

        /// <summary>
        /// Sync data for rule settings.
        /// It is applied to the game at the start of the rule settings function if not default/null.
        /// </summary>
        public Volatile<Timestamped<RuleSettingsSync>> RuleSettingsSync = new Volatile<Timestamped<RuleSettingsSync>>(new Timestamped<RuleSettingsSync>());

        /// <summary>
        /// Sync data for character select.
        /// It is applied to the game at the start of the character select function if not default/null.
        /// </summary>
        public Volatile<Timestamped<CharaSelectSync>> CharaSelectSync = new Volatile<Timestamped<CharaSelectSync>>(new Timestamped<CharaSelectSync>());

        /// <summary>
        /// Sync data for races.
        /// It is applied to the game at the start of the race event if not null.
        /// </summary>
        public Timestamped<UnreliablePacketPlayer>[] RaceSync = new Timestamped<UnreliablePacketPlayer>[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Contains movement flags for each client.
        /// </summary>
        public MovementFlagsMsg[] MovementFlagsSync = new MovementFlagsMsg[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Contains the synchronization data for handling attacks.
        /// </summary>
        public Timestamped<SetAttack>[] AttackSync = new Timestamped<SetAttack>[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Provides event notifications for when contents of menus etc. are changed.
        /// </summary>
        public MenuChangedEventHandler Delta = new MenuChangedEventHandler();

        /// <summary>
        /// Character select exit state.
        /// </summary>
        public ExitKind CharaSelectExit = ExitKind.Null;

        /// <summary>
        /// Stage intro cutscene skip requested by host.
        /// </summary>
        public bool SkipRequested = false;

        /// <summary>
        /// Set to true when client receives set stage flag for course select and is discarded
        /// after the set stage function runs.
        /// </summary>
        public bool ReceivedSetStageFlag = false;

        /// <summary>
        /// Gets the go command from the host for syncing start time.
        /// </summary>
        public Volatile<SyncStartGo> StartSyncGo = new Volatile<SyncStartGo>();

        /// <summary>
        /// When true, does not rebroadcast attack events.
        /// </summary>
        public bool IsProcessingAttackPackets = false;

        /// <summary>
        /// Drops character select apply packets if true.
        /// </summary>
        private bool _dropCharSelectPackets = false;

        /// <summary>
        /// Last applied character select sync packet.
        /// </summary>
        private CharaSelectSync _lastCharaSelectSync = new CharaSelectSync();

        /// <summary>
        /// Returns the total count of players.
        /// </summary>
        public int GetPlayerCount()
        {
            if (PlayerInfo.Length > 0)
                return Math.Max(PlayerInfo.Max(x => x.PlayerIndex) + 1, SelfInfo.PlayerIndex + 1);

            return 1;
        }

        /// <summary>
        /// True if there are any attacks.
        /// </summary>
        public bool HasAttacks() => AttackSync.Any(x => !x.IsDiscard(MaxLatency) && x.Value.IsValid);

        /// <summary>
        /// Checks if an attack should be rejected by rejecting any attacks performed on the player that were not sent over the network.
        /// </summary>
        public unsafe int ShouldRejectAttackTask(Sewer56.SonicRiders.Structures.Gameplay.Player* playerOne, Sewer56.SonicRiders.Structures.Gameplay.Player* playerTwo)
        {
            if (!IsProcessingAttackPackets)
            {
                var p1Index = Player.GetPlayerIndex(playerOne);
                return p1Index != 0 ? 1 : 0;
            }

            return 0;
        }

        /// <summary>
        /// Processes all attack tasks and resets them to the default value.
        /// </summary>
        public unsafe void ProcessAttackTasks()
        {
            IsProcessingAttackPackets = true;
            for (var x = 0; x < AttackSync.Length; x++)
            {
                if (x == 0)
                    continue;

                var atkSync = AttackSync[x];
                if (atkSync.IsDiscard(MaxLatency))
                    continue;

                var value = atkSync.Value;
                if (value.IsValid)
                {
                    Debug.WriteLine($"[State] Execute Attack by {x} on {value.Target}");
                    StartAttackTask(x, value.Target);
                }
            }

            Array.Fill(AttackSync, new Timestamped<SetAttack>(new SetAttack(false, 0)));
            IsProcessingAttackPackets = false;
        }

        /// <summary>
        /// Starts an attack between two players.
        /// </summary>
        /// <param name="playerOne">The attacking player index.</param>
        /// <param name="playerTwo">The player to be attacked index.</param>
        /// <param name="a3">Unknown Parameter</param>
        public unsafe void StartAttackTask(int playerOne, int playerTwo, int a3 = 1)
        {
            Functions.StartAttackTask.GetWrapper()(&Player.Players.Pointer[playerOne], &Player.Players.Pointer[playerTwo], a3);
        }

        /// <summary>
        /// Synchronizes the course select state with the currently reported state.
        /// </summary>
        public unsafe void SyncCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (!CourseSelectSync.HasValue) 
                return;
            
            var sync = CourseSelectSync.Get();
            if (!sync.IsDiscard(MaxLatency))
                sync.Value.ToGame(task);
        }

        /// <summary>
        /// Synchronizes rule setting state with the currently reported state.
        /// </summary>
        public unsafe void SyncRuleSettings(Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (!RuleSettingsSync.HasValue)
                return;

            var sync = RuleSettingsSync.Get();
            if (!sync.IsDiscard(MaxLatency))
                sync.Value.ToGame(task);
        }

        /// <summary>
        /// Common implementation for syncing character select events.
        /// </summary>
        /// <param name="task">Current character select task.</param>
        public unsafe void SyncCharaSelect(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (!CharaSelectSync.HasValue)
                return;

            var result = CharaSelectSync.Get();
            if (result.IsDiscard(MaxLatency) || _dropCharSelectPackets) 
                return;

            _lastCharaSelectSync = result.Value;
            result.Value.ToGame(task);
        }

        /// <summary>
        /// Execute this when starting a race in character select..
        /// </summary>
        public unsafe void OnCharacterSelectStartRace()
        {
            _dropCharSelectPackets = true;
        }

        /// <summary>
        /// Sets all players to non-CPU when the intro cutscene ends.
        /// We do this because CPUs can still trigger certain events such as boosting in-race.
        /// </summary>
        public unsafe void OnIntroCutsceneEnd()
        {
            for (int x = 0; x < Player.MaxNumberOfPlayers; x++)
            {
                Player.Players[x].IsAiLogic = PlayerType.Human;
                Player.Players[x].IsAiVisual = PlayerType.Human;
            }
        }

        /// <summary>
        /// Common implementation for handling the event of starting a race.
        /// </summary>
        public unsafe void OnSetupRace(Task<TitleSequence, TitleSequenceTaskState>* task)
        {
            if (task->TaskData->RaceMode != RaceMode.TagMode)
                *State.NumberOfRacers = (byte)GetPlayerCount();

            _dropCharSelectPackets = false;
            _lastCharaSelectSync.ToGameOnlyCharacter();
            ResetRace();
        }

        /// <summary>
        /// Applies the current race state obtained from clients/host to the game.
        /// </summary>
        public void ApplyRaceSync()
        {
            // Apply data of all players.
            for (int x = 1; x < RaceSync.Length; x++)
            {
                var sync = RaceSync[x];
                if (sync.IsDiscard(MaxLatency))
                    continue;

                if (sync.Value.IsDefault())
                {
                    Debug.WriteLine("Discarding Race Packet due to Default Comparison");
                    continue;
                }

                sync.Value.ToGame(x);
            }
        }

        /// <summary>
        /// Handles all Boost/Tornado/Attack tasks received from the clients.
        /// </summary>
        public unsafe Sewer56.SonicRiders.Structures.Gameplay.Player* OnAfterSetMovementFlags(Sewer56.SonicRiders.Structures.Gameplay.Player* player)
        {
            var index = Player.GetPlayerIndex(player);

            if (index == 0)
                return player;

            MovementFlagsSync[index].ToGame(player);
            return player;
        }

        /// <summary>
        /// Gets the index of a remote (on the host's end) player.
        /// </summary>
        public virtual int GetRemotePlayerIndex(int localPlayerIndex)
        {
            if (localPlayerIndex == 0)
                return SelfInfo.PlayerIndex;

            return PlayerInfo[localPlayerIndex - 1].PlayerIndex;
        }

        protected MessageQueue _queue = new MessageQueue();
    }
}
