using System;
using DearImguiSharp;
namespace Sewer56.Imgui.Controls.Extensions;

/// <summary>
/// Instance of <see cref="ImVec4"/> with a finalizer.
/// </summary>
public class FinalizedImVec4 : ImVec4
{
    /// <inheritdoc />
    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        GC.SuppressFinalize(this);
    }

    ~FinalizedImVec4() => Dispose();
}
