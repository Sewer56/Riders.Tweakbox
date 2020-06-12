using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets.Helpers
{
    /// <summary>
    /// Using hooks, tracks the differences in menu movement.
    /// </summary>
    public class MenuDeltaTracker
    {
        public RuleSettingsLoop Rule = new RuleSettingsLoop();
        public CourseSelectLoop Course = new CourseSelectLoop();
        public event RuleSettingsUpdated OnRuleSettingsUpdated;
        public event CourseSelectUpdated OnCourseSelectUpdated;

        private IHook<Functions.DefaultTaskFnWithReturn> _onCourseSelectHook;
        private IHook<Functions.DefaultTaskFnWithReturn> _onRaceSettingsHook;

        public MenuDeltaTracker()
        {
            _onCourseSelectHook = Functions.CourseSelectTask.Hook(OnCourseSelectTask).Activate();
            _onRaceSettingsHook = Functions.RaceSettingTask.Hook(OnRaceSettingsTask).Activate();
        }

        private unsafe byte OnRaceSettingsTask()
        {
            var lastRule    = RuleSettingsSync.FromGame((Task<RaceRules, RaceRulesTaskState>*)(*State.CurrentTask));
            var result      = _onRaceSettingsHook.OriginalFunction();
            var rule        = RuleSettingsSync.FromGame((Task<RaceRules, RaceRulesTaskState>*)(*State.CurrentTask));
            Rule            = lastRule.Delta(rule);

            if (!Rule.IsDefault())
                OnRuleSettingsUpdated?.Invoke(Rule, (Task<RaceRules, RaceRulesTaskState>*)(*State.CurrentTask));

            return result;
        }

        private unsafe byte OnCourseSelectTask()
        {
            var lastCourse = CourseSelectSync.FromGame((Task<CourseSelect, CourseSelectTaskState>*) (*State.CurrentTask));
            var result     = _onCourseSelectHook.OriginalFunction();
            var course     = CourseSelectSync.FromGame((Task<CourseSelect, CourseSelectTaskState>*)(*State.CurrentTask));
            Course         = lastCourse.GetDelta(course);

            if (!Course.IsDefault())
                OnCourseSelectUpdated?.Invoke(Course, (Task<CourseSelect, CourseSelectTaskState>*)(*State.CurrentTask));

            return result;
        }

        public unsafe delegate void CourseSelectUpdated(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task);
        public unsafe delegate void RuleSettingsUpdated(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task);
    }
}
