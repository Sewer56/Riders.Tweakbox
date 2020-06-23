using System.Collections.Concurrent;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class HostState : CommonState
    {
        public HostState(HostPlayerData selfInfo) : base(selfInfo) { }

        /// <summary>
        /// Stores a mapping of peers to players.
        /// </summary>
        public PlayerMap<PlayerState> PlayerMap = new PlayerMap<PlayerState>();

        /// <summary>
        /// Changes in character select since the last time they were sent to the clients.
        /// </summary>
        public Timestamped<CharaSelectLoop>[] CharaSelectLoop = new Timestamped<CharaSelectLoop>[Constants.MaxNumberOfPlayers];

        /// <summary>
        /// Changes in rule settings since the last time they were sent to the clients.
        /// </summary>
        public ConcurrentQueue<Timestamped<RuleSettingsLoop>> RuleSettingsLoop => _queue.Get<Timestamped<RuleSettingsLoop>>();

        /// <summary>
        /// Changes in course select since the last time they were sent to the clients.
        /// </summary>
        public ConcurrentQueue<Timestamped<CourseSelectLoop>> CourseSelectLoop => _queue.Get<Timestamped<CourseSelectLoop>>();

        /// <summary>
        /// Empties the character select queue and populates an array of character entries, indexed
        /// by player id.
        /// </summary>
        public CharaSelectLoop[] GetCharacterSelect()
        {
            var charLoops = new CharaSelectLoop[Constants.MaxNumberOfPlayers];
            for (var x = 0; x < CharaSelectLoop.Length; x++)
            {
                var charSelect = CharaSelectLoop[x];
                if (charSelect == null || charSelect.IsDiscard(MaxLatency))
                    continue;

                charLoops[x] = charSelect;
            }

            return charLoops;
        }

        /// <summary>
        /// Empties the rule settings queue and merges all loops into a single loop.
        /// </summary>
        public RuleSettingsLoop GetRuleSettings()
        {
            var loop = new RuleSettingsLoop();
            while (RuleSettingsLoop.TryDequeue(out var result))
            {
                if (result.IsDiscard(MaxLatency))
                    continue;

                loop = loop.Add(result.Value);
            }

            return loop;
        }

        /// <summary>
        /// Empties the course select queue and merges all loops into a single sync command.
        /// </summary>
        public CourseSelectLoop GetCourseSelect()
        {
            var loop = new CourseSelectLoop();
            while (CourseSelectLoop.TryDequeue(out var result))
            {
                if (result.IsDiscard(MaxLatency))
                    continue;

                loop = loop.Add(result.Value);
            }

            return loop;
        }


        /// <summary>
        /// Gets the index of a remote (on the host's end) player.
        /// </summary>
        public override int GetRemotePlayerIndex(int localPlayerIndex) => localPlayerIndex;
    }
}
