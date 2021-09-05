using Sewer56.Imgui.Layout;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Utilities;
using System.Collections.Generic;
using DearImguiSharp;
using Riders.Tweakbox.Misc.Extensions;
using static DearImguiSharp.ImGuiWindowFlags;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Main;

public class OpenSourceLibraries : ComponentBase
{
    public override string Name { get; set; } = "Open Source Libraries";

    private InformationWindow _informationWindow = new InformationWindow("About Menu", Pivots.Pivot.Center, Pivots.Pivot.Center, false);
    private List<HorizontalCenterHelper> _centerHelpers = new List<HorizontalCenterHelper>();
    private IO _io;

    public OpenSourceLibraries(IO io)
    {
        _io = io;
        _informationWindow.WindowFlags &= ~ImGuiWindowFlagsNoInputs;
        _informationWindow.Size.X = 500;
    }

    public override void Render()
    {
        if (!Enabled)
            return;

        _informationWindow.Begin();
        RenderContents();
        _informationWindow.End();
    }

    private void RenderContents()
    {
        ImGui.Text("Third Party Library Listing");

        RenderLicense("Test Data Randomization", "Bogus");
        RenderLicense("Formatting & Manipulating File Sizes", "ByteSize");
        RenderLicense("Testing Code Coverage", "Coverlet");
        RenderLicense("User Interface", "DearImgui");
        RenderLicense("Discord Rich Presence", "DiscordRichPresence");
        RenderLicense("Extension Methods for Enums", "Enums.NET");
        RenderLicense("IL Assembly Weaving/Post Processing & Add-ins", "Fody");
        RenderLicense("Object Mapping Acceleration", "Fast Expression Compiler");
        RenderLicense("LZ4 Compression Implementation", "K4os.Compression.LZ4");
        RenderLicense("Fast Object Mappeing", "Mapster");
        RenderLicense("MessagePack Serialization Implementation", "MessagePack");
        RenderLicense("Dependency Injection", "Ninject");
        RenderLicense("F# Style Discriminated Unions", "OneOf");
        RenderLicense("Resilience & Fault-Handling", "Polly");
        RenderLicense("DirectX 9.0 Wrapper APIs", "SharpDX");
        RenderLicense("Base85 Encoding", "SimpleBase");
        RenderLicense("xxHash Hashing Implementation", "Standart.Hash.xxHash");
        RenderLicense("Allocation Free LINQ", "StructLinq");
        RenderLicense("Unit Testing Framework", "xUnit");
    }

    private void RenderLicense(string licenseDescription, string libraryName)
    {
        if (ImGui.TreeNodeStr($"{libraryName}: {licenseDescription}"))
        {
            ImGui.TextWrapped(_io.GetLicenseFile(libraryName));
            ImGui.TreePop();
        }
    }
}
