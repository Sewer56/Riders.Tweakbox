using DearImguiSharp;
using Reloaded.Hooks.Definitions;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Misc;
using Sewer56.SonicRiders.Functions;

namespace Riders.Tweakbox.Components.Debug;

public unsafe class AFSTester : ComponentBase
{
    public override string Name { get; set; } = "AFS Tester";

    private int _index = 0;
    private void** _voiceAdxtHandle = (void**)0x017E3A14;

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            RenderMenu();
        }

        ImGui.End();
    }

    private void RenderMenu()
    {
        Reflection.MakeControl(ref _index, "Sound Index", 0.1f, 0, 100);
        if (ImGui.Button("Play Voice", Constants.ButtonSize))
        {
            Functions.PlayAfsSound.GetWrapper()(*_voiceAdxtHandle, 0, _index);
        }
    }
}