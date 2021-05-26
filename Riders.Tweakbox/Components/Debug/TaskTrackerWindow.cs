using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;

namespace Riders.Tweakbox.Components.Debug
{
    public class TaskTrackerWindow : ComponentBase
    {
        public override string Name { get; set; } = "Task Tracker";
        public EventController Events = IoC.Get<EventController>(); 

        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                RenderStateTracker();
            }

            ImGui.End();
        }

        private unsafe void RenderStateTracker()
        {
            ImGui.Text($"Last Task: {Events.LastTask}");
            if (Pointers.IsNotNull(Events.CharacterSelect))
                ImGui.Text($"{nameof(Events.CharacterSelect)}: {Events.CharacterSelect->TaskStatus}");

            if (Pointers.IsNotNull(Events.CourseSelect))
                ImGui.Text($"{nameof(Events.CourseSelect)}: {Events.CourseSelect->TaskStatus}");

            if (Pointers.IsNotNull(Events.Race))
                ImGui.Text($"{nameof(Events.Race)}: {Events.Race->TaskStatus}");

            if (Pointers.IsNotNull(Events.RaceRules))
                ImGui.Text($"{nameof(Events.RaceRules)}: {Events.RaceRules->TaskStatus}");

            if (Pointers.IsNotNull(Events.TitleSequence))
                ImGui.Text($"{nameof(Events.TitleSequence)}: {Events.TitleSequence->TaskStatus}");

            if (Pointers.IsNotNull(Events.MessageBox))
                ImGui.Text($"{nameof(Events.MessageBox)}: {Events.MessageBox->TaskStatus}");
        }
    }
}
