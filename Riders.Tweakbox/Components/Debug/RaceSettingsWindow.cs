using DearImguiSharp;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Gameplay;
namespace Riders.Tweakbox.Components.Debug;

public class RaceSettingsWindow : ComponentBase
{
    /// <inheritdoc />
    public override string Name { get; set; } = "Race Settings Viewer";

    /// <inheritdoc />
    public override unsafe void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            var settings = State.CurrentRaceSettings;
            ImGui.Text($"{nameof(RaceSettings.Laps)}: {settings->Laps}");
            ImGui.Text($"{nameof(RaceSettings.Announcer)}: {settings->Announcer}");
            ImGui.Text($"{nameof(RaceSettings.Item)}: {settings->Item}");
            ImGui.Text($"{nameof(RaceSettings.AirPit)}: {settings->AirPit}");
            ImGui.Text($"{nameof(RaceSettings.Ghost)}: {settings->Ghost}");
            ImGui.Text($"{nameof(RaceSettings.TimeLimit)}: {settings->TimeLimit}");
            ImGui.Text($"{nameof(RaceSettings.RunOnAirLoss)}: {settings->RunOnAirLoss}");
            ImGui.Text($"{nameof(RaceSettings.Unknown)}: {settings->Unknown}");
            ImGui.Text($"{nameof(RaceSettings.NumberOfPoints)}: {settings->NumberOfPoints}");
        }

        ImGui.End();
    }
}
