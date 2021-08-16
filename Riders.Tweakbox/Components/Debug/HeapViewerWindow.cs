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

    public override void Render()
    {
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
            RenderHeapViewer();

        ImGui.End();
    }

    private unsafe void RenderHeapViewer()
    {
        ImGui.TextWrapped("This utility allows you to view objects allocated on the game's native heap. It's a work in progress, sometimes it doesn't work quite right :/");
        ImGui.TextWrapped("The objects on the front of the buffer are long lived objects, while the ones at the back of the buffer are short lived objects.");
        if (ImGui.CollapsingHeaderTreeNodeFlags("Front", 0))
            IterateFront(_controller.FirstAllocResult);

        if (ImGui.CollapsingHeaderTreeNodeFlags("Back", 0))
            IterateBack(*Heap.FirstHeaderBack);
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

    private unsafe void IterateBack(MemoryHeapHeaderHigh* header)
    {
        // Check for error cases.
        if (header == (void*)0)
            return;

        // Else iterate over all objects.
        int objectCount = 0;

        while (true)
        {
            ImGui.TextWrapped($"[0x{(long)header:X}] Object: {objectCount}, Next Size: {ByteSize.FromBytes(header->GetSize(header))}");

            if (header->NextItem == (void*) 0)
                break;

            header = header->NextItem;
            objectCount++;
        }
    }
}
