using System.Diagnostics;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    public class PlayerState
    {
        public int Latency = 999;
        public CourseSelectLoop CourseSelectLoop = new CourseSelectLoop();
        public CharaSelectLoop CharaSelectLoop = new CharaSelectLoop();
        public RuleSettingsLoop RuleSettingsLoop = new RuleSettingsLoop();
        public bool ReadyToStartRace = false;

        /// <summary>
        /// Gets and erases the current <see cref="RuleSettingsLoop"/>.
        /// </summary>
        public RuleSettingsLoop GetEraseRuleSettingsLoop() => Get(ref RuleSettingsLoop);

        /// <summary>
        /// Gets and erases the current <see cref="CourseSelectLoop"/>.
        /// </summary>
        public CourseSelectLoop GetEraseCourseLoop() => Get(ref CourseSelectLoop);

        /// <summary>
        /// Gets a field and erases its current content.
        /// </summary>
        public T Get<T>(ref T item) where T : new()
        {
            var copy = item;
            item = new T();
            return copy;
        }

        /// <summary>
        /// Gets a field and erases its current content.
        /// </summary>
        public T Get<T>(ref T item, T defaultValue) where T : new()
        {
            var copy = item;
            item = defaultValue;
            return copy;
        }
        
        /// <summary>
        /// Adds a synchronization command to the current player.
        /// </summary>
        public void SetLoopCommand(IMenuSynchronizationCommand syncCommand)
        {
            switch (syncCommand)
            {
                case CharaSelectLoop charaSelectLoop:
                    CharaSelectLoop = charaSelectLoop;
                    break;
                case CourseSelectLoop courseSelectLoop:
                    CourseSelectLoop = courseSelectLoop;
                    break;
                case RuleSettingsLoop ruleSettingsLoop:
                    RuleSettingsLoop = ruleSettingsLoop;
                    break;
            }
        }
    }
}
