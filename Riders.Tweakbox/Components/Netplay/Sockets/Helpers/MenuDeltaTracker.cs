using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Misc;
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

        private TaskTracker _tracker = IoC.GetConstant<TaskTracker>();
        private IHook<Functions.DefaultTaskFnWithReturn> _onCourseSelectHook;
        private IHook<Functions.DefaultTaskFnWithReturn> _onRaceSettingsHook;

        public MenuDeltaTracker()
        {
            _onCourseSelectHook = Functions.CourseSelectTask.Hook(OnCourseSelectTask).Activate();
            _onRaceSettingsHook = Functions.RaceSettingTask.Hook(OnRaceSettingsTask).Activate();
        }

        private unsafe int OnRaceSettingsTask()
        {
            if (_tracker.RaceRules != null)
            {
                var lastRule   = RuleSettingsSync.FromGame(_tracker.RaceRules);
                var result     = _onRaceSettingsHook.OriginalFunction();
                var rule       = RuleSettingsSync.FromGame(_tracker.RaceRules);
                Rule = lastRule.Delta(rule);

                if (!Rule.IsDefault())
                    OnRuleSettingsUpdated?.Invoke(Rule, _tracker.RaceRules);

                return result;
            }

            return _onRaceSettingsHook.OriginalFunction();
        }

        private unsafe int OnCourseSelectTask()
        {
            if (_tracker.CourseSelect != null)
            {
                var lastCourse = CourseSelectSync.FromGame(_tracker.CourseSelect);
                var result     = _onCourseSelectHook.OriginalFunction();
                var course     = CourseSelectSync.FromGame(_tracker.CourseSelect);
                Course         = lastCourse.GetDelta(course);

                if (!Course.IsDefault())
                    OnCourseSelectUpdated?.Invoke(Course, _tracker.CourseSelect);

                return result;
            }

            return _onCourseSelectHook.OriginalFunction();
        }

        public unsafe delegate void CourseSelectUpdated(CourseSelectLoop loop, Task<CourseSelect, CourseSelectTaskState>* task);
        public unsafe delegate void RuleSettingsUpdated(RuleSettingsLoop loop, Task<RaceRules, RaceRulesTaskState>* task);
    }
}
