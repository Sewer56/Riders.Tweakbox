using DearImguiSharp;
using Riders.Tweakbox.Definitions.Interfaces;

namespace Riders.Tweakbox.Components.Imgui
{
    public class UserGuideWindow : IComponent
    {
        public string Name { get; set; } = "User Guide";

        public void Disable() { }
        public void Enable() { }
        public void Render(ref bool compEnabled)
        {
            if (!compEnabled) 
                return;
            
            if (ImGui.Begin(Name, ref compEnabled, 0))
            {
                ImGui.ShowUserGuide();
                ImGui.End();
            }
        }
    }
}
