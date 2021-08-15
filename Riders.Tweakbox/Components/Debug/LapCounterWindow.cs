using DearImguiSharp;
using Sewer56.SonicRiders.API;
namespace Riders.Tweakbox.Components.Debug;

public class LapCounterWindow : ComponentBase
{
    /// <inheritdoc />
    public override string Name { get; set; } = "Lap Counter Viewer";

    /// <inheritdoc />
    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            for (int x = 0; x < Riders.Netplay.Messages.Misc.Constants.MaxRidersNumberOfPlayers; x++)
                ImGui.TextWrapped($"Player {x}: {Player.Players[x].LapCounter}");
        }

        ImGui.End();
    }
}
