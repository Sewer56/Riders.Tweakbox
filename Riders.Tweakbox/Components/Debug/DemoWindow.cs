﻿using DearImguiSharp;
namespace Riders.Tweakbox.Components.Debug;

public class DemoWindow : ComponentBase
{
    public override string Name { get; set; } = "Demo Window";

    public override void Render()
    {
        if (IsEnabled())
            ImGui.ShowDemoWindow(ref IsEnabled());
    }
}
