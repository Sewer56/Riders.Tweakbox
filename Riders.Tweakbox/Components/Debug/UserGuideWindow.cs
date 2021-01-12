using DearImguiSharp;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Debug
{
    public class UserGuideWindow : IComponent
    {
        public string Name { get; set; } = "User Guide";
        private bool _isEnabled;

        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() { }
        public void Enable() { }
        
        public void Render()
        {
            if (!_isEnabled) 
                return;
            
            if (ImGui.Begin(Name, ref _isEnabled, 0))
                ImGui.ShowUserGuide();

            ImGui.End();
        }
    }
}
