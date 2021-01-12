using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Debug
{
    public class ShellTestWindow : IComponent
    {
        public string Name { get; set; } = "Shell Test Window";
        private string TestDialogId => "Cool Test Dialog";
        private string TestWindowId => "Cool Test Window";
        private bool _isEnabled;

        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() { }
        public void Enable() { }

        public void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                if (ImGui.Button("Shell Dialog Manual", Constants.DefaultVector2)) 
                    Shell.AddCustom(TestManualDialog);

                if (ImGui.Button("Shell Dialog Auto", Constants.DefaultVector2))
                    Shell.AddDialog(TestDialogId, TestDialog);

                if (ImGui.Button("Shell Dialog Text", Constants.DefaultVector2))
                    Shell.AddDialog(TestDialogId, "Arg is cute");

                if (ImGui.Button("Shell Window Auto", Constants.DefaultVector2))
                    Shell.AddWindow(TestWindowId, TestDialog);
            }

            ImGui.End();
        }

        private void TestDialog(ref bool isOpened)
        {
            ImGui.Text("Protag is Pepega");
        }

        private bool TestManualDialog()
        {
            bool isOpened = true;
            ImGui.OpenPopup(TestDialogId);
            if (ImGui.BeginPopupModal(TestDialogId, ref isOpened, (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize)) 
                ImGui.Text("Protag is Pepega");

            ImGui.EndPopup();
            ImGui.CloseCurrentPopup();
            return isOpened;
        }
    }
}