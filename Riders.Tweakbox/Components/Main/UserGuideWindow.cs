using DearImguiSharp;
namespace Riders.Tweakbox.Components.Main;

public class UserGuideWindow : ComponentBase
{
    public override string Name { get; set; } = "Menu Help";

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
            ImGui.ShowUserGuide();

        ImGui.End();
    }
}
