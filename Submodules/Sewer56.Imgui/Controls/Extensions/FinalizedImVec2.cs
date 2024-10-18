using DearImguiSharp;
namespace Sewer56.Imgui.Controls.Extensions;

/// <summary>
/// Instance of <see cref="ImVec4"/> with a finalizer.
/// </summary>
public class FinalizedImVec2 : ImVec2
{
    ~FinalizedImVec2() => Dispose();
}
