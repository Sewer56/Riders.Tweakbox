using System;
using System.Numerics;
using DearImguiSharp;
using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Utilities;
namespace Sewer56.Imgui.Shell;

/// <summary>
/// Encapsulates a simple, non-interactive information window that self resizes to meet the necessary content size.
/// </summary>
public class InformationWindow : IDisposable
{
    /// <summary>
    /// The flags used for rendering this window.
    /// </summary>
    public ImGuiWindowFlags WindowFlags = (ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs |
                                           ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings |
                                           ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoNav |
                                           ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing);

    /// <summary>
    /// The name of the window.
    /// </summary>
    public string Name;

    /// <summary>
    /// True if the window is open, else false.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set => _isOpen = value;
    }

    /// <summary>
    /// The padding of the window relative to the chosen edge/corner of the window.
    /// If centered, this has no effect on the window.
    /// </summary>
    public Vector2 Padding = new Vector2(5, 5);

    /// <summary>
    /// Minimum size of the window.
    /// A value of 0 in an axis indicates infinite.
    /// </summary>
    public FinalizedImVec2 Size = new FinalizedImVec2() { X = 0, Y = 0 };

    /// <summary>
    /// Position in the window against which the rest of the window pivots.
    /// </summary>
    public FinalizedImVec2 WindowPivot = new FinalizedImVec2() { X = 0, Y = 0 };

    /// <summary>
    /// Corner/edge relative to which the window is positioned.
    /// </summary>
    public Pivots.Pivot PositionPivot;

    /// <summary>
    /// Contains the size of the last window/frame.
    /// </summary>
    public FinalizedImVec2 LastWindowSize { get; private set; } = new FinalizedImVec2();

    private bool _isOpen;
    private ImVec2 _nextWindowPos = new ImVec2();

    /// <param name="name">Name of the window.</param>
    /// <param name="positionPivot">Corner/edge relative to which the window is positioned.</param>
    /// <param name="windowPivot">Position in the window against which the rest of the window pivots.</param>
    /// <param name="isOpen">Whether the window is open or not.</param>
    public InformationWindow(string name, Pivots.Pivot positionPivot, Pivots.Pivot windowPivot, bool isOpen = true)
    {
        Name = name;
        SetPivot(positionPivot, windowPivot);
        _isOpen = isOpen;
    }

    /// <param name="name">Name of the window.</param>
    /// <param name="positionPivot">Corner/edge relative to which the window is positioned.</param>
    /// <param name="windowPivot">Position in the window against which the rest of the window pivots.</param
    /// <param name="isOpen">Whether the window is open or not.</param>
    public InformationWindow(string name, Pivots.Pivot positionPivot, ImVec2 windowPivot, bool isOpen = true)
    {
        Name = name;
        PositionPivot = positionPivot;
        WindowPivot.X = windowPivot.X;
        WindowPivot.Y = windowPivot.Y;
        _isOpen = isOpen;
    }

    /// <summary>
    /// Adjusts the pivot of the current window.
    /// </summary>
    /// <param name="positionPivot">Corner/edge relative to which the window is positioned.</param>
    /// <param name="windowPivot">Position in the window against which the rest of the window pivots.</param>
    public void SetPivot(Pivots.Pivot positionPivot, Pivots.Pivot windowPivot)
    {
        PositionPivot = positionPivot;
        var point = Pivots.GetPoint(windowPivot);
        point.ToImVec(WindowPivot);
    }

    /// <summary>
    /// Call this to begin rendering the window.
    /// </summary>
    public void Begin()
    {
        SetWindowPosition();
        ImGui.SetNextWindowSize(Size, (int)ImGuiCond.Always);
        ImGui.Begin(Name, ref _isOpen, (int)WindowFlags);
    }

    /// <summary>
    /// Call this to end rendering the window.
    /// </summary>
    public void End()
    {
        ImGui.End();
        ImGui.GetWindowSize(LastWindowSize);
    }

    private void SetWindowPosition()
    {
        var io = Shell.IO;
        var displaySize = io.DisplaySize;
        var frameBufferScale = io.DisplayFramebufferScale;
        var scaledPadding = new Vector2(Padding.X * frameBufferScale.X, Padding.Y * frameBufferScale.Y);

        // Add padding.
        switch (PositionPivot)
        {
            case Pivots.Pivot.TopLeft:
                _nextWindowPos.X = scaledPadding.X;
                _nextWindowPos.Y = scaledPadding.Y;
                break;
            case Pivots.Pivot.Top:
                _nextWindowPos.X = displaySize.X / 2;
                _nextWindowPos.Y = scaledPadding.Y;
                break;
            case Pivots.Pivot.TopRight:
                _nextWindowPos.X = displaySize.X - scaledPadding.X;
                _nextWindowPos.Y = scaledPadding.Y;
                break;
            case Pivots.Pivot.Left:
                _nextWindowPos.X = scaledPadding.X;
                _nextWindowPos.Y = displaySize.Y / 2;
                break;
            case Pivots.Pivot.Center:
                _nextWindowPos.X = displaySize.X / 2;
                _nextWindowPos.Y = displaySize.Y / 2;
                break;
            case Pivots.Pivot.Right:
                _nextWindowPos.X = displaySize.X - scaledPadding.X;
                _nextWindowPos.Y = displaySize.Y / 2;
                break;
            case Pivots.Pivot.BottomLeft:
                _nextWindowPos.X = scaledPadding.X;
                _nextWindowPos.Y = displaySize.Y - scaledPadding.Y;
                break;
            case Pivots.Pivot.Bottom:
                _nextWindowPos.X = displaySize.X / 2;
                _nextWindowPos.Y = displaySize.Y - scaledPadding.Y;
                break;
            case Pivots.Pivot.BottomRight:
                _nextWindowPos.X = displaySize.X - scaledPadding.X;
                _nextWindowPos.Y = displaySize.Y - scaledPadding.Y;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ImGui.SetNextWindowPos(_nextWindowPos, (int)ImGuiCond.Always, WindowPivot);
    }

    ~InformationWindow() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        Size?.Dispose();
        LastWindowSize?.Dispose();
        WindowPivot?.Dispose();
        _nextWindowPos?.Dispose();
        GC.SuppressFinalize(this);
    }
}
