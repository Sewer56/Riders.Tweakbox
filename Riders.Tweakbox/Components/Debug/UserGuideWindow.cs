using DearImguiSharp;
namespace Riders.Tweakbox.Components.Debug;

public class UserGuideWindow : ComponentBase
{
    public override string Name { get; set; } = "User Guide";

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
            ImGui.ShowUserGuide();

        ImGui.End();
    }
}
