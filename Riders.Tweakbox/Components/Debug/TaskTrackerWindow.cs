using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell.Interfaces;
using Sewer56.SonicRiders.Utility;

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
            if (Extensions.IsNotNull(Events.CharacterSelect))
                ImGui.Text($"{nameof(Events.CharacterSelect)}: {Events.CharacterSelect->TaskStatus}");

            if (Extensions.IsNotNull(Events.CourseSelect))
                ImGui.Text($"{nameof(Events.CourseSelect)}: {Events.CourseSelect->TaskStatus}");

            if (Extensions.IsNotNull(Events.Race))
                ImGui.Text($"{nameof(Events.Race)}: {Events.Race->TaskStatus}");

            if (Extensions.IsNotNull(Events.RaceRules))
                ImGui.Text($"{nameof(Events.RaceRules)}: {Events.RaceRules->TaskStatus}");

            if (Extensions.IsNotNull(Events.TitleSequence))
                ImGui.Text($"{nameof(Events.TitleSequence)}: {Events.TitleSequence->TaskStatus}");

            if (Extensions.IsNotNull(Events.MessageBox))
                ImGui.Text($"{nameof(Events.MessageBox)}: {Events.MessageBox->TaskStatus}");
        }
    }
}
