using System;
using DearImguiSharp;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Misc
{
    public class TaskTrackerWindow : IComponent
    {
        public string Name { get; set; } = "Task Tracker";
        public TaskTracker Tracker = IoC.GetConstant<TaskTracker>(); 
        private bool _isEnabled;

        public ref bool IsEnabled() => ref _isEnabled;

        public void Disable() { }
        public void Enable() { }

        public void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                RenderStateTracker();
            }

            ImGui.End();
        }

        private unsafe void RenderStateTracker()
        {
            ImGui.Text($"Last Task: {Tracker.LastTask}");
            if (Extensions.IsNotNull(Tracker.CharacterSelect))
                ImGui.Text($"{nameof(Tracker.CharacterSelect)}: {Tracker.CharacterSelect->TaskStatus}");

            if (Extensions.IsNotNull(Tracker.CourseSelect))
                ImGui.Text($"{nameof(Tracker.CourseSelect)}: {Tracker.CourseSelect->TaskStatus}");

            if (Extensions.IsNotNull(Tracker.Race))
                ImGui.Text($"{nameof(Tracker.Race)}: {Tracker.Race->TaskStatus}");

            if (Extensions.IsNotNull(Tracker.RaceRules))
                ImGui.Text($"{nameof(Tracker.RaceRules)}: {Tracker.RaceRules->TaskStatus}");

            if (Extensions.IsNotNull(Tracker.TitleSequence))
                ImGui.Text($"{nameof(Tracker.TitleSequence)}: {Tracker.TitleSequence->TaskStatus}");

            if (Extensions.IsNotNull(Tracker.MessageBox))
                ImGui.Text($"{nameof(Tracker.MessageBox)}: {Tracker.MessageBox->TaskStatus}");
        }
    }
}
