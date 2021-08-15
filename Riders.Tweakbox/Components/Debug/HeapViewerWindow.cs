using ByteSizeLib;
using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Functions;
namespace Riders.Tweakbox.Components.Debug;

public class HeapViewerWindow : ComponentBase
{
    public override string Name { get; set; } = "Heap Viewer";
    private HeapController _controller = IoC.GetSingleton<HeapController>();
    private bool _frontOpen;

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
            RenderHeapViewer();

        ImGui.End();
    }

    private unsafe void RenderHeapViewer()
    {
        ImGui.TextWrapped("This utility allows you to view objects allocated on the game's native heap. It's a work in progress, sometimes it doesn't work quite right :/");
        if (ImGui.CollapsingHeaderBoolPtr("Front", ref _frontOpen, 0))
            IterateFront(_controller.FirstAllocResult);
    }

    private unsafe void IterateFront(MallocResult* result)
    {
        // Check for error cases.
        if (result == (void*)0 || result == *Heap.FirstHeaderFront)
            return;

        // Else iterate over all objects.
        int objectCount = 0;
        var header = result->GetHeader(result);
        var headFront = *Heap.FrameHeadFront;

        while (true)
        {
            ImGui.TextWrapped($"[0x{(long)header:X}] Object: {objectCount}, Base: {(long)header->Base:X} Size: {ByteSize.FromBytes(header->AllocationSize)}");
            header = header->GetNextItem();

            var expectedBase = (void*)header;

            // Due to alignment, we might not have the right address, keep moving the address
            // until the base is what we expect.
            while (header->Base != expectedBase && header < headFront)
                header = (MemoryHeapHeader*)(((byte*)header) + 1);

            if (header >= headFront)
                break;

            objectCount++;
        }
    }
}
