using System;
using System.Collections.Concurrent;
using System.Linq;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class ClientState
    {
        public ClientState(HostPlayerData selfInfo)
        {
            SelfInfo = selfInfo;
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
        /// Gets the go command from the host for syncing start time.
        /// </summary>
        public Volatile<SyncStartGo> StartSyncGo = new Volatile<SyncStartGo>();

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
        /// Synchronizes the course select state with the currently reported state.
        /// </summary>
        public unsafe void SyncCourseSelect(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (!CourseSelectSync.HasValue) 
                return;
            
            var sync = CourseSelectSync.Get();
            if (!sync.IsDiscard(MaxLatency))
                sync.Value.ToGame(task);

            Console.WriteLine($"CourseSelectSync");
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

            Console.WriteLine($"SyncRuleSettings");
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

            Console.WriteLine($"CharaSelectSync");
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
        /// Common implementation for handling the event of starting a race.
        /// </summary>
        public unsafe void OnSetupRace(Task<TitleSequence, TitleSequenceTaskState>* task)
        {
            if (task->TaskData->RaceMode != RaceMode.TagMode)
                *Sewer56.SonicRiders.API.State.NumberOfRacers = (byte)GetPlayerCount();

            _dropCharSelectPackets = false;
            _lastCharaSelectSync.ToGameOnlyCharacter();

            Console.WriteLine($"{Player.Players[0].IsAiVisual}");
            Console.WriteLine($"{Player.Players[0].IsAiLogic}");

            Console.WriteLine($"{Player.Players[1].IsAiVisual}");
            Console.WriteLine($"{Player.Players[1].IsAiLogic}");
        }

        protected MessageQueue _queue = new MessageQueue();
    }
}
