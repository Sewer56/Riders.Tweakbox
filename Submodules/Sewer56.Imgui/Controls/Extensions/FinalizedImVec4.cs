using DearImguiSharp;
namespace Sewer56.Imgui.Controls.Extensions;

/// <summary>
/// Instance of <see cref="ImVec4"/> with a finalizer.
/// </summary>
public class FinalizedImVec4 : ImVec4
{
    ~FinalizedImVec4() => Dispose();
}
