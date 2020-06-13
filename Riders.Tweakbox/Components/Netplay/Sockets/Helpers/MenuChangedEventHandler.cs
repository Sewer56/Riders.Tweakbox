using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Allows for tracking of menu movement by comparing user manually submitted before and after menu states.
    /// </summary>
    public unsafe class MenuChangedEventHandler
    {
        public event RuleSettingsUpdated OnRuleSettingsUpdated;
        public event CourseSelectUpdated OnCourseSelectUpdated;

        private RuleSettingsSync _lastRule   = new RuleSettingsSync();
        private CourseSelectSync _lastCourse = new CourseSelectSync();

        public void Set(Task<RaceRules, RaceRulesTaskState>* task) => _lastRule = RuleSettingsSync.FromGame(task);
        public void Set(Task<CourseSelect, CourseSelectTaskState>* task) => _lastCourse = CourseSelectSync.FromGame(task);

        public void Update(Task<RaceRules, RaceRulesTaskState>* task)
        {
            var rule  = RuleSettingsSync.FromGame(task);
            var delta = _lastRule.Delta(rule);
            _lastRule = rule;

            if (!delta.IsDefault())
                OnRuleSettingsUpdated?.Invoke(delta, task);
        }

        public void Update(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            var course = CourseSelectSync.FromGame(task);
            var delta  = _lastCourse.Delta(course);
            _lastCourse = course;

            if (!delta.IsDefault())
                OnCourseSelectUpdated?.Invoke(delta, task);
        }

        public unsafe delegate void CourseSelectUpdated(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task);
        public unsafe delegate void RuleSettingsUpdated(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task);
    }
}
