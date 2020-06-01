using DearImguiSharp;
using Riders.Tweakbox.Definitions.Interfaces;

namespace Riders.Tweakbox.Components.Imgui
{
    public class DemoWindow : IComponent
    {
        public string Name { get; set; } = "Demo Window";

        public void Disable() { }
        public void Enable()  { }
        public void Render(ref bool compEnabled)
        {
            if (compEnabled)
                ImGui.ShowDemoWindow(ref compEnabled);
        }
    }
}
