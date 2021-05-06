using System;
using System.Diagnostics;
using DearImguiSharp;
using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiWindowFlags;

namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Renders the first time welcome screen.
    /// </summary>
    public class WelcomeScreenRenderer : IDisposable
    {
        private const string Title = "Welcome!";

        private HorizontalCenterHelper _centerHelper = new HorizontalCenterHelper();
        private ImVec4 _accentColor = Utilities.HexToFloat(0xef5350ff);
        private ImVec4 _hyperlinkColor = Utilities.HexToFloat(0x42a5f5ff);
        
        ~WelcomeScreenRenderer() => Dispose();

        /// <inheritdoc />
        public void Dispose()
        {
            _hyperlinkColor?.Dispose(true);
            _accentColor?.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Pass this function to <see cref="Shell.AddCustom"/>.
        /// </summary>
        public unsafe bool RenderFirstTimeDialog()
        {
            bool isOpened = true;
            
            ImGui.OpenPopup(Title, (int) ImGuiPopupFlags.ImGuiPopupFlagsNoOpenOverExistingPopup);
            ImGui.__Internal.SetNextWindowSize(new ImVec2.__Internal() { x = 400, y = 0 }, (int) ImGuiCond.ImGuiCondAlways);
            if (ImGui.BeginPopupModal(Title, ref isOpened, (int) (ImGuiWindowFlagsAlwaysAutoResize | ImGuiWindowFlagsNoTitleBar | ImGuiWindowFlagsNoSavedSettings)))
            {
                // Title
                ImGui.TextColored(_accentColor, "Ohayou !");
                ImGui.Spacing();

                // Text
                ImGui.TextWrapped("This is probably your first time using Tweakbox.");
                ImGui.Spacing();

                ImGui.TextWrapped("Please note that this project is an active work in progress (alpha build). Features like Netplay can be incomplete, buggy and prone to crashing.");
                ImGui.Spacing();

                ImGui.PushStyleColorVec4((int) ImGuiCol.ImGuiColText, _hyperlinkColor);
                ImGui.TextWrapped("Please report crashes and issues using the guidelines provided in the documentation.");
                if (ImGui.IsItemClicked((int) ImGuiMouseButton.ImGuiMouseButtonLeft))
                    Process.Start(new ProcessStartInfo("cmd", $"/c start https://sewer56.dev/Riders.Tweakbox/") { CreateNoWindow = true });

                ImGui.PopStyleColor(1);

                // Footer
                ImGui.Spacing();
                ImGui.Text("And most of all, remember to ");
                ImGui.SameLine(0, 0);
                ImGui.TextColored(_accentColor, "have fun!");

                // Render OK Button
                ImGui.Spacing();
                _centerHelper.Begin();
                if (ImGui.Button("OK", Sewer56.Imgui.Misc.Constants.ButtonSizeThin))
                    isOpened = false;

                _centerHelper.End();
                ImGui.EndPopup();
            }

            if (!isOpened)
                Dispose();

            return isOpened;
        }
    }
}
