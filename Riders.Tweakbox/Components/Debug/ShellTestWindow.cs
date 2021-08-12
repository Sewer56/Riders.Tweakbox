using System.Threading.Tasks;
using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;

namespace Riders.Tweakbox.Components.Debug
{
    public class ShellTestWindow : ComponentBase
    {
        public override string Name { get; set; } = "Shell Test Window";
        private string TestDialogId => "Cool Test Dialog";
        private string TestWindowId => "Cool Test Window";

        public override void Render()
        {
            if (ImGui.Begin(Name, ref IsEnabled(), 0))
            {
                if (ImGui.Button("Shell Dialog Manual", Constants.DefaultVector2)) 
                    Shell.AddCustom(TestManualDialog);

                if (ImGui.Button("Shell Dialog Auto", Constants.DefaultVector2))
                    Shell.AddDialog(TestDialogId, TestDialog);

                if (ImGui.Button("Shell Dialog Auto Async", Constants.DefaultVector2))
                    Shell.AddDialogAsync(TestDialogId, TestDialog)
                        .ContinueWith(task => Shell.AddDialog(TestDialogId, TestDialogAsync));

                if (ImGui.Button("Shell Dialog Text", Constants.DefaultVector2))
                    Shell.AddDialog(TestDialogId, "Test Dialog Text");

                if (ImGui.Button("Shell Dialog Text Async", Constants.DefaultVector2))
                    Shell.AddDialogAsync(TestDialogId, "This Dialog Uses Async/Await")
                        .ContinueWith(task => Shell.AddDialog(TestDialogId, "And this dialog is a continuation of the last task!"));

                if (ImGui.Button("Shell Window Auto", Constants.DefaultVector2))
                    Shell.AddWindow(TestWindowId, TestDialog);

                if (ImGui.Button("Shell Window Auto Async", Constants.DefaultVector2))
                    Shell.AddWindowAsync(TestWindowId, TestDialogAsync)
                        .ContinueWith(task => Shell.AddWindow(TestWindowId, (ref bool opened) => ImGui.TextWrapped("Continuation Task")));
            }

            ImGui.End();
        }

        private void TestDialog(ref bool isOpened)
        {
            ImGui.Text("Test Dialog");
        }

        private void TestDialogAsync(ref bool isOpened)
        {
            ImGui.Text("Test Dialog Async Continuation");
        }

        private bool TestManualDialog()
        {
            bool isOpened = true;
            ImGui.OpenPopupStr(TestDialogId, 0);
            if (ImGui.BeginPopupModal(TestDialogId, ref isOpened, (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            {
                ImGui.Text("Test Manual Dialog");
                ImGui.EndPopup();
            }

            return isOpened;
        }
    }
}